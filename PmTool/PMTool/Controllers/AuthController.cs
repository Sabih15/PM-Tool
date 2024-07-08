using PMTool.Resources.Response;
using PMTool.Resources.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Repository;
using DAL.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using PMTool.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using PMTool.Models.Response;
using PMTool.Models.DTOs;
using Microsoft.AspNetCore.Hosting;
using PMTool.Models.Request;
using AutoMapper;
using static PMTool.General.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PMTool.Models.General;
using PMTool.Authorization;
using PMTool.Services;

namespace PMTool.Controllers
{
    public class AuthController : BaseController
    {
        #region Fields

        private readonly IConfiguration config;
        private readonly IRepository<User> userRepository;
        private readonly IRepository<Role> roleRepository;
        private readonly IRepository<RefreshToken> refreshTokenRepository;
        private readonly IRepository<TemporaryProjectMember> tempProjectRepository;
        private readonly IRepository<TemporaryTeamMember> tempTeamRepository;
        private readonly IRepository<ProjectMember> projectMemberRepository;
        private readonly IRepository<TeamMember> teamMemberRepository;
        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly TokenValidationParameters tokenValidationParameters;
        private readonly ILogger<AuthController> logger;
        private readonly IWebHostEnvironment environment;

        #endregion

        #region Constructors

        public AuthController(
            IRepository<User> _userRepository,
            IRepository<Role> _roleRepository,
            IRepository<RefreshToken> _refreshTokenRepository,
            IRepository<TemporaryProjectMember> _tempProjectRepository,
            IRepository<TemporaryTeamMember> _tempTeamRepository,
            IRepository<ProjectMember> _projectMemberRepository,
            IRepository<TeamMember> _teamMemberRepository,
            IUserService _userService,
            IConfiguration _config,
            ILogger<AuthController> _logger,
            IWebHostEnvironment _environment,
            IMapper _mapper,
            TokenValidationParameters _tokenValidationParameters
            )
        {
            config = _config;
            userRepository = _userRepository;
            roleRepository = _roleRepository;
            logger = _logger;
            environment = _environment;
            refreshTokenRepository = _refreshTokenRepository;
            tempProjectRepository = _tempProjectRepository;
            tempTeamRepository = _tempTeamRepository;
            projectMemberRepository = _projectMemberRepository;
            teamMemberRepository = _teamMemberRepository;
            userService = _userService;
            mapper = _mapper;
            tokenValidationParameters = _tokenValidationParameters;
        }

        #endregion

        #region Methods

        #region Private

        /// <summary>
        /// Gets Current Logged-in UserId
        /// </summary>
        /// <returns></returns>
        private int? GetCurrentUserId()
        {
            string currentUserId = JwtClaimAccessor.GetClaimByType(ClaimTypes.NameIdentifier, HttpContext);
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var user = userService.GetUserByPublicId(currentUserId);
                return user.UserId;
            }
            return null;
        }

        /// <summary>
        /// Verify JWT token and if it is valid then store a refresh token in the database.
        /// </summary>
        /// <param name="refreshTokenReq"></param>
        /// <returns></returns>
        private GeneralResponse VerifyToken(RefreshTokenReq refreshTokenReq)
        {
            GeneralResponse response = new GeneralResponse();
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // This validation function will make sure that the token meets the validation parameters
                // and its an actual jwt token not just a random string
                var principal = jwtTokenHandler.ValidateToken(refreshTokenReq.Token, tokenValidationParameters, out var validatedToken);

                // Now we need to check if the token has a valid security algorithm
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (result == false)
                    {
                        return null;
                    }
                }

                // Will get the time stamp in unix time
                var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                // we convert the expiry date from seconds to the date
                var expDate = UnixTimeStampToDateTime(utcExpiryDate);

