using AutoMapper;
using DAL;
using DAL.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PMTool.Authorization;
using PMTool.Hubs;
using PMTool.Models.General;
using PMTool.Models.Services;
using PMTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PMTool
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            services.AddDbContext<PMToolDbContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //MAPPER DI

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new Mappers.AutoMapper());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            //MAPPER DI END


            //REPOSITORY DI
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            //APPSERVICES DI
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ITeamService, TeamService>();
            services.AddTransient<IProjectService, ProjectService>();
            services.AddTransient<ICommonService, CommonService>();
            services.AddTransient<IChallengeService, ChallengeService>();
            services.AddTransient<ICardService, CardService>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<IFaqService, FaqService>();
            services.AddTransient<IReportService, ReportService>();

            //CORS DI
            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options =>
                options.AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithOrigins("http://localhost:4200", "http://sandbox.aimviz.com:6333", "http://sandbox:6333"));
            });
            services.AddSignalR();


            //JWT DI
            var key = Encoding.ASCII.GetBytes(Configuration["Authentication:JwtBearer:SecurityKey"]);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = false,

                // Allow to use seconds for expiration of token
                // Required only when token lifetime less than 5 minutes
                // THIS ONE
                ClockSkew = TimeSpan.Zero
            };

            services.AddSingleton(tokenValidationParameters);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Authentication:JwtBearer:Issuer"],
                        ValidAudience = Configuration["Authentication:JwtBearer:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:JwtBearer:SecurityKey"]))
                    };
                });

            //JWT DI END

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ActiveUserPolicy", policy => policy.Requirements.Add(new ActiveUserAuthorizationRequirement()));
            });

            services.AddScoped<IAuthorizationHandler, ActiveUserAuthorizationHandler>();

            //SWAGGER DI
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PMTool", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            Constants.ContentRootPath = env.ContentRootPath;
            loggerFactory.AddFile(env.ContentRootPath + "\\Logs\\log.txt");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PMTool v1"));


            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(env.ContentRootPath, "Files")),
                RequestPath = "/Files"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(env.ContentRootPath, "Templates")),
                RequestPath = "/Templates"
            });

            app.UseRouting();

            app.UseCors("AllowOrigin");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/notificationHub", opts =>
                {

                });
            });
        }
    }


}
