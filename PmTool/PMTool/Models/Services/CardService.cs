using AutoMapper;
using DAL.Models;
using DAL.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMTool.General;
using PMTool.Models.DTOs;
using PMTool.Models.Enums;
using PMTool.Models.General;
using PMTool.Models.Request;
using PMTool.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Services
{
    public class CardService : ICardService
    {
        #region Fields

        private readonly IRepository<ActivityLog> activityLogRepository;
        private readonly IRepository<Comments> commentsRepository;
        private readonly IRepository<CheckList> checkListRepository;
        private readonly IRepository<Card> cardRepository;
        private readonly IRepository<CardAssignedMember> cardAssignedMembersRepository;
        private readonly IRepository<CardAttachment> attachmentRepository;
        private readonly IRepository<User> userRepository;
        private readonly IRepository<ChallengeList> challengeListRepository;
        private readonly IRepository<NotificationFieldType> notificationFieldTypesRepository;
        private readonly IUserService userService;
        private readonly ICommonService commonService;
        private readonly INotificationService notificationService;
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<CardService> logger;
        private readonly IMapper mapper;


        #endregion

        #region Constructors

        public CardService(IRepository<ActivityLog> _activityLogRepository,
            IRepository<Comments> _commentsRepository,
            IRepository<CheckList> _checkListRepository,
            IRepository<Card> _cardRepository,
            IRepository<CardAssignedMember> _cardAssignedMembersRepository,
            IRepository<CardAttachment> _attachmentRepository,
            IRepository<User> _userRepository,
            IRepository<ChallengeList> _challengeListRepository,
            IRepository<NotificationFieldType> _notificationFieldTypesRepository,
            IUserService _userService,
            ICommonService _commonService,
            INotificationService _notificationService,
            IConfiguration _config,
            IWebHostEnvironment _environment,
            ILogger<CardService> _logger,
            IMapper _mapper)
        {
            activityLogRepository = _activityLogRepository;
            commentsRepository = _commentsRepository;
            checkListRepository = _checkListRepository;
            cardRepository = _cardRepository;
            cardAssignedMembersRepository = _cardAssignedMembersRepository;
            attachmentRepository = _attachmentRepository;
            userRepository = _userRepository;
            challengeListRepository = _challengeListRepository;
            notificationFieldTypesRepository = _notificationFieldTypesRepository;
            userService = _userService;
            commonService = _commonService;
            notificationService = _notificationService;
            config = _config;
            environment = _environment;
            logger = _logger;
            mapper = _mapper;
        }

        #endregion

        #region Methods

        #region Public

        public async Task<Dictionary<string, object>> GetCardListLog(int? cardId)
        {
            try
            {
                var result = new Dictionary<string, object>();
                result.Add("CheckList", await GetCheckList(cardId));
                result.Add("Activity", await GetCardActivities(cardId, 0));
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateDescription(int cardId, string description, int? currentUserId)
        {
            try
            {
                var card = await cardRepository.Get(cardId);
                card.Description = description;
                card.UpdatedBy = currentUserId;
                cardRepository.Update(card);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task<Dictionary<string, object>> GetCardDetails(int? cardId, int? currentUserId)
        {
            try
            {
                Dictionary<string, object> cardDetails = new Dictionary<string, object>();
                var cardMembers = await GetCardMembers(cardId);
                var cardAttachments = await GetCardAttachments(cardId);
                var cardDescription = await GetCardDescription(cardId);
                var cardComments = await GetcardComments(cardId, currentUserId);
                var cardStatus = await GetCardStatus(cardId);
                var cardDate = await GetCardDate(cardId);
                cardDetails.Add("CardMembers", cardMembers);
                cardDetails.Add("CardAttachments", cardAttachments);
                cardDetails.Add("CardDescription", cardDescription);
                cardDetails.Add("CardComments", cardComments);
                cardDetails.Add("CardStatus", cardStatus);
                cardDetails.Add("CardDate", cardDate);

                return cardDetails;

            }
            catch (Exception e)
            {
                throw;
            }
        }
        
        public async Task<string> GetCardNameById(int id)
        {
            try
            {
                var card = await cardRepository.Get(id);
                if (card != null)
                    return card.CardName;
                return null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task AddComment(AddCommentRequest addCommentRequest, int? currentUserId)
        {
            try
            {
                var card = await cardRepository.GetAll().Include(s => s.Challenge).FirstOrDefaultAsync(s => s.CardId == addCommentRequest.CardId);
                addCommentRequest.CommentText = addCommentRequest.CommentText.Trim();
                var comment = mapper.Map<Comments>(addCommentRequest);
                comment.CreatedBy = currentUserId;
                await commentsRepository.InsertAsync(comment);

                foreach (var item in addCommentRequest.ToUsers)
                {
                    List<NotificationField> fields = new List<NotificationField>();
                    var field = new NotificationField
                    {
                        CardId = addCommentRequest.CardId,
                        ProjectId = card.Challenge.ProjectId,
                        ChallengeId = card.ChallengeId,
                    };
                    fields.Add(field);
                    AddNotificationRequest addNotificationRequest = new AddNotificationRequest
                    {
                        Fields = fields,
                        FromUserId = currentUserId,
                        ToUserId = userService.GetUserByPublicId(item).UserId,
                        NotificationTypeId = notificationService.GetNotificationTypeId(NotificationTypes.Comment.ToString()).NotificationTypeId
                    };
                    var notificationId = await notificationService.GenerateNotification(addNotificationRequest);
                    var data = await notificationService.GetNotificationText(notificationId);
                    await notificationService.SendAsync(NotificationListeners.commentMention.ToString(), item, data);
                    var notificationData = await notificationService.GetNotifications(currentUserId, 10, 0);
                    await notificationService.SendAsync(NotificationListeners.updateNotificationPanel.ToString(), item, notificationData);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public int AssignMember(string email, int cardId, int? currentUserId)
        {
            try
            {
                var user = userService.GetUserByEmail(email);
                var currentUser = userService.GetUserById(currentUserId.Value);
                var card = cardRepository.GetAll().Include(s => s.Challenge).FirstOrDefault(s => s.CardId == cardId);
                var cardmember = new CardAssignedMember()
                {
                    CardId = cardId,
                    MemberUserId = user.UserId,
                    CreatedBy = currentUserId,
                };
                var id = cardAssignedMembersRepository.InsertAndGetId(cardmember);

                var notificationFieldTypes = notificationFieldTypesRepository.GetAllListAsync().Result;
                NotificationField notificationField = new NotificationField
                {
                    NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{CardName}").NotificationFieldTypeId,
                    Name = notificationFieldTypes.First(s => s.Text == "{CardName}").Type,
                    Value = card.CardName
                };
                NotificationField notificationField1 = new NotificationField
                {
                    CardId = card.CardId,
                    ProjectId = card.Challenge.ProjectId,
                };
                var fields = new List<NotificationField>();
                fields.Add(notificationField);
                fields.Add(notificationField1);
                var notification = new AddNotificationRequest
                {
                    Fields = fields,
                    FromUserId = currentUserId,
                    NotificationTypeId = notificationService.GetNotificationTypeId(NotificationTypes.AssignedCard.ToString()).NotificationTypeId,
                    ToUserId = user.UserId
                };
                var notificationId = notificationService.GenerateNotification(notification).Result;
                var data = notificationService.GetNotificationText(notificationId).Result;
                notificationService.SendAsync(NotificationListeners.assignedCard.ToString(), user.UserPublicId.ToString(), data);
                var notificationData = notificationService.GetNotifications(currentUserId, 10, 0).Result;
                notificationService.SendAsync(NotificationListeners.updateNotificationPanel.ToString(), user.UserPublicId.ToString(), notificationData);

                AddActivityLogRequest addActivityLogRequest = new AddActivityLogRequest()
                {
                    ActivityDateTime = DateTime.Now.ToString(),
                    ActivityText = "<b>"+currentUser.FullName + "</b> added <b>" + user.FullName + "</b> to the card <b>" + card.CardName+"</b>",
                    CardId = card.CardId,
                };
                commonService.AddActivityLog(addActivityLogRequest, currentUserId);
                return id;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void UnAssignMember(string email, int cardId, int? currentUserId)
        {
            try
            {
                var user = userService.GetUserByEmail(email);
                var currentUser = userService.GetUserById(currentUserId.Value);
                var card = cardRepository.GetAll().Include(s => s.Challenge).FirstOrDefault(s => s.CardId == cardId);
                var cardMember = cardAssignedMembersRepository.GetAll().FirstOrDefault(s => s.MemberUserId == user.UserId && s.CardId == s.CardId);
                if (cardMember == null)
                    throw new Exception();
                cardMember.UpdatedBy = currentUserId;
                cardAssignedMembersRepository.Delete(cardMember);

                AddActivityLogRequest addActivityLogRequest = new AddActivityLogRequest()
                {
                    ActivityDateTime = DateTime.Now.ToString(),
                    ActivityText = "<b>" + currentUser.FullName + "</b> removed <b>" + user.FullName + "</b> from the card <b>" + card.CardName + "</b>",
                    CardId = card.CardId,
                };
                commonService.AddActivityLog(addActivityLogRequest, currentUserId);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<bool> CreateCheckList(AddCheckListRequest addCheckListRequest, int? currentUserId)
        {
            try
            {
                var currentUser = userService.GetUserById(currentUserId.Value);
                var checkList = mapper.Map<CheckList>(addCheckListRequest);
                checkList.CreatedBy = currentUserId;
                var id = checkListRepository.InsertAndGetId(checkList);

                if (addCheckListRequest.ParentCheckListId != null)
                {
                    var parent = await checkListRepository.Get(addCheckListRequest.ParentCheckListId.Value);
                    if (parent.IsCompleted == true)
                    {
                        parent.UpdatedBy = currentUserId;
                        parent.IsCompleted = false;
                        await checkListRepository.Update(parent);
                    }
                }
                AddActivityLogRequest addActivityLogRequest = new AddActivityLogRequest()
                {
                    ActivityDateTime = DateTime.Now.ToString(),
                    ActivityText = "<b>"+currentUser.FullName + "</b> created the checklist <b>" + checkList.CheckListName+"</b>",
                    CardId = checkList.CardId,
                };

                await commonService.AddActivityLog(addActivityLogRequest, currentUserId);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddAttachment(int cardId, IFormFile file, int? currentUserId)
        {
            try
            {
                var projectId = await cardRepository.GetAll().Include(s => s.Challenge).ThenInclude(s => s.Project).Where(s => s.CardId == cardId).Select(s => s.Challenge.Project.ProjectPublicId).FirstOrDefaultAsync();
                var name = file.FileName;

                var thumbnail = config.GetValue<string>("ThumbnailUrl");
                var fileType = Constants.GetFileTypeByExtension(file.FileName.Split('.')[file.FileName.Split('.').Length-1]);
                if (fileType == FileTypes.Pdf.ToString())
                    thumbnail += "pdf.png";
                //else if (fileType == FileTypes.Image.ToString())
                //    thumbnail += "myimg.png";
                else if (fileType == FileTypes.Word.ToString())
                    thumbnail += "word.png";
                else if (fileType == FileTypes.Excel.ToString())
                    thumbnail += "excel.png";
                else
                    thumbnail += "default.png";

                var path = config.GetValue<string>("AttachmentUrl") + projectId.ToString() + "\\Card_" + cardId.ToString();
                //var stream = new FileStream(config.GetValue<string>("ServerUrl") + config.GetValue<string>("AttachmentUrl") + name, FileMode.Create);
                //var filePath = config.GetValue<string>("ServerUrl") + config.GetValue<string>("AttachmentUrl");
                var filePath = Constants.ContentRootPath + path;
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                using (FileStream fs = new FileStream(Path.Combine(filePath,name), FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                var attachment = new CardAttachment()
                {
                    CardId = cardId,
                    CreatedBy = currentUserId,
                    FileExtension = file.FileName.Split('.')[file.FileName.Split('.').Length - 1],
                    Name = name,
                    SizeInKB = file.Length / 1024,
                    FileUrl = Path.Combine(path,name),
                    ThumbnailUrl = fileType == FileTypes.Image.ToString() ? Path.Combine(path, name) : thumbnail
                };
                await attachmentRepository.InsertAsync(attachment);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ChangeStatus(int cardId, int cardStatus, int? currentUserId)
        {
            try
            {
                var currentUser = userService.GetUserById(currentUserId.Value);
                var card = cardRepository.GetAll().Include(s => s.Challenge).ThenInclude(s => s.Project).FirstOrDefault(s => s.CardId == cardId);
                card.CardStatus = cardStatus;
                card.UpdatedBy = currentUserId;
                await cardRepository.Update(card);

                var members = cardAssignedMembersRepository.GetAll().Include(s => s.MemberUser).Where(s => s.CardId == cardId).ToList();
                var notificationFieldTypes = await notificationFieldTypesRepository.GetAllListAsync();

                foreach (var member in members)
                {

                    NotificationField notificationField = new NotificationField
                    {
                        NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{CardName}").NotificationFieldTypeId,
                        Name = notificationFieldTypes.First(s => s.Text == "{CardName}").Type,
                        Value = card.CardName
                    };
                    NotificationField notificationField1 = new NotificationField
                    {
                        NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{ProjectName}").NotificationFieldTypeId,
                        Name = notificationFieldTypes.First(s => s.Text == "{ProjectName}").Type,
                        Value = card.Challenge.Project.ProjectName
                    };
                    NotificationField notificationField2 = new NotificationField
                    {
                        NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{CardStatus}").NotificationFieldTypeId,
                        Name = notificationFieldTypes.First(s => s.Text == "{CardStatus}").Type,
                        Value = ((CardStatus)cardStatus).ToString()
                    };
                    NotificationField notificationField3 = new NotificationField
                    {
                        ProjectId = card.Challenge.ProjectId,
                        CardId = card.CardId
                    };
                    var fields = new List<NotificationField>();
                    fields.Add(notificationField);
                    fields.Add(notificationField1);
                    fields.Add(notificationField2);
                    fields.Add(notificationField3);
                    var notification = new AddNotificationRequest
                    {
                        Fields = fields,
                        FromUserId = currentUserId,
                        NotificationTypeId = notificationService.GetNotificationTypeId(NotificationTypes.CardStatusChange.ToString()).NotificationTypeId,
                        ToUserId = member.MemberUserId
                    };
                    var notificationId = await notificationService.GenerateNotification(notification);
                    var data = await notificationService.GetNotificationText(notificationId);
                    await notificationService.SendAsync(NotificationListeners.cardStatusChange.ToString(), member.MemberUser.UserPublicId.ToString(), data);
                    var notificationData = await notificationService.GetNotifications(member.MemberUserId, 10, 0);
                    await notificationService.SendAsync(NotificationListeners.updateNotificationPanel.ToString(), member.MemberUser.UserPublicId.ToString(), notificationData);
                }

                AddActivityLogRequest addActivityLogRequest = new AddActivityLogRequest()
                {
                    ActivityDateTime = DateTime.Now.ToString(),
                    ActivityText = "<b>" + currentUser.FullName + "</b> updated <b>" + card.CardName + "</b>'s status to " + ((CardStatus)card.CardStatus).ToString(),
                    CardId = card.CardId,
                };
                await commonService.AddActivityLog(addActivityLogRequest, currentUserId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SetDueDate(int cardId, string date, int? currentUserId)
        {
            try
            {
                var currentUser = userService.GetUserById(currentUserId.Value);
                var card = await cardRepository.Get(cardId);
                card.DueDate = DateTime.Parse(date);
                card.UpdatedBy = currentUserId;
                await cardRepository.Update(card);
                var activity = new AddActivityLogRequest()
                {
                    ActivityDateTime = DateTime.Now.ToString(),
                    ActivityText = "<b>"+currentUser.FullName + "</b> set the due date for card <b>" + card.CardName + "</b> to " + card.DueDate.Value,
                    CardId = card.CardId,
                };
                await commonService.AddActivityLog(activity, currentUserId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<CheckListDto>> GetCheckList(int? cardId)
        {
            try
            {
                var result = new List<CheckListDto>();
                var lists = checkListRepository.GetAll().Where(s => s.CardId == cardId).AsEnumerable().GroupBy(s => s.GroupId);
                foreach (var group in lists)
                {
                    var item = new CheckListDto();
                    item.GroupId = group.Key.Value;
                    var checkListGroupDto = new CheckListGroupDto();
                    int completedItems = 0;
                    var a = group.ToList().Where(s => s.ParentCheckListId != null).ToList();
                    //checkListGroupDto.Progress = (int)((double)group.ToList().Where(s => s.IsCompleted == true).Count() / group.ToList().Count() * 100);
                    checkListGroupDto.CheckLists = mapper.Map<List<CheckListItemDto>>(group.ToList().Where(s => s.ParentCheckListId == null).ToList());
                    foreach (var list in checkListGroupDto.CheckLists)
                    {
                        var subitems = group.ToList().Where(s => s.ParentCheckListId == list.CheckListId).ToList();
                        if (subitems.Count > 0)
                            list.SubList = mapper.Map<List<CheckListItemDto>>(subitems);
                        else
                            a.Add(group.Where(s => s.CheckListId == list.CheckListId).First());
                    }
                    checkListGroupDto.Progress = (int)(((double)a.Where(s => s.IsCompleted == true).Count() / a.Count()) * 100);
                    item.checkListGroup = checkListGroupDto;
                    result.Add(item);
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> UpdateCheckItemStatus(int itemid, bool status, int? currentUserid)
        {
            try
            {
                var currentuser = userService.GetUserById(currentUserid.Value);
                var subItems = await checkListRepository.GetAll().Where(s => s.ParentCheckListId == itemid).ToListAsync();
                var item = await checkListRepository.Get(itemid);
                item.IsCompleted = status;
                item.UpdatedBy = currentUserid;
                await checkListRepository.Update(item);

                //EITHER ITEM HAS A PARENT OR THE ITEM IS PARENT OR NONE OF THEM
                if (subItems.Count > 0)
                {
                    foreach (var subItem in subItems)
                    {
                        subItem.IsCompleted = status;
                        subItem.UpdatedBy = currentUserid;
                        await checkListRepository.Update(item);
                    }
                }

                else if (item.ParentCheckListId != null)
                {
                    var x = checkListRepository.GetAll().Where(s => (s.ParentCheckListId == item.ParentCheckListId) && (s.CheckListId != item.CheckListId)).Select(s => s.IsCompleted).ToList();
                    if (x.Contains(false) || (item.IsCompleted == false))
                    {
                        var parent = await checkListRepository.Get(item.ParentCheckListId.Value);
                        parent.IsCompleted = false;
                        parent.UpdatedBy = currentUserid;
                        await checkListRepository.Update(parent);
                    }
                    else
                    {
                        var parent = await checkListRepository.Get(item.ParentCheckListId.Value);
                        parent.IsCompleted = true;
                        parent.UpdatedBy = currentUserid;
                        await checkListRepository.Update(parent);
                    }
                }

                var activity = new AddActivityLogRequest()
                {
                    ActivityDateTime = DateTime.Now.ToString(),
                    ActivityText = status ? "<b>"+currentuser.FullName + "</b> marked the item <b>" + item.CheckListName+"</b>" : "<b>"+currentuser.FullName + "</b> unmarked the item <b>" + item.CheckListName+"</b>",
                    CardId = item.CardId,
                };
                await commonService.AddActivityLog(activity, currentUserid);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> UpdateCheckItemName(int itemid, string name, int? currentUserid)
        {
            try
            {
                var currentuser = userService.GetUserById(currentUserid.Value);
                var item = await checkListRepository.Get(itemid);
                string text = "<b>" + currentuser.FullName + "</b> changed the item from <b>" + item.CheckListName + "</b> to <b>" + name + "</b>";
                item.CheckListName = name;
                item.UpdatedBy = currentUserid;
                await checkListRepository.Update(item);
                var activity = new AddActivityLogRequest()
                {
                    ActivityDateTime = DateTime.Now.ToString(),
                    ActivityText = text,
                    CardId = item.CardId,
                };
                await commonService.AddActivityLog(activity, currentUserid);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetCardActivities(int? cardId, int currentCount)
        {
            try
            {
                var activities = await activityLogRepository.GetAll()
                    .Include(s => s.Card)
                    .Include(s => s.FromList)
                    .Include(s => s.ToList)
                    .Where(s => s.CardId == cardId)
                    .OrderByDescending(s => s.ActivityDateTime)
                    .Select(s => new CardActivityDto
                    {
                        ActivityId = s.ActivityLogId,
                        ActivityDateTime = s.ActivityDateTime,
                        ActivityText = s.ActivityText,
                        CardName = s.Card != null ? s.Card.CardName : null,
                        FromListName = s.FromList != null ? s.FromList.ChallengeListName : null,
                        ToListName = s.ToList != null ? s.ToList.ChallengeListName : null,
                    })
                    .ToListAsync();

                int totalCount = activities.Count();
                activities = activities.Take(currentCount + 10).ToList();
                currentCount = activities.Count();
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("activities", activities);
                dict.Add("currentCount", currentCount);
                dict.Add("totalCount", totalCount);
                return dict;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteCheckGroup(int groupId, int? currentUserId)
        {
            try
            {
                var items = checkListRepository.GetAll().Where(s => s.GroupId == groupId).ToList();
                foreach (var item in items)
                {
                    item.UpdatedBy = currentUserId;
                    await checkListRepository.Delete(item);
                }
                return items[0].CardId.Value;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> DeleteCheckItem(int itemId, int? currentUserId)
        {
            try
            {
                var items = checkListRepository.GetAll().Where(s => s.CheckListId == itemId || s.ParentCheckListId == itemId).ToList();
                foreach (var item in items)
                {
                    item.UpdatedBy = currentUserId;
                    await checkListRepository.Delete(item);
                }
                return items[0].CardId.Value;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> DeleteCheckSubItem(int subItemid, int? currentUserId)
        {
            try
            {
                var item = checkListRepository.GetAll().FirstOrDefault(s => s.CheckListId == subItemid);
                item.UpdatedBy = currentUserId;
                await checkListRepository.Delete(item);

                if (item.ParentCheckListId != null && item.IsCompleted == false)
                {
                    var siblings = checkListRepository.GetAll().Where(s => s.ParentCheckListId == item.ParentCheckListId).ToList();
                    if (siblings.Any(x => x.IsCompleted != true) == false)
                    {
                        var parent = await checkListRepository.Get(item.ParentCheckListId.Value);
                        parent.IsCompleted = true;
                        parent.UpdatedBy = currentUserId;
                        await checkListRepository.Update(parent);
                    }
                }
                return item.CardId.Value;
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task<bool> DeleteCard(int cardId, int? currentUserId)
        {
            try
            {
                var item = cardRepository.Get(cardId).Result;
                item.UpdatedBy = currentUserId;
                await cardRepository.Delete(item);

                var checklist = checkListRepository.GetAll().Where(s => s.CardId == item.CardId).ToList();
                foreach (var list in checklist)
                {
                    list.UpdatedBy = currentUserId;
                    await checkListRepository.Delete(list);
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public void UpdateCardName(int cardId, string cardName, int? currentUserId)
        {
            try
            {
                var card = cardRepository.Get(cardId).Result;
                card.CardName = cardName;
                card.UpdatedBy = currentUserId;
                cardRepository.Update(card);
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public void MoveCardInList(int toListId, int cardId, int? currentUserId)
        {
            try
            {
                var card = cardRepository.Get(cardId).Result;
                card.ChallengeListId = toListId;
                card.UpdatedBy = currentUserId;
                cardRepository.Update(card);
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task<List<CardAttachmentDTO>> GetCardAttachments(int? cardId)
        {
            try
            {
                var cardAttachmentDetails = new List<CardAttachmentDTO>();
                var cardAttachments = attachmentRepository.GetAll()
                    .Where(w => w.CardId == cardId).ToList();
                cardAttachmentDetails = mapper.Map<List<CardAttachmentDTO>>(cardAttachments);
                foreach (var item in cardAttachmentDetails)
                {
                    item.FileUrl = config.GetValue<string>("ServerUrl") + item.FileUrl;
                    item.ThumbnailUrl = config.GetValue<string>("ServerUrl") + item.ThumbnailUrl;
                }

                return cardAttachmentDetails;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        
        public async Task<List<CardCommentsDTO>> GetcardComments(int? cardId, int? currentUserId)
        {
            try
            {
                var cardComments = commentsRepository.GetAll()
                    .Where(w => w.CardId == cardId)
                    .OrderByDescending(s => s.CommentDateTime)
                    .Select(s => new CardCommentsDTO
                    {
                        CommentId = s.CommentsId,
                        CommentsText = s.CommentText,
                        CommentsDateTime = s.CommentDateTime,
                        CommentBy = s.CreatedBy,
                        CommentByFullName = userRepository.GetAll().FirstOrDefault(w => w.UserId == s.CreatedBy).FullName,
                        IsOwner = s.CreatedBy == currentUserId ? true : false
                    }).ToList();

                return cardComments;
            }
            catch (Exception e)
            {

                throw;
            }
        }
        
        public async Task DeleteComment(int? commentId, int? currentUserId)
        {
            try
            {
                var currentUser = userService.GetUserById(currentUserId.Value);
                var cardComment = await commentsRepository.GetAll()
                    .FirstOrDefaultAsync(w => w.CommentsId == commentId);
                cardComment.UpdatedBy = currentUserId;
                await commentsRepository.Delete(cardComment);

                AddActivityLogRequest addActivityLogRequest = new AddActivityLogRequest()
                {
                    ActivityDateTime = DateTime.Now.ToString(),
                    ActivityText = "<b>" + currentUser.FullName + "</b> deleted a comment",
                    CardId = cardComment.CardId,
                };
                await commonService.AddActivityLog(addActivityLogRequest, currentUserId);
            }
            catch (Exception e)
            {

                throw;
            }
        }
        
        public async Task DeleteAttachment(int id, int? currentUserId)
        {
            try
            {
                var attachment = attachmentRepository.GetAll().FirstOrDefault(s => s.CardAttachmentId == id);
                attachment.UpdatedBy = currentUserId;
                await attachmentRepository.Delete(attachment);
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task CardDueTodayReminder()
        {
            try
            {
                var cards = cardRepository.GetAll()
                    .Include(s => s.Challenge)
                    .ThenInclude(s => s.Project)
                    .Where(s => s.DueDate != null && s.DueDate.Value.Date == DateTime.Today.Date).ToList();
                var notificationFieldTypes = notificationFieldTypesRepository.GetAllListAsync().Result;
                foreach (var card in cards)
                {
                    var cardUsers = cardAssignedMembersRepository.GetAll()
                        .Include(s => s.MemberUser)
                        .Where(s => s.CardId == card.CardId).Select(s => s.MemberUser)
                        .ToList();
                    foreach (var user in cardUsers)
                    {
                        NotificationField notificationField = new NotificationField
                        {
                            NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{CardName}").NotificationFieldTypeId,
                            Name = notificationFieldTypes.First(s => s.Text == "{CardName}").Type,
                            Value = card.CardName
                        };
                        NotificationField notificationField1 = new NotificationField
                        {
                            NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{ProjectName}").NotificationFieldTypeId,
                            Name = notificationFieldTypes.First(s => s.Text == "{ProjectName}").Type,
                            Value = card.Challenge.Project.ProjectName
                        };
                        NotificationField notificationField2 = new NotificationField
                        {
                            NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{CardDueTime}").NotificationFieldTypeId,
                            Name = notificationFieldTypes.First(s => s.Text == "{CardDueTime}").Type,
                            Value = card.DueDate.Value.ToShortTimeString()
                        };
                        NotificationField notificationField3 = new NotificationField
                        {
                            CardId = card.CardId,
                            ProjectId = card.Challenge.ProjectId,
                        };
                        var fields = new List<NotificationField>();
                        fields.Add(notificationField);
                        fields.Add(notificationField1);
                        fields.Add(notificationField2);
                        fields.Add(notificationField3);
                        var notification = new AddNotificationRequest
                        {
                            Fields = fields,
                            NotificationTypeId = notificationService.GetNotificationTypeId(NotificationTypes.CardDueToday.ToString()).NotificationTypeId,
                            ToUserId = user.UserId
                        };
                        var notificationId = notificationService.GenerateNotification(notification).Result;
                        var data = notificationService.GetNotificationText(notificationId).Result;
                        await notificationService.SendAsync(NotificationListeners.cardDueToday.ToString(), user.UserPublicId.ToString(), data);
                        var notificationData = notificationService.GetNotifications(user.UserId, 10, 0).Result;
                        await notificationService.SendAsync(NotificationListeners.updateNotificationPanel.ToString(), user.UserPublicId.ToString(), notificationData);
                        var redirectUrl = notificationService.GetNotificationUrl(notificationId);
                        await CustomEmail<CardService>.SendReminder(
                            user.FullName, user.Email, (int)Helper.EmailTemplates.CardDueToday, "today", card.CardName,
                            card.Challenge.Project.ProjectName, card.DueDate.Value.ToShortTimeString(), redirectUrl, logger, config, environment);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task CardDueTomorrowReminder()
        {
            try
            {
                var cards = cardRepository.GetAll()
                    .Include(s => s.Challenge)
                    .ThenInclude(s => s.Project)
                    .Where(s => s.DueDate != null && s.DueDate.Value.Date == DateTime.Today.AddDays(1).Date).ToList();
                var notificationFieldTypes = notificationFieldTypesRepository.GetAllListAsync().Result;
                foreach (var card in cards)
                {
                    var cardUsers = cardAssignedMembersRepository.GetAll()
                        .Include(s => s.MemberUser)
                        .Where(s => s.CardId == card.CardId).Select(s => s.MemberUser)
                        .ToList();
                    foreach (var user in cardUsers)
                    {
                        NotificationField notificationField = new NotificationField
                        {
                            NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{CardName}").NotificationFieldTypeId,
                            Name = notificationFieldTypes.First(s => s.Text == "{CardName}").Type,
                            Value = card.CardName
                        };
                        NotificationField notificationField1 = new NotificationField
                        {
                            NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{ProjectName}").NotificationFieldTypeId,
                            Name = notificationFieldTypes.First(s => s.Text == "{ProjectName}").Type,
                            Value = card.Challenge.Project.ProjectName
                        };
                        NotificationField notificationField2 = new NotificationField
                        {
                            NotificationFieldTypeId = notificationFieldTypes.First(s => s.Text == "{CardDueTime}").NotificationFieldTypeId,
                            Name = notificationFieldTypes.First(s => s.Text == "{CardDueTime}").Type,
                            Value = card.DueDate.Value.ToShortTimeString()
                        };
                        NotificationField notificationField3 = new NotificationField
                        {
                            CardId = card.CardId,
                            ProjectId = card.Challenge.ProjectId,
                        };
                        var fields = new List<NotificationField>();
                        fields.Add(notificationField);
                        fields.Add(notificationField1);
                        fields.Add(notificationField2);
                        fields.Add(notificationField3);
                        var notification = new AddNotificationRequest
                        {
                            Fields = fields,
                            NotificationTypeId = notificationService.GetNotificationTypeId(NotificationTypes.CardDueTomorrow.ToString()).NotificationTypeId,
                            ToUserId = user.UserId
                        };
                        var notificationId = notificationService.GenerateNotification(notification).Result;
                        var data = notificationService.GetNotificationText(notificationId).Result;
                        await notificationService.SendAsync(NotificationListeners.cardDueTomorrow.ToString(), user.UserPublicId.ToString(), data);
                        var notificationData = notificationService.GetNotifications(user.UserId, 10, 0).Result;
                        await notificationService.SendAsync(NotificationListeners.updateNotificationPanel.ToString(), user.UserPublicId.ToString(), notificationData);
                        var redirectUrl = notificationService.GetNotificationUrl(notificationId);
                        await CustomEmail<CardService>.SendReminder(
                            user.FullName, user.Email, (int)Helper.EmailTemplates.CardDueTomorrow, "tomorrow", card.CardName,
                            card.Challenge.Project.ProjectName, card.DueDate.Value.ToShortTimeString(), redirectUrl, logger, config, environment);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Private

        private async Task<string> GetCardDescription(int? cardId)
        {
            try
            {
                var cardDescription = cardRepository.GetAll().FirstOrDefault(s => s.CardId == cardId).Description;
                return cardDescription;
            }
            catch (Exception e)
            {

                throw;
            }
        }
        private async Task<List<ProjectMemberDto>> GetCardMembers(int? cardId)
        {
            try
            {
                var memberList = new List<ProjectMemberDto>();
                var configuration = config.GetValue<string>("ServerUrl");
                memberList = cardAssignedMembersRepository.GetAll()
                    .Include(i => i.MemberUser)
                    .Where(x => x.CardId == cardId)
                    .Select(s => new ProjectMemberDto
                    {
                        TeamName = null,
                        MemberName = s.MemberUser.FullName,
                        MemberEmail = s.MemberUser.Email,
                        //Picture = Path.Combine(config.GetValue<string>("ServerUrl"), s.MemberUser.PictureURL.TrimStart('\\'))
                        Picture = string.IsNullOrEmpty(s.MemberUser.PictureURL) ? "" : configuration +  s.MemberUser.PictureURL
                    }).ToList();

                return memberList;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private async Task<string> GetCardStatus(int? cardId)
        {
            try
            {
                var cardStatus = cardRepository.GetAll().FirstOrDefault(s => s.CardId == cardId).CardStatus;
                if (cardStatus == null)
                    return null;
                return ((CardStatus)cardStatus).ToString();
            }
            catch (Exception e)
            {

                throw;
            }
        }
        private async Task<DateTime?> GetCardDate(int? cardId)
        {
            try
            {
                var cardDate = cardRepository.GetAll().FirstOrDefault(s => s.CardId == cardId).DueDate;
                return cardDate;
            }
            catch (Exception e)
            {

                throw;
            }
        }

        #endregion

        #endregion
    }
}
