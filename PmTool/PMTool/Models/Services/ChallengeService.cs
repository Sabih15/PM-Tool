using AutoMapper;
using DAL.Models;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PMTool.Models.DTOs;
using PMTool.Models.Enums;
using PMTool.Models.General;
using PMTool.Models.Request;
using PMTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Services
{
    public class ChallengeService : IChallengeService
    {
        #region Fields

        private readonly IRepository<User> userRepository;
        private readonly IProjectService projectService;
        private readonly IUserService userService;
        private readonly ICommonService commonService;
        private readonly INotificationService notificationService;
        private readonly IRepository<Challenge> challengeRepository;
        private readonly IRepository<ChallengeList> challengeListRepository;
        private readonly IRepository<ProjectMember> projectMembersRepository;
        private readonly IRepository<TemporaryProjectMember> projectTempMembersRepository;
        private readonly IRepository<ProjectTeam> projectTeamRepository;
        private readonly IRepository<TeamMember> teamMemberRepository;
        private readonly IRepository<UserChallengeDuration> userChallengeDurationRepository;
        private readonly IRepository<TemporaryTeamMember> tempTeamMemberRepository;
        private readonly IRepository<NotificationFieldType> notificationFieldTypesRepository;
        private readonly IRepository<ProjectMemberPermission> projectMemberPermissionRepository;
        private readonly IRepository<ProjectPermission> projectPermissionRepository;
        private readonly IRepository<Card> cardRepository;
        private readonly IRepository<CheckList> checkListRepository;
        private readonly IRepository<CardAttachment> attachmentRepository;
        private readonly IConfiguration config;

        #endregion

        #region Constructors

        public ChallengeService(IRepository<User> _userRepository,
            IProjectService _projectService,
            IUserService _userService,
            ICommonService _commonService,
            INotificationService _notificationService,
            IRepository<Challenge> _challengeRepository,
            IRepository<ChallengeList> _challengeListRepository,
            IRepository<ProjectMember> _projectMembersRepository,
            IRepository<TemporaryProjectMember> _projectTempMembersRepository,
            IRepository<ProjectTeam> _projectTeamRepository,
            IRepository<TeamMember> _teamMemberRepository,
            IRepository<UserChallengeDuration> _userChallengeDurationRepository,
            IRepository<TemporaryTeamMember> _tempTeamMemberRepository,
            IRepository<NotificationFieldType> _notificationFieldTypesRepository,
            IRepository<ProjectMemberPermission> _projectMemberPermissionRepository,
            IRepository<ProjectPermission> _projectPermissionRepository,
            IRepository<Card> _cardRepository,
            IRepository<CheckList> _checkListRepository,
            IRepository<CardAttachment> _attachmentRepository,
            IConfiguration _config
            )
        {
            userRepository = _userRepository;
            projectService = _projectService;
            userService = _userService;
            commonService = _commonService;
            notificationService = _notificationService;
            challengeRepository = _challengeRepository;
            challengeListRepository = _challengeListRepository;
            projectMembersRepository = _projectMembersRepository;
            projectTempMembersRepository = _projectTempMembersRepository;
            projectTeamRepository = _projectTeamRepository;
            teamMemberRepository = _teamMemberRepository;
            userChallengeDurationRepository = _userChallengeDurationRepository;
            tempTeamMemberRepository = _tempTeamMemberRepository;
            notificationFieldTypesRepository = _notificationFieldTypesRepository;
            projectMemberPermissionRepository = _projectMemberPermissionRepository;
            projectPermissionRepository = _projectPermissionRepository;
            cardRepository = _cardRepository;
            checkListRepository = _checkListRepository;
            attachmentRepository = _attachmentRepository;
            config = _config;
        }

        #endregion

        #region Methods

        public async Task<Challenge> GetChallengeById(int challengeId)
        {
            var item = await challengeRepository.Get(challengeId);
            return item;
        }

        public async Task Search(string query, string challengeId)
        {

        }

        public async Task<ProjectDetailHeaderDto> GetProjectDetailHeader(string projectId, int? currentUserId)
        {
            try
            {
                ProjectDetailHeaderDto projectDetailHeaderDto = new ProjectDetailHeaderDto();
                projectDetailHeaderDto.Members = new List<ProjectMemberDto>();
                var x = new List<ProjectMemberDto>();
                var project = projectService.GetProjectByPublicId(projectId);
                var challenges = await challengeRepository.GetAll().Where(s => s.ProjectId == project.ProjectId).ToListAsync();
                var temp = await GetProjectMembers(project.ProjectId, currentUserId);
                var temp1 = await GetProjectTeamMembers(project.ProjectId, currentUserId);
                var temp2 = await GetProjectTempMembers(project.ProjectId, currentUserId);
                var totalTime = challenges.Where(s => s.TotalWorkingDurationInMinutes != null).Sum(s => s.TotalWorkingDurationInMinutes);
                var days = (int)(totalTime / (24 * 60));
                var hrs = (int)(totalTime / 60);
                projectDetailHeaderDto.TimeTracked = (days).ToString() + ' ' + (hrs % 24).ToString() + ':' + ((int)totalTime % 60).ToString();
                projectDetailHeaderDto.Name = project.ProjectName;
                projectDetailHeaderDto.StartDate = project.CreatedDate.Value;
                projectDetailHeaderDto.DueDate = project.DueDate.Value;
                projectDetailHeaderDto.IsOwner = project.CreatedBy == currentUserId ? true : false;
                x.AddRange(temp);
                x.AddRange(temp1);
                x.AddRange(temp2);
                projectDetailHeaderDto.Members = x.GroupBy(s => s.MemberEmail, (key, values) => new ProjectMemberDto()
                {
                    isIndividualMember = values.Count() > 1 ? false : values.First().isIndividualMember,
                    MemberEmail = values.First().MemberEmail,
                    MemberName = values.First().MemberName,
                    Picture = values.First().Picture,
                    TeamName = string.Join(',', values.Select(s => s.TeamName)).Contains("Admin") ? "Admin" : string.Join(',', values.Select(s => s.TeamName))
                }).ToList();
                return projectDetailHeaderDto;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<ProjectDetailDto>> GetChallengeDetails(string projectId, int? currentUserId)
        {
            try
            {
                var resultList = new List<ProjectDetailDto>();

                var project = projectService.GetProjectByPublicId(projectId);
                var challenges = challengeRepository.GetAll().Where(s => s.ProjectId == project.ProjectId).ToList();
                foreach (var challenge in challenges)
                {
                    var result = new ProjectDetailDto();
                    result.IsOwner = project.CreatedBy == currentUserId ? true : false;
                    result.ChallengeId = challenge.ChallengeId;
                    result.ChallengeName = challenge.ChallengeName;
                    result.IsLocked = challenge.IsLocked == true ? true : false;
                    result.IsCompleted = challenge.IsCompleted == true ? true : false;
                    var totalTime = GetTotalDurationOfUserChallenge(challenge.ChallengeId, currentUserId.Value);
                    var days = (int)(totalTime / (24 * 60));
                    var hrs = (int)(totalTime / 60);
                    result.TimeTracked = (days).ToString() + ' ' + (hrs % 24).ToString() + ':' + ((int)totalTime % 60).ToString();
                    result.ChallengeList = new List<ChallengeListDto>();

                    var chList = challengeListRepository.GetAll().Where(s => s.ChallengeId == challenge.ChallengeId).ToList();
                    foreach (var ch in chList)
                    {
                        var challengeListDto = new ChallengeListDto();
                        challengeListDto.ListName = ch.ChallengeListName;
                        challengeListDto.ListId = ch.ChallengeListId;
                        challengeListDto.CardList = new List<CardListDto>();

                        var cards = cardRepository.GetAll().Where(s => s.ChallengeListId == ch.ChallengeListId).ToList();
                        foreach (var card in cards)
                        {
                            var chListStatus = await GetCheckListCountWithStatusCount(card.CardId);

                            CardListDto cardListDto = new CardListDto();
                            cardListDto.CardId = card.CardId;
                            cardListDto.CardName = card.CardName;
                            cardListDto.DueDate = card.DueDate;
                            cardListDto.TotalAttachments = await attachmentRepository.GetAll().Where(s => s.CardId == card.CardId).CountAsync();
                            cardListDto.CheckListItems = chListStatus[0];
                            cardListDto.Status = card.CardStatus == null ? null : ((CardStatus)(card.CardStatus)).ToString();
                            cardListDto.CheckListItemsCompleted = chListStatus[1];
                            challengeListDto.CardList.Add(cardListDto);
                        }
                        result.ChallengeList.Add(challengeListDto);
                    }
                    resultList.Add(result);
                }
                return resultList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<ChallengeList> AddListToChallenege(int? challengeId, string listName, int? currentUserId)
        {

            try
            {
                var challengeList = new ChallengeList();
                challengeList.ChallengeId = challengeId;
                challengeList.ChallengeListName = listName;
                challengeList.CreatedBy = currentUserId;
                var result = await challengeListRepository.InsertAsync(challengeList);
                if (result != null)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Card> AddCardToList(int? listId, string cardName, int? currentUserId)
        {

            try
            {
                var card = new Card();
                var challengeId = challengeListRepository.GetAll().FirstOrDefault(x => x.ChallengeListId == listId).ChallengeId.Value;
                var currentUser = userService.GetUserById(currentUserId.Value);
                card.ChallengeId = challengeId;
                card.ChallengeListId = listId;
                card.CardName = cardName;
                card.CreatedBy = currentUserId;
                var result = await cardRepository.InsertAsync(card);
                var activity = new AddActivityLogRequest()
                {
                    ActivityDateTime = DateTime.Now.ToString(),
                    ActivityText = "<b>" + currentUser.FullName + "</b> added card <b>" + cardName + "</b>",
                    CardId = result.CardId
                };
                await commonService.AddActivityLog(activity, currentUserId);
                if (result != null)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ChallengeListDto> GetUpdatedList(ChallengeList ch)
        {
            var challengeListDto = new ChallengeListDto();
            challengeListDto.ListName = ch.ChallengeListName;
            challengeListDto.ListId = ch.ChallengeListId;
            challengeListDto.CardList = new List<CardListDto>();

            var cards = cardRepository.GetAll().Where(s => s.ChallengeListId == ch.ChallengeListId).ToList();
            foreach (var card in cards)
            {
                var chListStatus = await GetCheckListCountWithStatusCount(card.CardId);

                CardListDto cardListDto = new CardListDto();
                cardListDto.CardId = card.CardId;
                cardListDto.CardName = card.CardName;
                cardListDto.DueDate = card.DueDate;
                cardListDto.TotalAttachments = await attachmentRepository.GetAll().Where(s => s.CardId == card.CardId).CountAsync();
                cardListDto.CheckListItems = chListStatus[0];
                cardListDto.CheckListItemsCompleted = chListStatus[1];
                challengeListDto.CardList.Add(cardListDto);
            }

            return challengeListDto;
        }

        public async Task<CardListDto> GetUpdatedCard(Card card)
        {

            var chListStatus = await GetCheckListCountWithStatusCount(card.CardId);

            CardListDto cardListDto = new CardListDto();
            cardListDto.CardId = card.CardId;
            cardListDto.CardName = card.CardName;
            cardListDto.DueDate = card.DueDate;
            cardListDto.TotalAttachments = await attachmentRepository.GetAll().Where(s => s.CardId == card.CardId).CountAsync();
            cardListDto.CheckListItems = chListStatus[0];
            cardListDto.CheckListItemsCompleted = chListStatus[1];
            return cardListDto;
        }

        public async Task<bool> DeleteList(int? listId, int? currentUserId)
        {

            try
            {

                var challengeListObj = await challengeListRepository.Get(listId.Value);
                challengeListObj.UpdatedBy = currentUserId;
                await challengeListRepository.Delete(challengeListObj);

                var cards = cardRepository.GetAll().Where(s => s.ChallengeListId == listId.Value).ToList();
                foreach (var card in cards)
                {
                    card.UpdatedBy = currentUserId;
                    await cardRepository.Delete(card);
                    var checklist = checkListRepository.GetAll().Where(s => s.CardId == card.CardId).ToList();
                    foreach (var item in checklist)
                    {
                        item.UpdatedBy = currentUserId;
                        await checkListRepository.Delete(item);
                    }
                }

                return true;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string AddDuration(double mins, int challengeId, int? currentUserid)
        {
            try
            {
                var item = userChallengeDurationRepository.GetAll().Where(s => s.ChallengeId == challengeId && s.UserId == currentUserid).FirstOrDefault();
                if (item == null)
                {
                    item = new UserChallengeDuration
                    {
                        ChallengeId = challengeId,
                        UserId = currentUserid,
                        Duration = Convert.ToSingle(mins),
                        CreatedBy = currentUserid,
                    };

                    userChallengeDurationRepository.Insert(item);
                }
                else
                {
                    item.Duration += Convert.ToSingle(mins);
                    item.UpdatedBy = currentUserid;
                    userChallengeDurationRepository.Update(item);
                }
                var totalTime = item.Duration;
                var days = (int)(totalTime / (24 * 60));
                var hrs = (int)(totalTime / 60);
                var result = (days).ToString() + ' ' + (hrs % 24).ToString() + ':' + ((int)totalTime % 60).ToString();
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ChallengeListDto>> GetUpdatedChallengeLists(int challengeId)
        {
            try
            {
                var challengeListDtos = new List<ChallengeListDto>();
                var challengelists = challengeListRepository.GetAll().Where(s => s.ChallengeId == challengeId).ToList();
                foreach (var ch in challengelists)
                {
                    var challengeListDto = new ChallengeListDto();
                    challengeListDto.ListName = ch.ChallengeListName;
                    challengeListDto.ListId = ch.ChallengeListId;
                    challengeListDto.CardList = new List<CardListDto>();

                    var cards = cardRepository.GetAll().Where(s => s.ChallengeListId == ch.ChallengeListId).ToList();
                    foreach (var card in cards)
                    {
                        var chListStatus = await GetCheckListCountWithStatusCount(card.CardId);

                        CardListDto cardListDto = new CardListDto();
                        cardListDto.CardId = card.CardId;
                        cardListDto.CardName = card.CardName;
                        cardListDto.DueDate = card.DueDate;
                        cardListDto.TotalAttachments = await attachmentRepository.GetAll().Where(s => s.CardId == card.CardId).CountAsync();
                        cardListDto.CheckListItems = chListStatus[0];
                        cardListDto.CheckListItemsCompleted = chListStatus[1];
                        cardListDto.Status = card.CardStatus == null ? null : ((CardStatus)(card.CardStatus)).ToString();
                        challengeListDto.CardList.Add(cardListDto);
                    }
                    challengeListDtos.Add(challengeListDto);
                }
                return challengeListDtos;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void UpdateListName(int listId, string listName, int? currentUserId)
        {
            try
            {
                var chList = challengeListRepository.Get(listId).Result;
                chList.ChallengeListName = listName;
                chList.UpdatedBy = currentUserId;
                challengeListRepository.Update(chList);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> MarkChallengeComplete(int challengeId, int? currentuserId)
        {
            try
            {
                var challenges = challengeRepository.GetAll().Include(s => s.Project).Where(s => s.ChallengeId >= challengeId).ToList();
                var challenge = challenges[0];
                var nextChallenge = challenges.Count > 1 ? challenges[1] : null;
                challenge.IsCompleted = true;
                challenge.CompleteDate = DateTime.Now;
                challenge.UpdatedBy = currentuserId;
                await challengeRepository.Update(challenge);
                if (nextChallenge != null)
                {
                    nextChallenge.IsLocked = false;
                    challenge.UnlockDate = DateTime.Now;
                    nextChallenge.UpdatedBy = currentuserId;
                    await challengeRepository.Update(nextChallenge);
                }

                var members = await projectService.GetAllUsers(challenge.Project.ProjectPublicId.ToString());

                var notificationFieldTypes = notificationFieldTypesRepository.GetAllListAsync().Result;

                foreach (var member in members)
                {

                    NotificationField notificationField = new NotificationField
                    {
                        NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{ChallengeName}").NotificationFieldTypeId,
                        Name = notificationFieldTypes.First(s => s.Text == "{ChallengeName}").Type,
                        Value = challenge.ChallengeName
                    };
                    NotificationField notificationField1 = new NotificationField
                    {
                        NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{ProjectName}").NotificationFieldTypeId,
                        Name = notificationFieldTypes.First(s => s.Text == "{ProjectName}").Type,
                        Value = challenge.Project.ProjectName
                    };
                    NotificationField notificationField2 = new NotificationField
                    {
                        ProjectId = challenge.ProjectId
                    };
                    var fields = new List<NotificationField>();
                    fields.Add(notificationField);
                    fields.Add(notificationField1);
                    fields.Add(notificationField2);
                    var notification = new AddNotificationRequest
                    {
                        Fields = fields,
                        FromUserId = currentuserId,
                        NotificationTypeId = notificationService.GetNotificationTypeId(NotificationTypes.ChallengeMarkComplete.ToString()).NotificationTypeId,
                        ToUserId = userService.GetUserByPublicId(member.Id.ToString()).UserId
                    };
                    var notificationId = await notificationService.GenerateNotification(notification);
                    var data = await notificationService.GetNotificationText(notificationId);
                    await notificationService.SendAsync(NotificationListeners.challengeComplete.ToString(), member.Id.ToString(), data);
                    var notificationData = await notificationService.GetNotifications(currentuserId, 10, 0);
                    await notificationService.SendAsync(NotificationListeners.updateNotificationPanel.ToString(), member.Id.ToString(), notificationData);
                }

                return true;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> MarkChallengeInComplete(int challengeId, int? currentuserId)
        {
            try
            {
                var challenge = challengeRepository.GetAll().FirstOrDefault(s => s.ChallengeId >= challengeId);
                challenge.IsCompleted = false;
                challenge.CompleteDate = null;
                challenge.UpdatedBy = currentuserId;
                await challengeRepository.Update(challenge);
                return true;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async Task<int[]> GetCheckListCountWithStatusCount(int cardId)
        {
            var list = await checkListRepository.GetAll().Where(s => s.CardId == cardId).ToListAsync();
            var parentIds = list.Where(s => s.ParentCheckListId != null).Select(s => s.ParentCheckListId.Value).ToList();
            var completed = list.Where(s => !parentIds.Contains(s.CheckListId) && s.IsCompleted.Value == true).Count();
            return new int[2] { list.Where(s => !parentIds.Contains(s.CheckListId)).Count(), completed };
        }

        private async Task<List<ProjectMemberDto>> GetProjectMembers(int projectId, int? currentUserId)
        {
            try
            {
                var configuration = config.GetValue<string>("ServerUrl");
                var members = await projectMembersRepository.GetAll()
                    .Include(s => s.ProjectMemberUser)
                    .Where(s => s.ProjectId == projectId)
                    .Select(s => new ProjectMemberDto
                    {
                        TeamName = s.CreatedBy == s.ProjectMemberUser.UserId ? "Admin" : "Individual User",
                        MemberName = s.ProjectMemberUser.FullName,
                        MemberEmail = s.ProjectMemberUser.Email,
                        //Picture = s.ProjectMemberUser.PictureURL,
                        //Picture = Path.Combine(config.GetValue<string>("ServerUrl"), s.ProjectMemberUser.PictureURL.TrimStart('\\')),
                        Picture = string.IsNullOrEmpty(s.ProjectMemberUser.PictureURL) ? "" : configuration +  s.ProjectMemberUser.PictureURL,
                        isIndividualMember = true
                    })
                    .ToListAsync();

                return members;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<List<ProjectMemberDto>> GetProjectTempMembers(int projectId, int? currentUserId)
        {
            try
            {
                var members = await projectTempMembersRepository.GetAll()
                    .Where(s => s.ProjectId == projectId)
                    .Select(s => new ProjectMemberDto
                    {
                        TeamName = "Individual User",
                        MemberName = "",
                        MemberEmail = s.Email,
                        Picture = null,
                        isIndividualMember = true
                    })
                    .ToListAsync();
                return members;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<List<ProjectMemberDto>> GetProjectTeamMembers(int projectId, int? currentUserId)
        {
            try
            {
                var teamIds = projectTeamRepository.GetAll().Where(s => s.ProjectId == projectId).Select(s => s.TeamId).ToList();
                var configuration = config.GetValue<string>("ServerUrl");
                var members = await teamMemberRepository.GetAll()
                    .Include(s => s.MemberUser)
                    .Include(s => s.Team)
                    .Where(s => teamIds.Contains(s.TeamId))
                    .Select(s => new ProjectMemberDto
                    {
                        TeamName = s.Team.TeamName,
                        MemberName = s.MemberUser.FullName,
                        MemberEmail = s.MemberUser.Email,
                        //Picture = s.MemberUser.PictureURL,
                        //Picture = Path.Combine(config.GetValue<string>("ServerUrl"), s.MemberUser.PictureURL.TrimStart('\\')),
                        Picture = string.IsNullOrEmpty(s.MemberUser.PictureURL) ? "" : configuration +  s.MemberUser.PictureURL,
                        isIndividualMember = false
                    })
                    .ToListAsync();

                var tempMembers = await tempTeamMemberRepository.GetAll()
                    .Include(s => s.Team)
                    .Where(s => teamIds.Contains(s.TeamId))
                    .Select(s => new ProjectMemberDto
                    {
                        TeamName = s.Team.TeamName,
                        MemberName = "",
                        MemberEmail = s.Email,
                        Picture = null,
                        isIndividualMember = false
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
        private double GetTotalDurationOfUserChallenge(int challengeId, int userId)
        {
            try
            {
                var item = userChallengeDurationRepository.GetAll().Where(s => s.ChallengeId == challengeId && s.UserId == userId).FirstOrDefault();
                if (item == null)
                    return 0;
                else
                    return Convert.ToDouble(item.Duration);
            }
            catch (Exception)
            {

                throw;
            }
        }


        #endregion
    }
}
