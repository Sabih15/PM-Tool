using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PMTool.Authorization;
using PMTool.Hubs;
using PMTool.Models.General;
using PMTool.Models.Request;
using PMTool.Models.Services;
using PMTool.Resources.Response;
using PMTool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PMTool.Controllers
{
    [Authorize(Policy = "ActiveUserPolicy")]
    public class CardController : BaseController
    {
        #region Fields

        private readonly ICardService cardService;
        private readonly IUserService userService;
        private readonly ILogger<CardController> logger;

        #endregion

        #region Constructors

        public CardController(ICardService _cardService, IUserService _userService, ILogger<CardController> _logger)
        {
            cardService = _cardService;
            userService = _userService;
            logger = _logger;
        }

        #endregion

        #region Methods

        #region Private

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

        #endregion

        #region Public

        [HttpGet("GetCardListLog")]
        public async Task<GeneralResponse> GetCardListLog(int cardId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await cardService.GetCardListLog(cardId);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);
                response.Data = result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetCardDetails")]
        public async Task<GeneralResponse> GetCardDetails(int? cardId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var cardDetails = await cardService.GetCardDetails(cardId, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);
                response.Data = cardDetails;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + "\nTarget: " + e.TargetSite.Name + "\nStackTrace: " + e.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("AddComment")]
        public async Task<GeneralResponse> AddComment(AddCommentRequest addCommentRequest)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await cardService.AddComment(addCommentRequest, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.CreateSuccess);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + "\nTarget: " + e.TargetSite.Name + "\nStackTrace: " + e.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("AssignMember")]
        public async Task<GeneralResponse> AssignMember(string email, int cardId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var cardMemberId = cardService.AssignMember(email, cardId, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.CreateSuccess);
                response.Data = cardMemberId;
                response.Message = Constants.CARD_MEMBER_ASSIGN;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + "\nTarget: " + e.TargetSite.Name + "\nStackTrace: " + e.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpDelete("UnassignMember")]
        public async Task<GeneralResponse> UnassignMember(string email, int cardId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                cardService.UnAssignMember(email, cardId, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DeleteSuccess);
                response.Message = Constants.CARD_MEMBER_UNASSIGN;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + "\nTarget: " + e.TargetSite.Name + "\nStackTrace: " + e.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("AddCheckList")]
        public async Task<GeneralResponse> AddCheckList(AddCheckListRequest addCheckListRequest)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await cardService.CreateCheckList(addCheckListRequest, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.CreateSuccess);
                if (result)
                {
                    var data = await cardService.GetCheckList(addCheckListRequest.CardId);
                    response.Data = data;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + "\nTarget: " + e.TargetSite.Name + "\nStackTrace: " + e.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("ChangeCardStatus")]
        public async Task<GeneralResponse> ChangeCardStatus(int cardId, int cardStatus)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await cardService.ChangeStatus(cardId, cardStatus, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.CreateSuccess);
                response.Message = Constants.CARD_STATUS_CHANGE;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + "\nTarget: " + e.TargetSite.Name + "\nStackTrace: " + e.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("SetDueDate")]
        public async Task<GeneralResponse> SetDueDate(int cardId, string date)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await cardService.SetDueDate(cardId, date, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.CreateSuccess);
                response.Message = Constants.CARD_DUE_DATE_CHANGE;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + "\nTarget: " + e.TargetSite.Name + "\nStackTrace: " + e.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPut("UpdateCheckItemStatus")]
        public async Task<GeneralResponse> UpdateCheckItemStatus(int itemid, bool status)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await cardService.UpdateCheckItemStatus(itemid, status, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.CreateSuccess);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPut("UpdateCheckItemName")]
        public async Task<GeneralResponse> UpdateCheckItemName(int itemid, string name)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await cardService.UpdateCheckItemName(itemid, name, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.CreateSuccess);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetComments")]
        public async Task<GeneralResponse> GetComments(int cardid)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await cardService.GetcardComments(cardid, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);
                response.Data = result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetActivities")]
        public async Task<GeneralResponse> GetActivities(int cardId, int currentCount)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await cardService.GetCardActivities(cardId, currentCount);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);
                response.Data = result;

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPost("AddAttachment")]
        public async Task<GeneralResponse> AddAttachment(int cardId, IFormFile file)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await cardService.AddAttachment(cardId, file, GetCurrentUserId());
                if (result)
                {
                    var data = await cardService.GetCardAttachments(cardId);
                    response.Data = data;
                    response.Message = Constants.CARD_ATTACHMENT_ADD;
                }
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.GetSuccess);

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpDelete("DeleteCheckGroup")]
        public async Task<GeneralResponse> DeleteCheckGroup(int groupId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var cardId = await cardService.DeleteCheckGroup(groupId, GetCurrentUserId());
                var data = await cardService.GetCheckList(cardId);
                response.Data = data;
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DeleteSuccess);

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpDelete("DeleteCheckItem")]
        public async Task<GeneralResponse> DeleteCheckItem(int itemId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var cardId = await cardService.DeleteCheckItem(itemId, GetCurrentUserId());
                var data = await cardService.GetCheckList(cardId);
                response.Data = data;
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DeleteSuccess);

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpDelete("DeleteCheckSubItem")]
        public async Task<GeneralResponse> DeleteCheckSubItem(int subItemid)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var cardId = await cardService.DeleteCheckSubItem(subItemid, GetCurrentUserId());
                var data = await cardService.GetCheckList(cardId);
                response.Data = data;
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DeleteSuccess);

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpDelete("DeleteCard")]
        public async Task<GeneralResponse> DeleteCard(int cardId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await cardService.DeleteCard(cardId, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DeleteSuccess);

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPut("UpdateCardName")]
        public GeneralResponse UpdateCardName(int cardId, string cardName)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                cardService.UpdateCardName(cardId, cardName, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.UpdateSuccess);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }
        
        [HttpPut("MoveCardInList")]
        public GeneralResponse MoveCardInList(int toListId, int cardId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                cardService.MoveCardInList(toListId, cardId, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.UpdateSuccess);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpPut("UpdateDescription")]
        public async Task<GeneralResponse> UpdateDescription(UpdateCardDescriptionRequest updateCardDescriptionRequest)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await cardService.UpdateDescription(updateCardDescriptionRequest.CardId, updateCardDescriptionRequest.Description, GetCurrentUserId());
                if (result)
                    GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.UpdateSuccess);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpDelete("DeleteComment")]
        public async Task<GeneralResponse> DeleteComment(int? commentId)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await cardService.DeleteComment(commentId, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DeleteSuccess);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpDelete("DeleteAttachment")]
        public async Task<GeneralResponse> DeleteAttachment(int id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                await cardService.DeleteAttachment(id, GetCurrentUserId());
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DeleteSuccess);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        [HttpGet("GetCardNameById")]
        public async Task<GeneralResponse> GetCardNameById(int id)
        {
            GeneralResponse response = new GeneralResponse();
            try
            {
                var result = await cardService.GetCardNameById(id);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DeleteSuccess);
                response.Data = result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + "\nTarget: " + ex.TargetSite.Name + "\nStackTrace: " + ex.StackTrace);
                GeneralResponse.SetResponse(response, General.Helper.ResponseEnum.DefaultErrorMsg);
            }
            return response;
        }

        #endregion

        #endregion
    }
}


