using DAL.Models;
using PMTool.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMTool.Models.Services
{
    public interface IChallengeService
    {
        Task<ProjectDetailHeaderDto> GetProjectDetailHeader(string projectId, int? currentuserId);
        Task<List<ProjectDetailDto>> GetChallengeDetails(string projectId, int? currentUserId);
        Task<ChallengeList> AddListToChallenege(int? challengeId, string listName, int? currentUserId);
        Task<Card> AddCardToList(int? listId, string cardName, int? currentUserId);
        Task<bool> DeleteList(int? listId, int? currentUserId);
        Task<ChallengeListDto> GetUpdatedList(ChallengeList ch);
        Task<CardListDto> GetUpdatedCard(Card card);

        string AddDuration(double mins, int challengeId, int? currentUserid);
        Task<List<ChallengeListDto>> GetUpdatedChallengeLists(int challengeId);
        void UpdateListName(int listId, string listName, int? currentUserId);
        Task<bool> MarkChallengeComplete(int challengeId, int? currentuserId);
        Task<bool> MarkChallengeInComplete(int challengeId, int? currentuserId);
    }
}