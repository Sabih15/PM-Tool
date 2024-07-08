using Microsoft.AspNetCore.Http;
using PMTool.Models.DTOs;
using PMTool.Models.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMTool.Models.Services
{
    public interface ICardService
    {
        Task<Dictionary<string, object>> GetCardListLog(int? cardId);
        Task<Dictionary<string, object>> GetCardDetails(int? cardId, int? currentUserId);
        Task AddComment(AddCommentRequest addCommentRequest, int? currentUserId);
        int AssignMember(string email, int cardId, int? currentUserId);
        Task<bool> CreateCheckList(AddCheckListRequest addCheckListRequest, int? currentUserId);
        Task ChangeStatus(int cardId, int cardStatus, int? currentUserId);
        Task SetDueDate(int cardId, string date, int? currentUserId);
        Task<List<CheckListDto>> GetCheckList(int? cardId);
        Task<bool> UpdateCheckItemStatus(int itemid, bool status, int? currentUserid);
        Task<List<CardCommentsDTO>> GetcardComments(int? cardId, int? currentUserId);
        Task<Dictionary<string, object>> GetCardActivities(int? cardId, int currentCount);
        Task<bool> UpdateDescription(int cardId, string description, int? currentUserId);
        Task<List<CardAttachmentDTO>> GetCardAttachments(int? cardId);
        Task<bool> AddAttachment(int cardId, IFormFile file, int? currentUserId);
        Task<bool> UpdateCheckItemName(int itemid, string name, int? currentUserid);
        Task<int> DeleteCheckGroup(int groupId, int? currentUserId);
        Task<int> DeleteCheckItem(int itemId, int? currentUserId);
        Task<int> DeleteCheckSubItem(int subItemid, int? currentUserId);
        Task<bool> DeleteCard(int cardId, int? currentUserId);
        void UpdateCardName(int cardId, string cardName, int? currentUserId);
        void MoveCardInList(int toListId, int cardId, int? currentUserId);
        Task DeleteComment(int? commentId, int? currentUserId);
        Task DeleteAttachment(int id, int? currentUserId);
        Task<string> GetCardNameById(int id);
        void UnAssignMember(string email, int cardId, int? currentUserId);
    }
}