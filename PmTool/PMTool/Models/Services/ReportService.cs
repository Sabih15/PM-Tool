using AutoMapper;
using DAL.Models;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PMTool.Models.DTOs;
using PMTool.Models.Enums;
using PMTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Services
{
    public class ReportService : IReportService
    {
        #region Fields

        private readonly IRepository<Project> projectRepository;
        private readonly IRepository<ProjectTeam> projectTeamRepository;
        private readonly IRepository<ProjectMember> projectMemberRepository;
        private readonly IRepository<TemporaryProjectMember> tempProjectMemberRepository;
        private readonly IRepository<TeamMember> teamMemberRepository;
        private readonly IRepository<TemporaryTeamMember> tempTeamMemberRepository;
        private readonly IRepository<Challenge> challengeRepository;
        private readonly IRepository<UserChallengeDuration> userChallengeDurationRepository;
        private readonly IRepository<CardAssignedMember> cardAssignedMemberRepository;
        private readonly IProjectService projectService;
        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly IConfiguration config;

        #endregion

        #region Constructors

        public ReportService(
            IRepository<Project> _projectRepository,
            IRepository<ProjectTeam> _projectTeamRepository,
            IRepository<ProjectMember> _projectMemberRepository,
            IRepository<TemporaryProjectMember> _tempProjectMemberRepository,
            IRepository<TeamMember> _teamMemberRepository,
            IRepository<TemporaryTeamMember> _tempTeamMemberRepository,
            IRepository<Challenge> _challengeRepository,
            IRepository<UserChallengeDuration> _userChallengeDurationRepository,
            IRepository<CardAssignedMember> _cardAssignedMemberRepository,
            IProjectService _projectService,
            IUserService _userService,
            IMapper _mapper,
            IConfiguration _config
            )
        {
            projectRepository = _projectRepository;
            projectTeamRepository = _projectTeamRepository;
            projectMemberRepository = _projectMemberRepository;
            tempProjectMemberRepository = _tempProjectMemberRepository;
            teamMemberRepository = _teamMemberRepository;
            tempTeamMemberRepository = _tempTeamMemberRepository;
            challengeRepository = _challengeRepository;
            userChallengeDurationRepository = _userChallengeDurationRepository;
            cardAssignedMemberRepository = _cardAssignedMemberRepository;
            projectService = _projectService;
            userService = _userService;
            mapper = _mapper;
            config = _config;
        }

        #endregion

        #region Methods

        #region Private

        #region ProjectMatrix

        private async Task<Dictionary<string, int>> GetProjectMatrix(int? currentUserId)
        {
            try
            {
                var projects = await GetTotalProjectsIds(currentUserId);
                var myProjectsCount = await GetMyProjectsCount(projects, currentUserId);
                var completedProjects = await GetCompletedProjectsCount(projects);
                var totalProjectsCount = projects.Count();
                var otherProjectsCount = totalProjectsCount - myProjectsCount;
                var dict = new Dictionary<string, int>();
                dict.Add("Total Projects", totalProjectsCount);
                dict.Add("My Projects", myProjectsCount);
                dict.Add("Other Projects", otherProjectsCount);
                dict.Add("Completed Projects", completedProjects);
                return dict;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<List<int>> GetTotalProjectsIds(int? currentUserId)
        {
            try
            {
                var projects = await projectMemberRepository.GetAll()
                    .Include(s => s.Project)
                    .Where(s => s.Project.IsActive == true && s.ProjectMemberUserId == currentUserId).Select(s => s.ProjectId).ToListAsync();
                var teamIds = await teamMemberRepository.GetAll().Where(s => s.MemberUserId == currentUserId).Select(s => s.TeamId).ToListAsync();
                var teamProjects = await projectTeamRepository.GetAll()
                    .Include(s => s.Project)
                    .Where(s => s.Project.IsActive == true && teamIds.Contains(s.TeamId)).Select(s => s.ProjectId.Value).ToListAsync();
                var projectIds = new List<int>();
                projectIds.AddRange(projects);
                projectIds.AddRange(teamProjects);
                projectIds = projectIds.Distinct().ToList();
                return projectIds;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<int> GetMyProjectsCount(List<int> projectIds, int? currentUserId)
        {
            try
            {
                var count = await projectRepository.GetAll().Where(s => projectIds.Contains(s.ProjectId) && s.CreatedBy == currentUserId).CountAsync();
                return count;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<int> GetCompletedProjectsCount(List<int> projectIds)
        {
            try
            {
                int count = 0;
                var challenges = await challengeRepository.GetAll().Where(s => projectIds.Contains(s.ProjectId.Value)).ToListAsync();
                foreach (var item in challenges.GroupBy(s => s.ProjectId).ToList())
                {
                    var list = item.Select(s => s.IsCompleted).ToList();
                    if (!list.Contains(false) && !list.Contains((bool?)null))
                        count++;
                }
                return count;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region TeamMatrix

        private async Task<Dictionary<string, int>> GetTeamMatrix(int? currentUserId)
        {
            try
            {
                var teams = await teamMemberRepository.GetAll()
                    .Include(s => s.Team)
                    .Where(s => s.Team.IsActive == true && s.MemberUserId == currentUserId).ToListAsync();
                var totalTeamsCount = teams.Count();
                var myTeamsCount = teams.Where(s => s.CreatedBy == currentUserId).Count();
                var otherTeamsCount = totalTeamsCount - myTeamsCount;
                var dict = new Dictionary<string, int>();
                dict.Add("Total Teams", totalTeamsCount);
                dict.Add("My Teams", myTeamsCount);
                dict.Add("Other Teams", otherTeamsCount);
                return dict;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region TaskMatrix

        private async Task<Dictionary<string, int>> GetTaskMatrix(int? currentUserId)
        {
            try
            {
                var tasks = await cardAssignedMemberRepository.GetAll().Include(s => s.Card).Where(s => s.MemberUserId == currentUserId).ToListAsync();
                var totalTasksCount = tasks.Count();
                var notStartedCount = tasks.Where(s => s.Card.CardStatus == null).Count();
                var inprogressCount = tasks.Where(s => s.Card.CardStatus != null && (int)CardStatus.InProgress == s.Card.CardStatus.Value).Count();
                var completedCount = tasks.Where(s => s.Card.CardStatus != null && (int)CardStatus.Completed == s.Card.CardStatus.Value).Count();

                var dict = new Dictionary<string, int>();
                dict.Add("Total Tasks", totalTasksCount);
                dict.Add("Not Started Tasks", notStartedCount);
                dict.Add("InProgress Tasks", inprogressCount);
                dict.Add("Completed Tasks", completedCount);
                return dict;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        private async Task<int> GetCompleteChallengeCount(int projectId)
        {
            try
            {
                var count = await challengeRepository.GetAll().Where(s => s.ProjectId == projectId && s.IsCompleted == true).CountAsync();
                return count;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<List<ProjectMemberDto>> GetProjecMembers(int projectId)
        {
            try
            {
                var configuration = config.GetValue<string>("ServerUrl");
                var result = new List<ProjectMemberDto>();
                var members = await projectMemberRepository.GetAll()
                    .Include(s => s.ProjectMemberUser)
                    .Where(s => s.ProjectId == projectId)
                    .Select(s => new ProjectMemberDto
                    {
                        isIndividualMember = true,
                        MemberEmail = s.ProjectMemberUser.Email,
                        MemberName = s.ProjectMemberUser.FullName,
                        //Picture = s.ProjectMemberUser.PictureURL,
                        //Picture = Path.Combine(config.GetValue<string>("ServerUrl"), s.ProjectMemberUser.PictureURL.TrimStart('\\')),
                        Picture = string.IsNullOrEmpty(s.ProjectMemberUser.PictureURL) ? "" :  configuration +  s.ProjectMemberUser.PictureURL,
                        TeamName = "Individual User"
                    }).ToListAsync();

                var tempMembers = await tempProjectMemberRepository.GetAll()
                    .Where(s => s.ProjectId == projectId)
                    .Select(s => new ProjectMemberDto
                    {
                        isIndividualMember = true,
                        MemberEmail = s.Email,
                        MemberName = null,
                        Picture = null,
                        TeamName = "Individual User"
                    }).ToListAsync();

                var configurations = config.GetValue<string>("ServerUrl");
                var teamIds = await projectTeamRepository.GetAll().Where(s => s.ProjectId == projectId).Select(s => s.TeamId).ToListAsync();
                var teamMembers = await teamMemberRepository.GetAll()
                    .Include(s => s.MemberUser)
                    .Include(s => s.Team)
                    .Where(s => teamIds.Contains(s.TeamId))
                    .Select(s => new ProjectMemberDto
                    {
                        isIndividualMember = false,
                        MemberEmail = s.MemberUser.Email,
                        MemberName = s.MemberUser.FullName,
                        //Picture = s.MemberUser.PictureURL,
                        //Picture = Path.Combine(config.GetValue<string>("ServerUrl"), s.MemberUser.PictureURL.TrimStart('\\')),
                        Picture = string.IsNullOrEmpty(s.MemberUser.PictureURL) ? "" : configurations +  s.MemberUser.PictureURL,
                        TeamName = s.Team.TeamName
                    }).ToListAsync();
                var tempTeamMembers = await tempTeamMemberRepository.GetAll()
                    .Include(s => s.Team)
                    .Where(s => teamIds.Contains(s.TeamId))
                    .Select(s => new ProjectMemberDto
                    {
                        isIndividualMember = false,
                        MemberEmail = s.Email,
                        MemberName = null,
                        Picture = null,
                        TeamName = s.Team.TeamName
                    }).ToListAsync();

                foreach (var item in members)
                {
                    if (result.Select(s => s.MemberEmail).Contains(item.MemberEmail) == false)
                    {
                        result.Add(item);
                    }
                    else
                    {
                        var ind = result.IndexOf(result.First(s => s.MemberEmail == item.MemberEmail));
                        result[ind].TeamName += ", " + item.TeamName;
                    }
                }
                foreach (var item in tempMembers)
                {
                    if (result.Select(s => s.MemberEmail).Contains(item.MemberEmail) == false)
                    {
                        result.Add(item);
                    }
                    else
                    {
                        var ind = result.IndexOf(result.First(s => s.MemberEmail == item.MemberEmail));
                        result[ind].TeamName += ", " + item.TeamName;
                    }
                }
                foreach (var item in teamMembers)
                {
                    if (result.Select(s => s.MemberEmail).Contains(item.MemberEmail) == false)
                    {
                        result.Add(item);
                    }
                    else
                    {
                        var ind = result.IndexOf(result.First(s => s.MemberEmail == item.MemberEmail));
                        result[ind].TeamName += ", " + item.TeamName;
                    }
                }
                foreach (var item in tempTeamMembers)
                {
                    if (result.Select(s => s.MemberEmail).Contains(item.MemberEmail) == false)
                    {
                        result.Add(item);
                    }
                    else
                    {
                        var ind = result.IndexOf(result.First(s => s.MemberEmail == item.MemberEmail));
                        result[ind].TeamName += ", " + item.TeamName;
                    }
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<string> GetTotalProjectDuration(int projectId)
        {
            try
            {
                var challengeIds = challengeRepository.GetAll().Where(s => s.ProjectId == projectId).Select(s => s.ChallengeId).ToList();
                var durationInMins = await userChallengeDurationRepository.GetAll().Where(s => challengeIds.Contains(s.ChallengeId.Value)).SumAsync(s => s.Duration);
                TimeSpan time = TimeSpan.FromMinutes(durationInMins.Value);
                string timeString = "";
                if (time.Hours > 0)
                    timeString = time.Hours.ToString() + "Hours";
                if (time.Minutes > 0)
                    timeString += time.Minutes.ToString() + "Mins";
                if (timeString == "")
                    timeString = "N/A";
                return timeString;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<List<ChallengeReportDto>> GetChallengeReportData(string projectId)
        {
            try
            {
                var result = new List<ChallengeReportDto>();
                var project = projectService.GetProjectByPublicId(projectId);
                var challenges = challengeRepository.GetAll().Where(s => s.ProjectId == project.ProjectId).ToList();
                foreach (var item in challenges)
                {
                    var resultItem = mapper.Map<ChallengeReportDto>(item);
                    var duration = await userChallengeDurationRepository.GetAll().Where(s => s.ChallengeId == item.ChallengeId).SumAsync(s => s.Duration);
                    TimeSpan time = TimeSpan.FromMinutes(duration.Value);
                    string timeString = "";
                    if (time.Hours > 0)
                        timeString = time.Hours.ToString() + "Hours";
                    if (time.Minutes > 0)
                        timeString += time.Minutes.ToString() + "Mins";
                    if (timeString == "")
                        timeString = "N/A";
                    resultItem.Duration = timeString;
                    resultItem.Members = await GetMembersReport(item.ChallengeId);
                    result.Add(resultItem);
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<List<MembersReportDto>> GetMembersReport(int challengeId)
        {
            try
            {
                var result = new List<MembersReportDto>();
                var temp = await userChallengeDurationRepository.GetAll()
                    .Include(s => s.User)
                    .Include(s => s.Challenge)
                    .Where(s => s.ChallengeId == challengeId)
                    .ToListAsync();

                foreach (var item in temp.GroupBy(s => s.UserId).ToList())
                {
                    var duration = item.Sum(s => s.Duration);
                    TimeSpan time = TimeSpan.FromMinutes(duration.Value);
                    string timeString = "";
                    if (time.Hours > 0)
                        timeString = time.Hours.ToString() + "Hours";
                    if (time.Minutes > 0)
                        timeString += time.Minutes.ToString() + "Mins";
                    if (timeString == "")
                        timeString = "N/A";
                    string teamName;
                    var x = projectMemberRepository.GetAll().FirstOrDefault(s => item.First().Challenge.ProjectId == s.ProjectId);
                    if (x != null)
                        teamName = "Individual Member";
                    else
                    {
                        var teams = teamMemberRepository.GetAll().Where(s => s.MemberUserId == item.Key).Select(s => s.TeamId).ToList();
                        var projectteam = projectTeamRepository.GetAll().Include(s => s.Team).Where(s => teams.Contains(s.TeamId)).FirstOrDefault();
                        teamName = projectteam.Team.TeamName;
                    }
                    var configuration = config.GetValue<string>("ServerUrl");
                    result.Add(new MembersReportDto
                    {
                        Email = item.First().User.Email,
                        Name = item.First().User.FullName,
                        //Picture = item.First().User.PictureURL,
                        //Picture = Path.Combine(config.GetValue<string>("ServerUrl"), item.First().User.PictureURL.TrimStart('\\')),
                        Picture = string.IsNullOrEmpty(item.First().User.PictureURL) ? "" : configuration +  item.First().User.PictureURL,
                        TeamName = teamName,
                        Duration = timeString
                    });
                }

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }


        #endregion

        #region Public

        public async Task<List<GeneralDto>> GetUserProjects(int? currentUserId)
        {
            try
            {
                var result = new List<GeneralDto>();
                var teamIds = await teamMemberRepository.GetAll().Where(s => s.MemberUserId == currentUserId).Select(s => s.TeamId).ToListAsync();
                var l1 = await projectMemberRepository.GetAll()
                    .Include(s => s.Project)
                    .Where(s => s.ProjectMemberUserId == currentUserId && s.Project.IsActive == true)
                    .Select(s => new GeneralDto
                    {
                        Id = s.Project.ProjectPublicId,
                        Name = s.Project.ProjectName
                    }).ToListAsync();
                var l2 = await projectTeamRepository.GetAll()
                    .Include(s => s.Project)
                    .Where(s => teamIds.Contains(s.TeamId) && s.Project.IsActive == true)
                    .Select(s => new GeneralDto
                    {
                        Id = s.Project.ProjectPublicId,
                        Name = s.Project.ProjectName
                    }).ToListAsync();

                foreach (var item in l1)
                {
                    if (!result.Select(s => s.Id).Contains(item.Id))
                    {
                        result.Add(item);
                    }
                }
                foreach (var item in l2)
                {
                    if (!result.Select(s => s.Id).Contains(item.Id))
                    {
                        result.Add(item);
                    }
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetPerformanceMatrix(int? currentUserId)
        {
            try
            {
                var projectMatrix = await GetProjectMatrix(currentUserId);
                var teamMatrix = await GetTeamMatrix(currentUserId);
                var taskMatrix = await GetTaskMatrix(currentUserId);

                var result = new Dictionary<string, int>();
                foreach (var item in projectMatrix)
                {
                    result.Add(item.Key, item.Value);
                }
                foreach (var item in teamMatrix)
                {
                    result.Add(item.Key, item.Value);
                }
                foreach (var item in taskMatrix)
                {
                    result.Add(item.Key, item.Value);
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ProjectReportDto>> GetProjectReport(int? currentUserId)
        {
            try
            {
                List<ProjectReportDto> result = new List<ProjectReportDto>();
                var projectIds = await GetTotalProjectsIds(currentUserId);
                var projects = await projectRepository.GetAll().Where(s => projectIds.Contains(s.ProjectId)).ToListAsync();
                foreach (var item in projects)
                {
                    var resultItem = mapper.Map<ProjectReportDto>(item);
                    resultItem.CompletedChallengeCount = await GetCompleteChallengeCount(item.ProjectId);
                    resultItem.Progress = (double)resultItem.CompletedChallengeCount / resultItem.ChallengeCount * 100;
                    resultItem.Members = await GetProjecMembers(item.ProjectId);
                    resultItem.Duration = await GetTotalProjectDuration(item.ProjectId);
                    result.Add(resultItem);
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetProjectWiseReport(string projectId)
        {
            try
            {
                var challengeData = await GetChallengeReportData(projectId);
                var dict = new Dictionary<string, object>();
                dict.Add("Challenge", challengeData);
                return dict;
            }
            catch (Exception)
            {
                throw;
            }
        }



        #endregion

        #endregion
    }
}
