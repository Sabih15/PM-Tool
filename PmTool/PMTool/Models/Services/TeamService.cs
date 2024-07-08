using AutoMapper;
using DAL.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMTool.Controllers;
using PMTool.General;
using PMTool.Models.DTOs;
using PMTool.Models.Enums;
using PMTool.Models.General;
using PMTool.Models.Request;
using PMTool.Models.Response;
using PMTool.Models.Services;
using PMTool.Resources.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Services
{
    public class TeamService : ITeamService
    {
        #region Fields

        private readonly IRepository<Team> teamRepository;
        private readonly IRepository<TeamMember> teamMemberRepository;
        private readonly IRepository<User> userRepository;
        private readonly IRepository<TemporaryTeamMember> tempTeamMemberRepository;
        private readonly IRepository<ProjectTeam> projectTeamRepository;
        private readonly IRepository<NotificationFieldType> notificationFieldTypesRepository;
        private readonly IUserService userService;
        private readonly INotificationService notificationService;
        private readonly IMapper mapper;
        private readonly IConfiguration config;
        private readonly ILogger<TeamService> logger;
        private readonly IWebHostEnvironment environment;

        #endregion

        #region Constructors

        public TeamService(
            IRepository<Team> _teamRepository,
            IRepository<TeamMember> _teamMemberRepository,
            IRepository<User> _userRepository,
            IRepository<TemporaryTeamMember> _tempTeamMemberRepository,
            IRepository<ProjectTeam> _projectTeamRepository,
            IRepository<NotificationFieldType> _notificationFieldTypesRepository,
            IUserService _userService,
            INotificationService _notificationService,
            IMapper _mapper,
            IConfiguration _config,
            ILogger<TeamService> _logger,
            IWebHostEnvironment _environment
            )
        {
            teamRepository = _teamRepository;
            teamMemberRepository = _teamMemberRepository;
            userRepository = _userRepository;
            tempTeamMemberRepository = _tempTeamMemberRepository;
            projectTeamRepository = _projectTeamRepository;
            notificationFieldTypesRepository = _notificationFieldTypesRepository;
            userService = _userService;
            notificationService = _notificationService;
            mapper = _mapper;
            config = _config;
            logger = _logger;
            environment = _environment;
        }

        #endregion

        #region Methods

        private async Task<int> UpdateTeamMember(List<string> emails, Team team, int? currentUserId)
        {
            int totalMembers = 0;
            try
            {
                var teamMembers = await teamMemberRepository.GetAll()
                .Include(s => s.MemberUser)
                .Where(s => (s.TeamId == team.TeamId) && s.MemberUserId != currentUserId)
                .ToListAsync();

                var tempTeamMembers = await tempTeamMemberRepository.GetAll().Where(s => s.TeamId == team.TeamId).ToListAsync();

                totalMembers = teamMembers.Count + tempTeamMembers.Count;

                var toAddEmails = emails.Except(teamMembers.Select(s => s.MemberUser.Email)).Except(tempTeamMembers.Select(s => s.Email)).ToList();

                var toAddUsers = userRepository.GetAll().Where(s => toAddEmails.Contains(s.Email)).ToList();

                var toInviteEmails = toAddEmails.Except(toAddUsers.Select(s => s.Email)).ToList();

                var removedMemberEmails = teamMembers.Select(s => s.MemberUser.Email).Except(emails);

                var removedInvites = tempTeamMembers.Select(s => s.Email).Except(emails);

                if (removedMemberEmails.ToList().Count > 0)
                {
                    var removedMembers = teamMembers.Where(s => removedMemberEmails.Contains(s.MemberUser.Email)).ToList();
                    totalMembers -= await RemoveRegisteredTeamMembers(removedMembers, team.TeamId, currentUserId);
                }

                if (removedInvites.ToList().Count > 0)
                {
                    var removedTempMembers = tempTeamMembers.Where(s => removedInvites.Contains(s.Email)).ToList();
                    totalMembers -= RemovedUnregisteredTeamMembers(removedTempMembers, team.TeamId, currentUserId);
                }

                if (toAddUsers.Count > 0)
                {
                    totalMembers += await AddRegisteredTeamMembers(toAddUsers, team, currentUserId);
                }

                if (toInviteEmails.Count > 0)
                {
                    totalMembers += await AddUnregisteredTeamMembers(toInviteEmails, team, currentUserId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
            }
            return totalMembers;
        }

        public async Task<bool> SendInviteEmail(string email, string inviteFor, string inviteLink)
        {
            try
            {
                var result = await CustomEmail<TeamService>.SendInviteEmail(email, inviteFor, inviteLink, logger, config, environment);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                throw;
            }
            
        }

        private async Task<int> RemoveRegisteredTeamMembers(List<TeamMember> members, int teamId, int? currentUserId)
        {
            int totalmembers = 0;
            try
            {
                foreach (var member in members)
                {
                    member.UpdatedBy = currentUserId;
                    await teamMemberRepository.Delete(member);
                    totalmembers++;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
            }
            return totalmembers;
        }

        private int RemovedUnregisteredTeamMembers(List<TemporaryTeamMember> tempMembers, int teamId, int? currentUserId)
        {
            int totalmembers = 0;
            try
            {
                foreach (var tempMember in tempMembers)
                {
                    tempTeamMemberRepository.HardDelete(tempMember);
                    totalmembers++;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
            }
            return totalmembers;
        }

        private async Task<int> AddRegisteredTeamMembers(List<User> users, Team team, int? currentUserId)
        {
            int totalmembers = 0;
            try
            {
                foreach (var userId in users.Select(s => s.UserId))
                {
                    var exist = teamMemberRepository.GetAll().Where(s => s.TeamId == team.TeamId && s.MemberUserId == userId).FirstOrDefault();
                    if (exist != null)
                    {
                        totalmembers++;
                        continue;
                    }
                    var link = config["BaseUrlWeb"] + "#/auth/login?t=" + team.TeamPublicId.ToString();
                    var member = new TeamMember()
                    {
                        CreatedBy = currentUserId,
                        MemberUserId = userId,
                        TeamId = team.TeamId
                    };
                    var result = await teamMemberRepository.InsertAsync(member);
                    if (result != null)
                    {
                        totalmembers++;
                        await CustomEmail<TeamService>.SendInviteEmail(users.First(s => s.UserId == userId).Email, team.TeamName, link, logger, config, environment);
                        var notificationFieldTypes = notificationFieldTypesRepository.GetAllListAsync().Result;
                        foreach (var item in users)
                        {
                            NotificationField notificationField = new NotificationField
                            {
                                NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{TeamName}").NotificationFieldTypeId,
                                Name = notificationFieldTypes.First(s => s.Text == "{TeamName}").Type,
                                Value = team.TeamName
                            };
                            var fields = new List<NotificationField>();
                            fields.Add(notificationField);
                            var notification = new AddNotificationRequest
                            {
                                Fields = fields,
                                FromUserId = currentUserId,
                                NotificationTypeId = notificationService.GetNotificationTypeId(NotificationTypes.AddedInTeam.ToString()).NotificationTypeId,
                                ToUserId = item.UserId
                            };
                            var notificationId = await notificationService.GenerateNotification(notification);
                            var data = await notificationService.GetNotificationText(notificationId);
                            await notificationService.SendAsync(NotificationListeners.addedInTeam.ToString(), item.UserPublicId.ToString(), data);
                            var notificationData = await notificationService.GetNotifications(item.UserId, 10, 0);
                            await notificationService.SendAsync(NotificationListeners.updateNotificationPanel.ToString(), item.UserPublicId.ToString(), notificationData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
            }
            
            return totalmembers;
        }

        private async Task<int> AddUnregisteredTeamMembers(List<string> inviteEmails, Team team, int? currentUserId)
        {
            int totalmembers = 0;
            try
            {
                foreach (var invite in inviteEmails)
                {
                    var exist = tempTeamMemberRepository.GetAll().Where(s => s.TeamId == team.TeamId && s.Email == invite).FirstOrDefault();
                    if (exist != null)
                    {
                        totalmembers++;
                        continue;
                    }
                    
                    var tempTeamMember = new TemporaryTeamMember()
                    {
                        CreatedBy = currentUserId,
                        Email = invite,
                        TeamId = team.TeamId
                    };
                    tempTeamMember = tempTeamMemberRepository.Insert(tempTeamMember);
                    var link = config["BaseUrlWeb"] + "#/auth/register?t="+ tempTeamMember.TemporaryTeamMemberPublicId;
                    if (tempTeamMember != null)
                    {
                        totalmembers++;
                        await CustomEmail<TeamService>.SendInviteEmail(invite, team.TeamName, link, logger, config, environment);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
            }

            return totalmembers;
        }

        private async Task<int> AddTeamMember(List<string> emails, Team team, int? currentUserId)
        {
            int totalmembers = 0;
            try
            {
                var users = userRepository.GetAll().Where(s => emails.Contains(s.Email)).ToList();

                var inviteEmails = emails.Except(users.Select(s => s.Email)).ToList();

                totalmembers += await AddRegisteredTeamMembers(users, team, currentUserId);

                totalmembers += await AddUnregisteredTeamMembers(inviteEmails, team, currentUserId);
            }

            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
            }

            return totalmembers;

        }

        public int GetUserTeamIdByName(string name, int? userId)
        {
            try
            {
                var team = teamRepository.GetAll().FirstOrDefault(s => s.TeamName == name && s.CreatedBy == userId);
                return team.TeamId;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<GeneralResponse> UpsertTeam(UpsertTeamReq upsertTeamReq, int? currentUserId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var team = teamRepository.GetAll().FirstOrDefault(s => s.TeamPublicId == upsertTeamReq.TeamId);

                //INSERT
                if (team == null)
                {
                    team = mapper.Map<Team>(upsertTeamReq);
                    team.CreatedBy = currentUserId;
                    var teamId = teamRepository.InsertAndGetId(team);

                    //INSERT TEAM CREATOR MEMBER
                    var teamMember = new TeamMember()
                    {
                        CreatedBy = currentUserId,
                        MemberUserId = currentUserId,
                        TeamId = teamId,
                    };
                    await teamMemberRepository.InsertAsync(teamMember);

                   //ADD MEMBERS
                    team.TotalMembers = await AddTeamMember(upsertTeamReq.MemberEmails, team, currentUserId);
                    team.UpdatedBy = currentUserId;
                    await teamRepository.Update(team);
                    GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.CreateSuccess);
                }
                else //Update
                {
                    team.TeamName = upsertTeamReq.TeamName.Trim();
                    team.Description = upsertTeamReq.Description.Trim();
                    team.TotalMembers = await UpdateTeamMember(upsertTeamReq.MemberEmails, team, currentUserId);

                    team.UpdatedBy = currentUserId;
                    await teamRepository.Update(team);
                    GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.UpdateSuccess);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return response;
        }

        public async Task<TeamListDtoPage> GetAllTeams(GetAllPageReq getAllTeamsReq, int? currentUserId)
        {
            try
            {
                var result = new TeamListDtoPage();
                result.TeamList = new List<TeamListDto>();
                List<Team> teams;

                var myTeams = teamMemberRepository.GetAll().Where(s => s.MemberUserId == currentUserId).Select(s => s.TeamId.Value).ToList();
                if (string.IsNullOrEmpty(getAllTeamsReq.Query) || string.IsNullOrWhiteSpace(getAllTeamsReq.Query))
                    teams = teamRepository.GetAll().Where(s => (s.CreatedBy == currentUserId) || myTeams.Contains(s.TeamId)).ToList();
                else
                    teams = teamRepository.GetAll().Where(s => (s.CreatedBy == currentUserId || myTeams.Contains(s.TeamId)) && (s.TeamName.Contains(getAllTeamsReq.Query) || s.Description.Contains(getAllTeamsReq.Query))).ToList();
                result.Count = teams.Count;
                teams = teams.Skip(getAllTeamsReq.PageCount * getAllTeamsReq.PageSize).Take(getAllTeamsReq.PageSize).ToList();
                foreach (var team in teams)
                {
                    var members = await teamMemberRepository.GetAll()
                        .Include(s => s.MemberUser)
                        .Where(s => s.TeamId == team.TeamId).ToListAsync();

                    var tempMembers = await tempTeamMemberRepository.GetAll().Where(s => s.TeamId == team.TeamId).ToListAsync();

                    var names = new List<object>();
                    var configuration = config.GetValue<string>("ServerUrl");
                    names.AddRange(members.Select(s => new { Email = s.MemberUser.Email, Picture = string.IsNullOrEmpty(s.MemberUser.PictureURL) ? "" : configuration + s.MemberUser.PictureURL }));
                    names.AddRange(tempMembers.Select(s => new { Email = s.Email, Picture = (string)null }));

                    var projects = await projectTeamRepository.GetAll().Include(s => s.Project).Where(s => s.TeamId == team.TeamId && s.Project.IsActive == true).Select(s => new GeneralDto
                    {
                        Id = s.Project.ProjectPublicId,
                        Name = s.Project.ProjectName
                    }).ToListAsync();

                    var teamListDto = mapper.Map<TeamListDto>(team);
                    teamListDto.IsOwner = team.CreatedBy == currentUserId ? true : false;
                    teamListDto.TotalMembers = names;
                    teamListDto.Projects = projects;
                    teamListDto.ProjectCount = projects.Count;
                    result.TeamList.Add(teamListDto);
                };
                return result;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<UpsertTeamReq> GetTeamById(string id, int? currentUserId)
        {
            try
            {
                var teamId = Guid.Parse(id);
                var team = teamRepository.GetAll().FirstOrDefault(s => s.TeamPublicId == teamId);
                var members = await teamMemberRepository.GetAll().Include(s => s.MemberUser).Where(s => s.TeamId == team.TeamId && s.MemberUserId != currentUserId).ToListAsync();
                var tempMembers = await tempTeamMemberRepository.GetAll().Where(s => s.TeamId == team.TeamId).ToListAsync();

                var result = mapper.Map<UpsertTeamReq>(team);
                result.Members = new List<object>();
                result.Members.AddRange(members.Select(s => new
                {
                    Email = s.MemberUser.Email,
                    Name = s.MemberUser.FullName,
                    IsNew = false
                }));
                result.Members.AddRange(tempMembers.Select(s => new
                {
                    Email = s.Email,
                    Name = s.Email,
                    IsNew = true
                }));
                //result.MemberEmails.AddRange(members.Select(s => new MemberDDL() { s.MemberUser.Email));
                //result.MemberEmails.AddRange(tempMembers.Select(s => s.Email));
                return result;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public List<MemberDDL> GetAllMembers(int? currentUserId)
        {
            try
            {
                var items = userRepository.GetAll().Where(s => s.IsVerified == true && s.UserId != currentUserId).ToList();
                var result = new List<MemberDDL>();
                items.ForEach(s => result.Add(new MemberDDL() { Email = s.Email, Name = s.FullName })); ;
                return result;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task DeleteTeam(string id, int? currentUserId)
        {
            try
            {
                var teamId = Guid.Parse(id);
                //DeleteMembers
                var team = teamRepository.GetAll().FirstOrDefault(s => s.TeamPublicId == teamId);
                team.UpdatedBy = currentUserId;
                await teamRepository.Delete(team);
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        #endregion
    }
}
