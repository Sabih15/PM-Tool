using AutoMapper;
using DAL.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMTool.General;
using PMTool.Models.DTOs;
using PMTool.Models.Enums;
using PMTool.Models.General;
using PMTool.Models.Request;
using PMTool.Models.Services;
using PMTool.Resources.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Services
{
    public class ProjectService : IProjectService
    {
        #region Fields

        private readonly IRepository<Project> projectRepository;
        private readonly IRepository<ProjectPermission> projectPermissionRepository;
        private readonly IRepository<ProjectMemberPermission> projectMemberPermissionRepository;
        private readonly IRepository<TemporaryProjectMember> tempProjectMemberRepository;
        private readonly IRepository<TemporaryTeamMember> tempTeamMemberRepository;
        private readonly IRepository<ProjectMember> projectMemberRepository;
        private readonly IRepository<ProjectTeam> projectTeamRepository;
        private readonly IRepository<TeamMember> teamMemberRepository;
        private readonly IRepository<ChallengeList> challengeListRepository;
        private readonly IRepository<CheckList> checkListRepository;
        private readonly IRepository<Team> teamRepository;
        private readonly IRepository<Card> cardRepository;
        private readonly IRepository<User> userRepository;
        private readonly IRepository<Challenge> challengeRepository;
        private readonly IRepository<NotificationFieldType> notificationFieldTypesRepository;
        private readonly IUserService userService;
        private readonly ITeamService teamService;
        private readonly INotificationService notificationService;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<ProjectService> logger;
        private readonly IConfiguration config;
        private readonly IMapper mapper;

        #endregion

        #region Constructors

        public ProjectService(
            IRepository<Project> _projectRepository,
            IRepository<ProjectPermission> _projectPermissionRepository,
            IRepository<ProjectMemberPermission> _projectMemberPermissionRepository,
            IRepository<TemporaryProjectMember> _tempProjectMemberRepository,
            IRepository<TemporaryTeamMember> _tempTeamMemberRepository,
            IRepository<ProjectMember> _projectMemberRepository,
            IRepository<ProjectTeam> _projectTeamRepository,
            IRepository<TeamMember> _teamMemberRepository,
            IRepository<ChallengeList> _challengeListRepository,
            IRepository<CheckList> _checkListRepository,
            IRepository<Team> _teamRepository,
            IRepository<NotificationFieldType> _notificationFieldTypesRepository,
            IRepository<Card> _cardRepository,
            IRepository<User> _userRepository,
            IRepository<Challenge> _challengeRepository,
            IUserService _userService,
            ITeamService _teamService,
            INotificationService _notificationService,
            IWebHostEnvironment _environment,
            ILogger<ProjectService> _logger,
            IConfiguration _config,
            IMapper _mapper)
        {
            projectRepository = _projectRepository;
            projectPermissionRepository = _projectPermissionRepository;
            projectMemberPermissionRepository = _projectMemberPermissionRepository;
            tempProjectMemberRepository = _tempProjectMemberRepository;
            tempTeamMemberRepository = _tempTeamMemberRepository;
            projectMemberRepository = _projectMemberRepository;
            projectTeamRepository = _projectTeamRepository;
            teamMemberRepository = _teamMemberRepository;
            challengeListRepository = _challengeListRepository;
            checkListRepository = _checkListRepository;
            teamRepository = _teamRepository;
            cardRepository = _cardRepository;
            notificationFieldTypesRepository = _notificationFieldTypesRepository;
            userRepository = _userRepository;
            challengeRepository = _challengeRepository;
            userService = _userService;
            teamService = _teamService;
            notificationService = _notificationService;
            environment = _environment;
            logger = _logger;
            config = _config;
            mapper = _mapper;
        }

        #endregion

        #region Methods

        #region Public

        public async Task<EditProjectDto> GetProjectById(string id, int? currentUserId)
        {
            try
            {
                var project = GetProjectByPublicId(id);
                var members = await projectMemberRepository.GetAll()
                    .Include(s => s.ProjectMemberUser)
                    .Where(s => s.ProjectId == project.ProjectId && s.ProjectMemberUserId != currentUserId)
                    .ToListAsync();

                var tempMembers = await tempProjectMemberRepository.GetAll()
                    .Where(s => s.ProjectId == project.ProjectId)
                    .ToListAsync();

                var teams = await projectTeamRepository.GetAll()
                    .Include(s => s.Team)
                    .Where(s => s.ProjectId == project.ProjectId)
                    .ToListAsync();

                var unlockedChallenges = challengeRepository.GetAll().Where(s => s.ProjectId == project.ProjectId && s.IsLocked == false).Count();


                var result = mapper.Map<EditProjectDto>(project);
                result.MemberEmails = new List<string>();
                result.Teams = new List<string>();

                result.UnlockedChallenges = unlockedChallenges == 0 ? unlockedChallenges + 1 : unlockedChallenges;
                result.MemberEmails.AddRange(members.Select(s => s.ProjectMemberUser.Email));
                result.MemberEmails.AddRange(tempMembers.Select(s => s.Email));
                result.Teams.AddRange(teams.Select(s => s.Team.TeamName));
                
                return result;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public List<MemberDDL> GetExistingProjectMembers(string id)
        {
            try
            {
                var project = GetProjectByPublicId(id);
                var members = projectMemberRepository.GetAll().Include(s => s.ProjectMemberUser).Where(s => s.ProjectId == project.ProjectId).Select(s => new MemberDDL
                {
                    Email = s.ProjectMemberUser.Email,
                    Name = s.ProjectMemberUser.FullName
                }).ToList();
                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public Project GetProjectByPublicId(string id)
        {
            try
            {
                var guidId = Guid.Parse(id);
                var project = projectRepository.GetAll().FirstOrDefault(s => s.ProjectPublicId == guidId);
                return project;
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        public async Task DeleteProject(string id, int? currentUserId)
        {
            try
            {
                var project = GetProjectByPublicId(id);
                project.UpdatedBy = currentUserId;
                await projectRepository.Delete(project);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<ProjectListDtoPage> GetProjects(GetAllPageReq getProjectsReq, int? currentUserid)
        {
            try
            {
                var result = new ProjectListDtoPage();
                List<Project> projects;
                HashSet<int> myProjects = new HashSet<int>();
                var x = await teamMemberRepository.GetAll().Where(s => s.MemberUserId == currentUserid).Select(s => s.TeamId).ToListAsync();
                myProjects.UnionWith(projectMemberRepository.GetAll().Where(s => s.ProjectMemberUserId == currentUserid).Select(s => s.ProjectId));
                myProjects.UnionWith(projectTeamRepository.GetAll().Where(s => x.Contains(s.TeamId)).Select(s => s.ProjectId.Value));
                if (string.IsNullOrEmpty(getProjectsReq.Query) || string.IsNullOrWhiteSpace(getProjectsReq.Query))
                    projects = projectRepository.GetAll().Where(s => (s.CreatedBy == currentUserid) || myProjects.Contains(s.ProjectId)).ToList();
                else
                    projects = projectRepository.GetAll().Where(s => ((s.CreatedBy == currentUserid) || myProjects.Contains(s.ProjectId)) && (s.ProjectName.Contains(getProjectsReq.Query))).ToList();
                result.Count = projects.Count;
                projects = projects.Skip(getProjectsReq.PageSize * getProjectsReq.PageCount).Take(getProjectsReq.PageSize).ToList();
                result.ProjectList = mapper.Map<List<ProjectListDto>>(projects);
                var configuration = config.GetValue<string>("ServerUrl");
                foreach (var item in result.ProjectList)
                {
                    item.isOwner = await IsProjectOwner(item, projects, currentUserid);
                    var dict = await GetTotalTasksCount(projects.First(s => s.ProjectPublicId == Guid.Parse(item.Id)).ProjectId);
                    item.TotalTasks = (int)dict["Count"];
                    item.Progress = (int)dict["Progress"];
                    item.TotalMembers = new List<object>();
                    
                    var projectId = projects.FirstOrDefault(s => s.ProjectPublicId.ToString() == item.Id).ProjectId;
                    var members = await projectMemberRepository.GetAll().Include(s => s.ProjectMemberUser).Where(s => s.ProjectId == projectId).Select(s => new { Email = s.ProjectMemberUser.Email, Picture = string.IsNullOrEmpty(s.ProjectMemberUser.PictureURL) ? "" :  configuration + s.ProjectMemberUser.PictureURL}).ToListAsync();
                    var tempMembers = await tempProjectMemberRepository.GetAll().Where(s => s.ProjectId == projectId).Select(s => new { Email = s.Email, Picture = (string)null }).ToListAsync();
                    var teamIds = await projectTeamRepository.GetAll().Where(s => s.ProjectId == projectId).Select(s => s.TeamId).ToListAsync();
                    var teams = await teamMemberRepository.GetAll().Include(s => s.MemberUser).Where(s => teamIds.Contains(s.TeamId)).Select(s => new { Email = s.MemberUser.Email, Picture = string.IsNullOrEmpty(s.MemberUser.PictureURL) ? "" : configuration + s.MemberUser.PictureURL }).ToListAsync();
                    var tempTeams = await tempTeamMemberRepository.GetAll().Where(s => teamIds.Contains(s.TeamId)).Select(s => new { Email = s.Email, Picture = (string)null }).ToListAsync();

                    item.TotalMembers.AddRange(teams);
                    item.TotalMembers.AddRange(tempTeams);
                    item.TotalMembers.AddRange(members);
                    item.TotalMembers.AddRange(tempMembers);
                    item.TotalMembers = item.TotalMembers.Distinct().ToList();

                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task<bool> UpdateProject(CreateProjectReq projectReq, int? currentUserId)
        {
            try
            {
                var guidId = Guid.Parse(projectReq.ProjectId);
                var project = await projectRepository.GetAll().FirstOrDefaultAsync(s => s.ProjectPublicId == guidId);

                if (project.TotalChallenges < projectReq.TotalChallenges)
                    await CreateChallengeTemplatesForProject(project.ProjectId, projectReq.TotalChallenges, currentUserId, project.TotalChallenges.Value);
                else if (project.TotalChallenges > projectReq.TotalChallenges)
                    await RemoveChallengeTemplatesFromProject(project.ProjectId, projectReq.TotalChallenges, currentUserId, project.TotalChallenges.Value);

                project.ProjectName = projectReq.Name.Trim();
                project.Description = projectReq.Description.Trim();
                project.DueDate = projectReq.DueDate.Value.AddDays(-1);
                project.UpdatedBy = currentUserId;
                project.UpdatedBy = currentUserId;
                project.TotalChallenges = projectReq.TotalChallenges;
                await projectRepository.Update(project);

                //Current Members
                var projectmembers = await projectMemberRepository.GetAll()
                    .Include(s => s.ProjectMemberUser)
                    .Where(s => s.ProjectId == project.ProjectId && s.CreatedBy != s.ProjectMemberUserId)
                    .ToListAsync();

                //Temp Members
                var tempProjectMembers = await tempProjectMemberRepository.GetAll()
                    .Where(s => s.ProjectId == project.ProjectId)
                    .ToListAsync();

                //Teams
                var projectTeams = await projectTeamRepository.GetAll()
                    .Include(s => s.Team)
                    .Where(s => s.ProjectId == project.ProjectId)
                    .ToListAsync();

                //Ids For User that are selected on frontend
                var selectedUsers = await userRepository.GetAll().Where(s => projectReq.MemberEmails.Contains(s.Email)).ToListAsync();
                var selectedTeams = await teamRepository.GetAll().Where(s => s.CreatedBy == currentUserId && projectReq.TeamNames.Contains(s.TeamName)).ToListAsync();
                var selectedTempEmails = projectReq.MemberEmails.Except(selectedUsers.Select(s => s.Email)).ToList();

                var removedMembers = projectmembers.Where(s => !selectedUsers.Select(s => s.Email).Contains(s.ProjectMemberUser.Email)).ToList();
                var addedMembers = selectedUsers.Where(s => !projectmembers.Select(s => s.ProjectMemberUser.Email).Contains(s.Email)).ToList();

                var removedTempMembers = tempProjectMembers.Where(s => !projectReq.MemberEmails.Contains(s.Email)).ToList();
                var addedTempMembers = selectedTempEmails.Except(tempProjectMembers.Select(s => s.Email)).ToList();

                var removedTeams = projectTeams.Where(s => !selectedTeams.Select(a => a.TeamId).ToList().Contains(s.TeamId.Value)).ToList();
                var addedTeams = selectedTeams.Where(s => !projectTeams.Select(s => s.TeamId).ToList().Contains(s.TeamId)).ToList();
                foreach (var removedMember in removedMembers)
                {
                    await RemoveRegisteredMember(removedMember, currentUserId);
                }
                foreach(var addedMember in addedMembers)
                {
                    await AddRegisteredMember(addedMember, currentUserId, project);
                }
                if (removedTempMembers.Count > 0)
                tempProjectMemberRepository.HardDeleteRange(removedTempMembers);
                foreach(var email in selectedTempEmails)
                {
                    await AddUnregisteredMember(email, currentUserId, project);
                }

                foreach (var removedTeam in removedTeams)
                {
                    await RemoveProjectTeam(removedTeam, currentUserId);
                }
                foreach (var addedTeam in addedTeams)
                {
                    AddProjectTeam(addedTeam.TeamName, currentUserId, project.ProjectId);
                }
                

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<bool> CreateProject(CreateProjectReq createProjectReq, int? currentUserId)
        {
            try
            {
                var project = mapper.Map<Project>(createProjectReq);
                project.CreatedBy = currentUserId;
                project.ProjectId = projectRepository.InsertAndGetId(project);

                //INSERT PROJECT CREATOR AS MEMBER
                var creatorMember = new ProjectMember()
                {
                    CreatedBy = currentUserId,
                    ProjectId = project.ProjectId,
                    ProjectMemberUserId = currentUserId,
                };
                await projectMemberRepository.InsertAsync(creatorMember);

                //INSERT TEAMS
                foreach (var team in createProjectReq.TeamNames)
                {
                    var projectteamId = AddProjectTeam(team, currentUserId, project.ProjectId);
                }

                //INSERT MEMBERS
                foreach (var email in createProjectReq.MemberEmails)
                {
                    //Permenant Users
                    var item = userService.GetUserByEmail(email);
                    if (item != null)
                        await AddRegisteredMember(item, currentUserId, project);
                    //TemporaryUser
                    else
                    {
                        await AddUnregisteredMember(email, currentUserId, project);
                    }
                }

                await AddAdminPermissions(project.ProjectId, currentUserId.Value);

                await CreateChallengeTemplatesForProject(project.ProjectId, createProjectReq.TotalChallenges, currentUserId);
                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public async Task<List<ProjectMemberDto>> AddMembersByInvite(List<string> Emails, string projectId, int? currentUserId)
        {
            try
            {
                var result = new List<ProjectMemberDto>();
                var project = GetProjectByPublicId(projectId);
                foreach (var email in Emails)
                {
                    var user = userService.GetUserByEmail(email);
                    if (user == null)
                        await AddUnregisteredMember(email, currentUserId, project);
                    else
                        await AddRegisteredMember(user, currentUserId, project);
                    result.Add(new ProjectMemberDto
                    {
                        isIndividualMember = true,
                        MemberEmail = email,
                        MemberName = user != null ? user.FullName : "",
                        TeamName = "Individual User"
                    });
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        public async Task<List<ProjectParticipants>> GetProjectMembers(int projectId)
        {
            List<ProjectParticipants> participants = new List<ProjectParticipants>();
            List<ProjectParticipants> x = new List<ProjectParticipants>();
            try
            {
                var temp = await GetProjectRegisteredMembers(projectId);
                var temp1 = await GetProjectTeamMembers(projectId);
                var temp2 = await GetProjectTempMembers(projectId);

                x.AddRange(temp);
                x.AddRange(temp1);
                x.AddRange(temp2);
                participants = x.GroupBy(s => s.Email, (key, values) => new ProjectParticipants()
                {
                    IsTeamMember = values.Count() > 1 ? true : values.First().IsTeamMember,
                    Email = values.First().Email,
                    Name = values.First().Name,
                    TeamName = string.Join(',', values.Select(s => s.TeamName)).Contains("Admin") ? "Admin" : string.Join(',', values.Select(s => s.TeamName))
                }).ToList();

                return participants;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<List<GeneralDto>> GetAllUsers(string projectId)
        {
            try
            {
                var project = GetProjectByPublicId(projectId);
                var members = await projectMemberRepository.GetAll()
                    .Include(s => s.ProjectMemberUser)
                    .Where(s => s.ProjectId == project.ProjectId)
                    .Select(s => new GeneralDto
                    {
                        Id = s.ProjectMemberUser.UserPublicId,
                        Name = s.ProjectMemberUser.FullName
                    }).ToListAsync();
                var teamIds = await projectTeamRepository.GetAll().Where(s => s.ProjectId == project.ProjectId).Select(y => y.TeamId).ToListAsync();
                var teamMembers = await teamMemberRepository.GetAll()
                    .Include(s => s.MemberUser)
                    .Where(s => teamIds.Contains(s.TeamId))
                    .Select(s => new GeneralDto
                    {
                        Id = s.MemberUser.UserPublicId,
                        Name = s.MemberUser.FullName
                    }).ToListAsync();
                var result = new List<GeneralDto>();
                foreach (var item in members)
                {
                    if (!result.Select(s => s.Id).Contains(item.Id))
                        result.Add(item);
                }
                foreach (var item in teamMembers)
                {
                    if (!result.Select(s => s.Id).Contains(item.Id))
                        result.Add(item);
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task ProjectDueTodayReminder()
        {
            try
            {
                var projects = projectRepository.GetAll()
                    .Where(s => s.DueDate != null && s.DueDate.Value.Date == DateTime.Today.Date).ToList();
                var notificationFieldTypes = notificationFieldTypesRepository.GetAllListAsync().Result;
                foreach (var project in projects)
                {
                    var projectUsers = projectMemberRepository.GetAll()
                        .Include(s => s.ProjectMemberUser)
                        .Where(s => s.ProjectId == project.ProjectId).Select(s => s.ProjectMemberUser)
                        .ToList();

                    var teamIds = await projectTeamRepository.GetAll().Where(s => s.ProjectId == project.ProjectId).Select(s => s.TeamId).ToListAsync();
                    var teamUsers = await teamMemberRepository.GetAll()
                        .Include(s => s.MemberUser)
                        .Where(s => teamIds.Contains(s.TeamId)).Select(s => s.MemberUser)
                        .ToListAsync();
                    List<User> users = new List<User>();
                    foreach (var item in projectUsers)
                    {
                        if (!users.Select(s => s.UserId).Contains(item.UserId))
                            users.Add(item);
                    }
                    foreach (var item in teamUsers)
                    {
                        if (!users.Select(s => s.UserId).Contains(item.UserId))
                            users.Add(item);
                    }
                    foreach (var user in users)
                    {
                        NotificationField notificationField = new NotificationField
                        {
                            NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{ProjectName}").NotificationFieldTypeId,
                            Name = notificationFieldTypes.First(s => s.Text == "{ProjectName}").Type,
                            Value = project.ProjectName
                        };
                        NotificationField notificationField1 = new NotificationField
                        {
                            NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{ProjectDueTime}").NotificationFieldTypeId,
                            Name = notificationFieldTypes.First(s => s.Text == "{ProjectDueTime}").Type,
                            Value = project.DueDate.Value.ToShortTimeString()
                        };
                        NotificationField notificationField2 = new NotificationField
                        {
                            ProjectId = project.ProjectId,
                        };
                        var fields = new List<NotificationField>();
                        fields.Add(notificationField);
                        fields.Add(notificationField1);
                        fields.Add(notificationField2);
                        var notification = new AddNotificationRequest
                        {
                            Fields = fields,
                            NotificationTypeId = notificationService.GetNotificationTypeId(NotificationTypes.ProjectDueToday.ToString()).NotificationTypeId,
                            ToUserId = user.UserId
                        };
                        var notificationId = notificationService.GenerateNotification(notification).Result;
                        var data = notificationService.GetNotificationText(notificationId).Result;
                        await notificationService.SendAsync(NotificationListeners.projectDueToday.ToString(), user.UserPublicId.ToString(), data);
                        var notificationData = notificationService.GetNotifications(user.UserId, 10, 0).Result;
                        await notificationService.SendAsync(NotificationListeners.updateNotificationPanel.ToString(), user.UserPublicId.ToString(), notificationData);
                        var redirectUrl = notificationService.GetNotificationUrl(notificationId);
                        await CustomEmail<ProjectService>.SendReminder(
                            user.FullName, user.Email, (int)Helper.EmailTemplates.ProjectDueToday, "today", "",
                            project.ProjectName, project.DueDate.Value.ToShortTimeString(), redirectUrl, logger, config, environment);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task ProjectDueTomorrowReminder()
        {
            try
            {
                var projects = projectRepository.GetAll()
                    .Where(s => s.DueDate != null && s.DueDate.Value.Date == DateTime.Today.Date.AddDays(1)).ToList();
                var notificationFieldTypes = notificationFieldTypesRepository.GetAllListAsync().Result;
                foreach (var project in projects)
                {
                    var projectUsers = await projectMemberRepository.GetAll()
                        .Include(s => s.ProjectMemberUser)
                        .Where(s => s.ProjectId == project.ProjectId).Select(s => s.ProjectMemberUser)
                        .ToListAsync();

                    var teamIds = await projectTeamRepository.GetAll().Where(s => s.ProjectId == project.ProjectId).Select(s => s.TeamId).ToListAsync();
                    var teamUsers = await teamMemberRepository.GetAll()
                        .Include(s => s.MemberUser)
                        .Where(s => teamIds.Contains(s.TeamId)).Select(s => s.MemberUser)
                        .ToListAsync();
                    List<User> users = new List<User>();
                    foreach (var item in projectUsers)
                    {
                        if (!users.Select(s => s.UserId).Contains(item.UserId))
                            users.Add(item);
                    }
                    foreach (var item in teamUsers)
                    {
                        if (!users.Select(s => s.UserId).Contains(item.UserId))
                            users.Add(item);
                    }
                    foreach (var user in users)
                    {
                        NotificationField notificationField = new NotificationField
                        {
                            NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{ProjectName}").NotificationFieldTypeId,
                            Name = notificationFieldTypes.First(s => s.Text == "{ProjectName}").Type,
                            Value = project.ProjectName
                        };
                        NotificationField notificationField1 = new NotificationField
                        {
                            NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{ProjectDueTime}").NotificationFieldTypeId,
                            Name = notificationFieldTypes.First(s => s.Text == "{ProjectDueTime}").Type,
                            Value = project.DueDate.Value.ToShortTimeString()
                        };
                        NotificationField notificationField2 = new NotificationField
                        {
                            ProjectId = project.ProjectId,
                        };
                        var fields = new List<NotificationField>();
                        fields.Add(notificationField);
                        fields.Add(notificationField1);
                        fields.Add(notificationField2);
                        var notification = new AddNotificationRequest
                        {
                            Fields = fields,
                            NotificationTypeId = notificationService.GetNotificationTypeId(NotificationTypes.ProjectDueTomorrow.ToString()).NotificationTypeId,
                            ToUserId = user.UserId
                        };
                        var notificationId = notificationService.GenerateNotification(notification).Result;
                        var data = notificationService.GetNotificationText(notificationId).Result;
                        await notificationService.SendAsync(NotificationListeners.projectDueTomorrow.ToString(), user.UserPublicId.ToString(), data);
                        var notificationData = notificationService.GetNotifications(user.UserId, 10, 0).Result;
                        await notificationService.SendAsync(NotificationListeners.updateNotificationPanel.ToString(), user.UserPublicId.ToString(), notificationData);
                        var redirectUrl = notificationService.GetNotificationUrl(notificationId);
                        await CustomEmail<ProjectService>.SendReminder(
                            user.FullName, user.Email, (int)Helper.EmailTemplates.ProjectDueTomorow, "tomorrow", "",
                            project.ProjectName, project.DueDate.Value.ToShortTimeString(), redirectUrl, logger, config, environment);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<List<ProjectPermissionDto>> GetAllPermissions()
        {
            try
            {
                var permissions = await projectPermissionRepository.GetAllListAsync();
                var result = mapper.Map<List<ProjectPermissionDto>>(permissions);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// Get All Existing Permissions of Project Members
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public List<ProjectMemberPermissionDto> GetAllUserPermissions(string projectId)
        {
            try
            {
                var project = GetProjectByPublicId(projectId);
                var memberPermissions = projectMemberPermissionRepository.GetAll()
                    .Include(s => s.MemberUser)
                    .Include(s => s.ProjectPermission)
                    .Where(s => s.ProjectId == project.ProjectId && s.CreatedBy != s.MemberUserId).ToList();
                var result = mapper.Map<List<ProjectMemberPermissionDto>>(memberPermissions);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void UpdateProjectPermissions(string projectId, List<UpdateProjectPermissionRequest> updateProjectPermissionRequest, int? currentUserId)
        {
            try
            {
                var project = GetProjectByPublicId(projectId);
                var projectMemberPermissions = projectMemberPermissionRepository.GetAll()
                    .Include(s => s.MemberUser)
                    .Where(s => s.ProjectId == project.ProjectId && s.CreatedBy != s.MemberUserId).ToList();

                var temp = projectMemberPermissions
                    .Where(s => updateProjectPermissionRequest.Any(x => x.Email == s.MemberUser.Email && x.PermissionId == s.ProjectPermissionId)).ToList();
                var removedPermissions = projectMemberPermissions.Except(temp).ToList();
                if (removedPermissions.Count > 0)
                    RemovePermissions(removedPermissions, currentUserId);

                var temp1 = updateProjectPermissionRequest
                    .Where(s => temp.Any(x => x.MemberUser.Email == s.Email && x.ProjectPermissionId == s.PermissionId)).ToList();
                updateProjectPermissionRequest = updateProjectPermissionRequest.Except(temp1).ToList();
                if (updateProjectPermissionRequest.Count > 0)
                    AddPermissions(updateProjectPermissionRequest, project.ProjectId, currentUserId);

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        /// <summary>
        /// Get Permissions To Toggle On Card and Challenge views.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public async Task<List<ProjectPermissionAllowedDto>> GetProjectMemberPermissions(string projectId, int? currentUserId)
        {
            try
            {
                var project = GetProjectByPublicId(projectId);
                var projectmemberPermisisons = await projectMemberPermissionRepository.GetAll().Where(s => s.ProjectId == project.ProjectId && s.MemberUserId == currentUserId).ToListAsync();
                var projectPermissions = await projectPermissionRepository.GetAllListAsync();
                List<ProjectPermissionAllowedDto> projectPermissionAllowedDto = new List<ProjectPermissionAllowedDto>();
                foreach (var item in projectPermissions)
                {
                    projectPermissionAllowedDto.Add(new ProjectPermissionAllowedDto
                    {
                        PermissionCode = item.ProjectPermissionCode,
                        IsAllowed = projectmemberPermisisons.Select(s => s.ProjectPermissionId).Contains(item.ProjectPermissionId) ? true : false
                    });
                }
                return projectPermissionAllowedDto;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region Private

        private async Task<Dictionary<string, double>> GetTotalTasksCount(int projectId)
        {
            var challenges = await challengeRepository.GetAll().Where(s => s.ProjectId == projectId).ToListAsync();
            var challengeIds = challenges.Select(s => s.ChallengeId).ToList();
            var tasks = await cardRepository.GetAll().Where(s => challengeIds.Contains(s.ChallengeId.Value)).ToListAsync();
            int count = tasks.Count();
            if (count == 0)
            {
                return new Dictionary<string, double>() { { "Count", count }, { "Progress", (double)challenges.Where(s => s.IsCompleted == true).Count() / challenges.Count() * 100 } };
            }
            else
            {
                return new Dictionary<string, double>() { { "Count", count }, { "Progress",  (double)challenges.Where(s => s.IsCompleted == true).Count()/challenges.Count()*100 } };
            }
        }
        private int AddProjectTeam(string teamName, int? userId, int projectId)
        {
            try
            {
                var teamId = teamService.GetUserTeamIdByName(teamName, userId);
                var exist = projectTeamRepository.GetAll().FirstOrDefault(s => s.TeamId == teamId && s.ProjectId == projectId);
                if (exist != null)
                    return exist.ProjectTeamId;
                var projectTeam = new ProjectTeam()
                {
                    CreatedBy = userId,
                    ProjectId = projectId,
                    TeamId = teamId
                };
                var projectTeamId = projectTeamRepository.InsertAndGetId(projectTeam);

                var teamMembers = teamMemberRepository.GetAll().Include(s => s.MemberUser).Where(s => s.TeamId == teamId).ToList();
                var notificationFieldTypes = notificationFieldTypesRepository.GetAllListAsync().Result;
                var project = projectRepository.Get(projectId).Result;
                foreach (var item in teamMembers)
                {
                    NotificationField notificationField = new NotificationField
                    {
                        NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{ProjectName}").NotificationFieldTypeId,
                        Name = notificationFieldTypes.First(s => s.Text == "{ProjectName}").Type,
                        Value = project.ProjectName
                    };
                    NotificationField notificationField1 = new NotificationField
                    {
                        ProjectId = project.ProjectId
                    };
                    var fields = new List<NotificationField>();
                    fields.Add(notificationField);
                    fields.Add(notificationField1);
                    var notification = new AddNotificationRequest
                    {
                        Fields = fields,
                        FromUserId = userId,
                        NotificationTypeId = notificationService.GetNotificationTypeId(NotificationTypes.AddedInProject.ToString()).NotificationTypeId,
                        ToUserId = item.MemberUser.UserId
                    };
                    notificationService.GenerateNotification(notification);
                }

                return projectTeamId;
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        private async Task RemoveProjectTeam(ProjectTeam projectTeam, int? userId)
        {
            projectTeam.UpdatedBy = userId;
            await projectTeamRepository.Delete(projectTeam);
        }
        private async Task AddRegisteredMember(User member, int? userId, Project project)
        {
            try
            {
                var exist = projectMemberRepository.GetAll().FirstOrDefault(s => s.ProjectMemberId == member.UserId && s.ProjectId == project.ProjectId);
                if (exist != null)
                    return;
                var projectMember = new ProjectMember()
                {
                    CreatedBy = userId,
                    ProjectId = project.ProjectId,
                    ProjectMemberUserId = member.UserId
                };
                projectMember = projectMemberRepository.Insert(projectMember);
                var inviteLink = config["BaseUrlWeb"] + "#/auth/login?p=" + project.ProjectPublicId.ToString();

                await projectMemberRepository.Update(projectMember);

                await teamService.SendInviteEmail(member.Email, project.ProjectName, inviteLink);
                var notificationFieldTypes = await notificationFieldTypesRepository.GetAllListAsync();
                NotificationField notificationField = new NotificationField
                {
                    NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{ProjectName}").NotificationFieldTypeId,
                    Name = notificationFieldTypes.First(s => s.Text == "{ProjectName}").Type,
                    Value = project.ProjectName
                };
                NotificationField notificationField1 = new NotificationField
                {
                    ProjectId = project.ProjectId
                };
                var fields = new List<NotificationField>();
                fields.Add(notificationField);
                fields.Add(notificationField1);
                var notification = new AddNotificationRequest
                {
                    Fields = fields,
                    FromUserId = userId,
                    NotificationTypeId = notificationService.GetNotificationTypeId(NotificationTypes.AddedInProject.ToString()).NotificationTypeId,
                    ToUserId = member.UserId
                };
                var notificationId = await notificationService.GenerateNotification(notification);
                var data = await notificationService.GetNotificationText(notificationId);
                await notificationService.SendAsync(NotificationListeners.addedInProject.ToString(), member.UserPublicId.ToString(), data);
                var notificationData = await notificationService.GetNotifications(member.UserId, 10, 0);
                await notificationService.SendAsync(NotificationListeners.updateNotificationPanel.ToString(), member.UserPublicId.ToString(), notificationData);
            }
            catch (Exception)
            {

                throw;
            }

        }
        private async Task RemoveRegisteredMember(ProjectMember projectMember, int? userId)
        {
            try
            {
                projectMember.UpdatedBy = userId;
                await projectMemberRepository.Delete(projectMember);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task AddUnregisteredMember(string email, int? userId, Project project)
        {
            try
            {
                var exist = tempProjectMemberRepository.GetAll().FirstOrDefault(s => s.Email == email && s.ProjectId == project.ProjectId);
                if (exist != null)
                    return;
                var projectMember = new TemporaryProjectMember()
                {
                    CreatedBy = userId,
                    ProjectId = project.ProjectId,
                    Email = email.Trim()
                };
                projectMember = await tempProjectMemberRepository.InsertAsync(projectMember);
                var inviteLink = config["BaseUrlWeb"] + "#/auth/register?p=" + projectMember.TemporaryProjectMemberPublicId.ToString();
                await teamService.SendInviteEmail(email, project.ProjectName, inviteLink);
            }
            catch (Exception)
            {

                throw;
            }

        }
        private async Task CreateChallengeTemplatesForProject(int projectId, int count, int? userId, int oldCount=0)
        {
            try
            {
                for (int i = oldCount == 0 ? 1 : oldCount; i <= count; i++)
                {
                    var challenge = new Challenge()
                    {
                        ChallengeName = ("Challenge " + i.ToString()).Trim(),
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        IsCompleted = false,
                        IsLocked = i == 1 ? false : true,
                        UnlockDate = i == 1 ? DateTime.Now : null,
                        ProjectId = projectId
                    };
                    await challengeRepository.InsertAsync(challenge);
                }
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        private async Task RemoveChallengeTemplatesFromProject(int projectId, int count, int? userId, int oldCount = 0)
        {
            try
            {
                var challenges = await challengeRepository.GetAll().Where(s => s.ProjectId == projectId && s.IsLocked == true).OrderByDescending(s => s.ChallengeId).Take(oldCount - count).ToListAsync();
                var chIds = challenges.Select(s => s.ChallengeId).ToList();
                var challengeList = await challengeListRepository.GetAll().Where(s => chIds.Contains(s.ChallengeId.Value)).ToListAsync();
                var cards = await cardRepository.GetAll().Where(s => chIds.Contains(s.ChallengeId.Value)).ToListAsync();
                var checkList = checkListRepository.GetAll().Where(s => cards.Select(s => s.CardId).Contains(s.CardId.Value)).ToList();

                foreach (var item in checkList)
                {
                    item.UpdatedBy = userId;
                    await checkListRepository.Delete(item);
                }
                foreach (var item in cards)
                {
                    item.UpdatedBy = userId;
                    await cardRepository.Delete(item);
                }
                foreach (var item in challengeList)
                {
                    item.UpdatedBy = userId;
                    await challengeListRepository.Delete(item);
                }
                foreach (var item in challenges)
                {
                    item.UpdatedBy = userId;
                    await challengeRepository.Delete(item);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        private async Task<bool> IsProjectOwner(ProjectListDto projectListDto, List<Project> projects, int? currentUserId)
        {
            try
            {
                var project = projects.First(s => s.ProjectPublicId.ToString() == projectListDto.Id);
                if (project.CreatedBy == currentUserId)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task AddAdminPermissions(int projectId, int currentUserId)
        {
            try
            {
                var permissions = await projectPermissionRepository.GetAllListAsync();
                for (int i = 0; i < permissions.Count; i++)
                {
                    var memberPermission = new ProjectMemberPermission
                    {
                        CreatedBy = currentUserId,
                        MemberUserId = currentUserId,
                        ProjectId = projectId,
                        ProjectPermissionId = permissions[i].ProjectPermissionId,
                    };
                    await projectMemberPermissionRepository.InsertAsync(memberPermission);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void RemovePermissions(List<ProjectMemberPermission> projectMemberPermissions, int? currentUserId)
        {
            try
            {
                foreach (var item in projectMemberPermissions)
                {
                    item.UpdatedBy = currentUserId;
                    projectMemberPermissionRepository.Delete(item);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void AddPermissions(List<UpdateProjectPermissionRequest> updateProjectPermissionRequests, int projectId, int? currentUserId)
        {
            try
            {
                foreach (var item in updateProjectPermissionRequests)
                {
                    var user = userService.GetUserByEmail(item.Email);
                    var memberPermission = new ProjectMemberPermission
                    {
                        CreatedBy = currentUserId,
                        MemberUserId = user.UserId,
                        ProjectId = projectId,
                        ProjectPermissionId = item.PermissionId,
                    };
                    projectMemberPermissionRepository.Insert(memberPermission);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<List<ProjectParticipants>> GetProjectRegisteredMembers(int projectId)
        {
            try
            {
                var members = await projectMemberRepository.GetAll()
                    .Include(s => s.ProjectMemberUser)
                    .Where(s => s.ProjectId == projectId && s.CreatedBy != s.ProjectMemberUserId)
                    .Select(s => new ProjectParticipants
                    {
                        TeamName = s.CreatedBy == s.ProjectMemberUser.UserId ? "Admin" : "Individual User",
                        Name = s.ProjectMemberUser.FullName,
                        Email = s.ProjectMemberUser.Email,
                        IsTeamMember = false
                    })
                    .ToListAsync();

                return members;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<List<ProjectParticipants>> GetProjectTempMembers(int projectId)
        {
            try
            {
                var members = await tempProjectMemberRepository.GetAll()
                    .Where(s => s.ProjectId == projectId)
                    .Select(s => new ProjectParticipants
                    {
                        TeamName = "Individual User",
                        Name = "Pending Registration",
                        Email = s.Email,
                        IsTeamMember = false
                    })
                    .ToListAsync();
                return members;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<List<ProjectParticipants>> GetProjectTeamMembers(int projectId)
        {
            try
            {
                var teamIds = projectTeamRepository.GetAll().Where(s => s.ProjectId == projectId).Select(s => s.TeamId).ToList();

                var members = await teamMemberRepository.GetAll()
                    .Include(s => s.MemberUser)
                    .Include(s => s.Team)
                    .Where(s => teamIds.Contains(s.TeamId) && s.CreatedBy != s.MemberUserId)
                    .Select(s => new ProjectParticipants
                    {
                        TeamName = s.Team.TeamName,
                        Name = s.MemberUser.FullName,
                        Email = s.MemberUser.Email,
                        IsTeamMember = true
                    })
                    .ToListAsync();

                var tempMembers = await tempTeamMemberRepository.GetAll()
                    .Include(s => s.Team)
                    .Where(s => teamIds.Contains(s.TeamId))
                    .Select(s => new ProjectParticipants
                    {
                        TeamName = s.Team.TeamName,
                        Name = "Pending Registration",
                        Email = s.Email,
                        IsTeamMember = true
                    })
                    .ToListAsync();

                members.AddRange(tempMembers);
                return members;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #endregion

        #endregion
    }
}
