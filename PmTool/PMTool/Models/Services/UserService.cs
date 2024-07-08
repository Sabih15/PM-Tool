using AutoMapper;
using DAL.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMTool.Authorization;
using PMTool.General;
using PMTool.Models.DTOs;
using PMTool.Models.General;
using PMTool.Models.Request;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PMTool.Services
{
    public class UserService : IUserService
    {
        #region Fields

        private readonly IRepository<User> userRepository;
        private readonly IRepository<Role> roleRepository;
        private readonly IRepository<RolePermission> rolePermissionRepository;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly ILogger<UserService> logger;

        #endregion

        #region Constructors

        public UserService(
            IRepository<User> _userRepository,
            IRepository<Role> _roleRepository,
            IRepository<RolePermission> _rolePermissionRepository,
            IMapper _mapper,
            IConfiguration _configuration,
            ILogger<UserService> _logger)
        {
            userRepository = _userRepository;
            roleRepository = _roleRepository;
            rolePermissionRepository = _rolePermissionRepository;
            mapper = _mapper;
            configuration = _configuration;
            logger = _logger;
        }

        #endregion

        #region Methods

        public UsersListDto GetFilteredList(string query, int pageSize = 10, int pageIndex = 0)
        {
            try
            {
                var result = new UsersListDto();
                List<User> items;
                if (string.IsNullOrEmpty(query) || string.IsNullOrWhiteSpace(query))
                    items = userRepository.GetAll().Include(s => s.Role).Where(s => s.IsVerified == true).ToList();
                else
                    items = userRepository.GetAll().Include(s => s.Role).Where(s =>
                        s.IsVerified == true &&
                        s.Email.StartsWith(query) ||
                        s.FullName.Contains(query))
                        .ToList();

                result.count = items.Count;
                items = GetPage(items, pageSize, pageIndex);
                result.resources = mapper.Map<List<UsersDto>>(items);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public EditUserReq GetEditUserById(string id)
        {
            try
            {
                var userId = Guid.Parse(id);
                var user = userRepository.GetAll().FirstOrDefault(s => s.UserPublicId == userId);
                var result = new EditUserReq()
                {
                    FullName = user.FullName,
                    UserPublicId = user.UserPublicId,
                    Email = user.Email,
                    Roleid = user.RoleId,
                    Picture = string.IsNullOrEmpty(user.PictureURL) ? "" : Path.Combine(configuration.GetValue<string>("ServerUrl"), user.PictureURL.TrimStart('\\')),
                    SocialUser = user.IsSocialUser ? "Yes" : "No"
                };
                return result;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        public User GetUserById(int id)
        {
            try
            {
                var user = userRepository.GetAll().Include(s => s.Role).FirstOrDefault(s => s.UserId == id);
                return user;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        public User GetUserByEmail(string email)
        {
            try
            {
                var user = userRepository.GetAll().Include(s => s.Role).FirstOrDefault(s => s.Email == email);
                return user;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public User GetUserByPublicId(string id)
        {
            try
            {
                var guidid = Guid.Parse(id);
                var user = userRepository.GetAll().Include(s => s.Role).FirstOrDefault(s => s.UserPublicId == guidid);
                return user;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<UsersListDto> EditUser(EditUserReq editUserReq, int? currentUserId, IFormFile file)
        {
            try
            {
                var user = userRepository.GetAll().Include(s => s.Role).FirstOrDefault(s => s.UserPublicId == editUserReq.UserPublicId);
                var path = configuration.GetValue<string>("UserAvatarUrl");
                var contentPath = Constants.ContentRootPath;
                var filePath = Path.Combine(contentPath, path.TrimStart('\\'), user.UserPublicId.ToString());
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                else
                {
                    foreach (var item in Directory.EnumerateFiles(filePath))
                        File.Delete(item);
                }    
                using (FileStream fs = new FileStream(Path.Combine(filePath, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                user.FullName = editUserReq.FullName.Trim();
                user.PictureURL = Path.Combine(configuration.GetValue<string>("UserAvatarUrl"), user.UserPublicId.ToString(), file.FileName);

                if (!string.IsNullOrEmpty(editUserReq.NewPassword))
                    user.Password = EncryptDecrypt.Encrypt(editUserReq.NewPassword).Trim();

                user.UpdatedBy = currentUserId;
                    await userRepository.Update(user);
                var result = GetFilteredList("");
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool?> DeleteUser(string id, int? currentUserId)
        {
            try
            {
                var guidId = Guid.Parse(id);
                var user = userRepository.GetAll().FirstOrDefault(s => s.UserPublicId == guidId);
                if (user == null)
                    return false;
                user.UpdatedBy = currentUserId;
                await userRepository.Delete(user);
                return true;
            }
            catch (Exception)
            {

                throw;
            }   
        }

        public async Task<List<GeneralDto>> GetRoles()
        {
            try
            {
                var items = await this.roleRepository.GetAllListAsync();
                var result = mapper.Map<List<GeneralDto>>(items);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<string> GetRolePermissions(int roleId)
        {
            try
            {
                var rolePermissions = rolePermissionRepository.GetAll()
                    .Include(s => s.Permission)
                    .Where(s => s.RoleId == roleId)
                    .ToList();
                return rolePermissions.Select(s => s.Permission.PermissionName).ToList();
                
            }
            catch (Exception)
            {

                throw;
            }
        }

        private List<User> GetPage(List<User> list, int pageSize, int pageIndex)
        {
            try
            {
                list = list.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return list;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion
    }
}
