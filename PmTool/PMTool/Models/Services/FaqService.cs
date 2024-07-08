using DAL.Models;
using DAL.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMTool.General;
using PMTool.Models.DTOs;
using PMTool.Models.General;
using PMTool.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Services
{
    public class FaqService : IFaqService
    {
        private readonly IRepository<FAQ> faqRepository;
        private readonly IRepository<UserFeedback> feedbackRepository;
        private readonly IConfiguration config;
        private readonly ILogger<FaqService> logger;
        #region Fields

        #endregion

        #region Constructors
        public FaqService(IRepository<FAQ> _faqRepository, IRepository<UserFeedback> _feedbackRepository, IConfiguration _config, ILogger<FaqService> _logger)
        {
            faqRepository = _faqRepository;
            feedbackRepository = _feedbackRepository;
            config = _config;
            logger = _logger;
        }
        #endregion
        #region Methods

        public List<FaqCategoryDto> GetFaq()
        {
            try
            {
                var result = new List<FaqCategoryDto>();
                var items = faqRepository.GetAll().ToList();
                var faqs = items.GroupBy(s => s.Category).ToList();

                foreach (var faq in faqs)
                {
                    result.Add(new FaqCategoryDto()
                    {
                        CategoryName = faq.Key,
                        Faqs = faq.Select(s => new FaqDto() { Question = s.Question, Answer = s.Answer }).ToList()
                    });
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<bool> AddFeedback(AddFeedbackRequest addFeedbackRequest, int? currentUserId)
        {
            try
            {
                bool res = false;
                await Email<FaqService>.SendMail(addFeedbackRequest.Email, Constants.FEEDBACK_SUBJECT, addFeedbackRequest.Message, "", logger, config);
                res = true;
                if (res)
                {
                    var feedback = new UserFeedback
                    {
                        CreatedBy = currentUserId,
                        Name = addFeedbackRequest.Name,
                        Email = addFeedbackRequest.Email,
                        Phone = string.IsNullOrEmpty(addFeedbackRequest.Phone) ? null : addFeedbackRequest.Phone,
                        Message = addFeedbackRequest.Message
                    };
                    await feedbackRepository.InsertAsync(feedback);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }

        }

        #endregion

    }
}
