using DAL.Models;
using Microsoft.AspNetCore.Http;
using PMTool.Models.DTOs;
using PMTool.Models.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMTool.Services
{
    public interface IUserService
    {
        UsersListDto GetFilteredList(string query, int pageSize, int pageIndex);
        EditUserReq GetEditUserById(string id);
        User GetUserById(int id);
        Task<UsersListDto> EditUser(EditUserReq editUserReq, int? currentUserId, IFormFile file);
        Task<List<GeneralDto>> GetRoles();
        Task<bool?> DeleteUser(string id, int? currentUserId);
        User GetUserByPublicId(string id);
        User GetUserByEmail(string email);
        List<string> GetRolePermissions(int roleId);
    }
}