                if (expDate > DateTime.UtcNow)
                {
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.DefaultErrorMsg);
                    return response;
                }

                // Check the token we got if its saved in the db
                var storedRefreshToken = refreshTokenRepository.GetAll().FirstOrDefault(x => x.Token == refreshTokenReq.RefreshToken);

                if (storedRefreshToken == null)
                {
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.InvalidInformation);
                    return response;
                }

                // Check the date of the saved token if it has expired
                if (DateTime.UtcNow > storedRefreshToken.ExpiryDate || storedRefreshToken.IsUsed || storedRefreshToken.IsRevoked)
                {
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.TokenExpired);
                    return response;
                }

                // we are getting here the jwt token id
                var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                // check the id that the recieved token has against the id saved in the db
                if (storedRefreshToken.JwtId != jti)
                {
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.InvalidInformation);
                    return response;
                }

                storedRefreshToken.IsUsed = true;
                storedRefreshToken.UpdatedBy = GetCurrentUserId();
                refreshTokenRepository.Update(storedRefreshToken);

                var dbUser = userRepository.Get(storedRefreshToken.UserId).Result;
                return GenerateToken(dbUser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                return null;
            }
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        /// <summary>
        /// Generates JWT Token and refresh token for the user.
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        private GeneralResponse GenerateToken(User userInfo)
        {
            var SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Authentication:JwtBearer:SecurityKey"]));
            var signingCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserPublicId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, DateTime.Now.AddHours(24).ToString())
            };

            //var refreshToken = new 

            var token = new JwtSecurityToken(config["Authentication:JwtBearer:Issuer"],
                config["Authentication:JwtBearer:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: signingCredentials);
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);


            var refreshToken = new RefreshToken()
            {
                JwtId = (token.Id).Trim(),
                IsUsed = false,
                UserId = userInfo.UserId,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                IsRevoked = false,
                Token = (Helper.GetRandomAplhanumericString(20) + Guid.NewGuid()).Trim(),
            };


            refreshToken.CreatedBy = GetCurrentUserId();

            refreshTokenRepository.Insert(refreshToken);

            var result = new AuthResult()
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token
            };

            var res = new GeneralResponse();
            res.Data = result;
            GeneralResponse.SetResponse(res, Helper.ResponseEnum.GetSuccess);


            return res;
        }

        /// <summary>
        /// Registers a new user for member signed-up with social login feature.
        /// </summary>
        /// <param name="socialUser"></param>
        /// <returns></returns>
        private async Task<User> RegisterSocialUser(SocialLoginReq socialUser)
        {
            try
            {
                var role = await roleRepository.GetAll().FirstOrDefaultAsync(s => s.RoleName.ToLower() == "user");
                var user = mapper.Map<User>(socialUser);
                user.RoleId = role.RoleId;
                user = userRepository.Insert(user);

                //CHECK IF THIS USER HAS ANY INVITES
                await CheckProjectnTeam(user.Email, user.UserId);

                return user;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                return null;
            }
        }

        private async Task<bool> SendVerificationEmail(User user) //fix
        {
            var res = await CustomEmail<AuthController>.SendVerificationEmail(user.FullName, user.Email, user.ResetCode, logger, config, environment);
            return res;
        }

        private async Task CheckProjectnTeam(string email, int userId)//fix
        {
            try
            {
                //TEAM
                var teams = await tempTeamRepository.GetAll().Include(s => s.Team).Where(s => s.Email == email).ToListAsync();
                if (teams != null && teams.Count > 0)
                //Add Team Member
                {
                    foreach (var team in teams)
                    {
                        var member = new TeamMember
                        {
                            CreatedBy = team.Team.CreatedBy,
                            MemberUserId = userId,
                            TeamId = team.TeamId
                        };
                        await teamMemberRepository.InsertAsync(member);

                        tempTeamRepository.HardDelete(team);
                    }
                }


                //PROJECTS
                var projects = await tempProjectRepository.GetAll().Include(s => s.Project).Where(s => s.Email == email).ToListAsync();
                if (projects != null && projects.Count > 0)
                //Add Project Member
                {
                    foreach (var project in projects)
                    {
                        var member = new ProjectMember
                        {
                            CreatedBy = project.Project.CreatedBy,
                            ProjectMemberUserId = userId,
                            ProjectId = project.ProjectId
                        };
                        await projectMemberRepository.InsertAsync(member);

                        tempProjectRepository.HardDelete(project);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        private Dictionary<string, object> GenerateLoginResponseData(User user)
        {
            var token = GenerateToken(user);
            var permissionAlias = userService.GetRolePermissions(user.RoleId.Value);
            Dictionary<string, Object> dict = new Dictionary<string, object>();
            dict.Add("token", token.Data);
            var userObj = mapper.Map<UserDto>(user);
            if(!string.IsNullOrEmpty(userObj.Picture))
            {
                userObj.Picture = config.GetValue<string>("ServerUrl") + userObj.Picture;
            }
            dict.Add("user", userObj);
            dict.Add("permissions", permissionAlias);
            return dict;
        }

        #endregion

        #region Public

        [HttpPost("RefreshToken")]
        public GeneralResponse RefreshToken([FromBody] RefreshTokenReq tokenRequest)
        {
            GeneralResponse response = new GeneralResponse();
            if (ModelState.IsValid)
            {
                var res = VerifyToken(tokenRequest);

                if (res == null)
                {
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.InvalidInformation);
                }
                else
                {
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.GetSuccess);
                }

            }

            GeneralResponse.SetResponse(response, Helper.ResponseEnum.InvalidInformation);
            return response;
        }

        [Authorize(Policy = "ActiveUserPolicy")]
        [HttpGet("VerifyToken")]
        public void VerifyToken()
        {

        }

        [HttpPost("SocialLogin")]
        public async Task<GeneralResponse> SocialLogin(SocialLoginReq socialLoginReq)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var user = userService.GetUserByEmail(socialLoginReq.email); //fix query

                if (user == null)
                {
                    user = await RegisterSocialUser(socialLoginReq);
                    if (user == null)
                    {
                        GeneralResponse.SetResponse(response, Helper.ResponseEnum.DefaultErrorMsg);
                        return response;
                    }
                }
                else if (!user.IsSocialUser)
                {
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.NotASocialUser);
                    return response;
                }
                var result = mapper.Map<UserDto>(user);
                var dict = GenerateLoginResponseData(user);
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.CreateSuccess); //fix
                response.Data = dict;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("Login")]
        public async Task<GeneralResponse> Login(LoginReq authenticationRequest)
        {
            GeneralResponse response = new GeneralResponse();

            try
            {
                var encPassword = EncryptDecrypt.Encrypt(authenticationRequest.Password); //fix
                var user = userRepository.GetAll().Include(s => s.Role).FirstOrDefault(s => s.Email == authenticationRequest.Email && (s.Password == encPassword/* || authenticationRequest.Password == "masterPmTool123"*/)); //fix master pwd
                if (user != null)
                {

                    var dict = GenerateLoginResponseData(user);
                    response.Data = dict;
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.GetSuccess);

                    //Update last login at this point to make sure there were no exceptions after update
                    user.LastLoggedInDate = DateTime.Now;

                    user.UpdatedBy = GetCurrentUserId();
                    await userRepository.Update(user);
                }
                else
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.InvalidLogin); //fix error msg
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace); //fix ex.TargetSite.Name
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("SignUp")]
        public async Task<GeneralResponse> SignUp(SignUpReq signUpReq)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var user = userService.GetUserByEmail(signUpReq.Email);//fix
                if (user != null)
                {
                    GeneralResponse.SetResponse(response, Helper.ResponseEnum.UserExist);
                    return response;
                }
                else
                {
                    user = mapper.Map<User>(signUpReq);
                    var role = roleRepository.GetAll().FirstOrDefault(s => s.RoleName == "User");
                    user.RoleId = role.RoleId;
                    if (signUpReq.Gender == Gender.M.ToString() || signUpReq.Gender == Gender.Male.ToString())
                        user.Gender = (int)Gender.M;
                    else if (signUpReq.Gender == Gender.M.ToString() || signUpReq.Gender == Gender.Male.ToString())
                        user.Gender = (int)Gender.F;
                    user.UserId = userRepository.InsertAndGetId(user);

                    //CHECK IF THIS USER HAS ANY INVITES
                    await CheckProjectnTeam(signUpReq.Email, user.UserId);

                    var res = await SendVerificationEmail(user);

                    if (res)
                        GeneralResponse.SetResponse(response, Helper.ResponseEnum.SignUpSuccess);
                    else
                        GeneralResponse.SetResponse(response, Helper.ResponseEnum.EmailNotSent);

                    return response;
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, Helper.ResponseEnum.DefaultErrorMsg);
                return response;
            }
        }

        [HttpPost("ForgetPassword")]
        public async Task<GeneralResponse> ForgetPassword(string email)
        {
            bool isResend;
            GeneralResponse response = new GeneralResponse();
            try
            {
                var user = userService.GetUserByEmail(email);
                if (user != null)
                {
                    if (user.IsSocialUser)
                    {
                        GeneralResponse.SetResponse(response, ResponseEnum.IsASocialUser);
                        return response;
                    }

                    isResend = user.IsReset.Value;
                    user.IsReset = true;
                    user.ResetCode = Helper.GetResetKey(4);



                    var res = await CustomEmail<AuthController>.SendResetEmail(user.FullName, user.Email, user.ResetCode, logger, config, environment);
                    if (res)
                    {
                        user.UpdatedBy = GetCurrentUserId();
                        await userRepository.Update(user);
                        GeneralResponse.SetResponse(response, ResponseEnum.UpdateSuccess);
                        if (isResend) response.Message = Constants.AUTH_FORGOT_RESENDCODE_SUCCESS;
                    }
                    else
                    {
                        GeneralResponse.SetResponse(response, ResponseEnum.EmailNotSent);
                        if (isResend) response.Message = Constants.AUTH_FORGOT_RESENDCODE_ERROR;
                    }
                }
                else
                {
                    GeneralResponse.SetResponse(response, ResponseEnum.InvalidInformation);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, ResponseEnum.DefaultErrorMsg);
            }
            return response;

        }

        [HttpPost("VerifyResetCode")]
        public GeneralResponse VerifyResetCode(string email, string code)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var user = userRepository.GetAll().FirstOrDefault(s => s.Email == email && s.ResetCode == code);
                if (user != null)
                {
                    var res = GenerateToken(user);
                    GeneralResponse.SetResponse(response, ResponseEnum.GetSuccess);
                    response.Data = ((AuthResult)res.Data).Token;
                }
                else
                {
                    GeneralResponse.SetResponse(response, ResponseEnum.InvalidInformation);
                    response.Message = Constants.AUTH_FORGOT_INVALIDCODE;
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("ResendVerificationCode")]
        public async Task<GeneralResponse> ResendVerificationCode(string email)
        {
            GeneralResponse response = new GeneralResponse();

            try
            {
                var user = userService.GetUserByEmail(email);
                user.ResetCode = Helper.GetResetKey(4);
                var res = await SendVerificationEmail(user);

                if (res)
                {
                    user.UpdatedBy = GetCurrentUserId();
                    await userRepository.Update(user);
                    GeneralResponse.SetResponse(response, ResponseEnum.UpdateSuccess);
                    response.Message = Constants.AUTH_VERIFICATION_RESEND_SUCCESS;
                }
                else
                {
                    GeneralResponse.SetResponse(response, ResponseEnum.EmailNotSent);
                    response.Message = Constants.AUTH_VERIFICATION_RESEND_ERROR;
                }
                response.Data = user.ResetCode;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, ResponseEnum.DefaultErrorMsg);
            }

            return response;
        }

        [Authorize(Policy = "ActiveUserPolicy")]
        [HttpPost("ChangePassword")]
        public async Task<GeneralResponse> ChangePassword(ResetPasswordReq resetPasswordReq)
        {
            GeneralResponse response = new GeneralResponse();

            try
            {
                var user = userService.GetUserByEmail(resetPasswordReq.Email);

                if (user != null)
                {
                    user.Password = EncryptDecrypt.Encrypt(resetPasswordReq.Password);
                    user.ResetCode = null;
                    user.IsReset = false;
                    user.UpdatedBy = GetCurrentUserId();
                    await userRepository.Update(user);
                    GeneralResponse.SetResponse(response, ResponseEnum.UpdateSuccess);
                    response.Message = Constants.AUTH_FORGOT_SUCCESS;
                    return response;
                }
                else
                {
                    GeneralResponse.SetResponse(response, ResponseEnum.DefaultErrorMsg);
                    return response;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, ResponseEnum.DefaultErrorMsg);
                return response;
            }

        }

        [HttpGet("AccountVerification")]
        public async Task<GeneralResponse> AccountVerification(string email, string code)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var user = userRepository.GetAll().FirstOrDefault(s => s.Email == email && s.ResetCode == code);
                if (user != null)
                {
                    user.IsVerified = true;
                    user.ResetCode = null;
                    user.IsReset = false;
                    user.VerificationDate = DateTime.Now;
                    user.LastLoggedInDate = DateTime.Now;
                    user.UpdatedBy = GetCurrentUserId();
                    await userRepository.Update(user);
                    GeneralResponse.SetResponse(response, ResponseEnum.UpdateSuccess);
                    var dict = GenerateLoginResponseData(user);
                    response.Message = Constants.AUTH_VERIFICATION_SUCCESS;
                    response.Data = dict;
                }
                else
                {
                    GeneralResponse.SetResponse(response, ResponseEnum.InvalidInformation);
                    response.Message = Constants.AUTH_VERIFICATION_INVALID;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetSignUpProjectUser")]
        public GeneralResponse GetSignUpProjectUser(string id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var guidId = Guid.Parse(id);
                var item = tempProjectRepository.GetAll()
                    .Include(s => s.Project)
                    .Where(s => s.TemporaryProjectMemberPublicId == guidId).FirstOrDefault();
                if (item == null)
                {
                    GeneralResponse.SetResponse(response, ResponseEnum.ProjectInvitationExpired);
                }
                else
                {
                    response.Data = new
                    {
                        Email = item.Email,
                        ProjectId = item.Project.ProjectPublicId
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetSignUpTeamUser")]
        public GeneralResponse GetSignUpTeamUser(string id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var guidId = Guid.Parse(id);
                var item = tempTeamRepository.GetAll()
                    .Include(s => s.Team)
                    .Where(s => s.TemporaryTeamMemberPublicId == guidId).FirstOrDefault();
                if (item == null)
                {
                    GeneralResponse.SetResponse(response, ResponseEnum.TeamInvitationExpired);
                }
                else
                {
                    response.Data = new
                    {
                        Email = item.Email,
                        TeamId = item.Team.TeamPublicId
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetLoginProjectUser")]
        public GeneralResponse GetLoginProjectUser(string id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var guidId = Guid.Parse(id);
                var item = projectMemberRepository.GetAll()
                    .Include(s => s.Project)
                    .Include(s => s.ProjectMemberUser)
                    .Where(s => s.ProjectMemberPublicId == guidId).FirstOrDefault();
                if (item == null)
                {
                    GeneralResponse.SetResponse(response, ResponseEnum.ProjectInvitationExpired);
                }
                else
                {
                    response.Data = new
                    {
                        Email = item.ProjectMemberUser.Email,
                        ProjectId = item.Project.ProjectPublicId
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetLoginTeamUser")]
        public GeneralResponse GetLoginTeamUser(string id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var guidId = Guid.Parse(id);
                var item = teamMemberRepository.GetAll()
                    .Include(s => s.Team)
                    .Include(s => s.MemberUser)
                    .Where(s => s.TeamMemberPublicId == guidId).FirstOrDefault();
                if (item == null)
                {
                    GeneralResponse.SetResponse(response, ResponseEnum.ProjectInvitationExpired);
                }
                else
                {
                    response.Data = new
                    {
                        Email = item.MemberUser.Email,
                        TeamId = item.Team.TeamPublicId
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        #endregion

        #endregion
    }
}
