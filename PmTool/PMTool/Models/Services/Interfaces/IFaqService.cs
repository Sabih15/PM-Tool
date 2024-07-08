using PMTool.Models.DTOs;
using PMTool.Models.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMTool.Models.Services
{
    public interface IFaqService
    {
        Task<bool> AddFeedback(AddFeedbackRequest addFeedbackRequest, int? currentUserId);
        List<FaqCategoryDto> GetFaq();
    }
}