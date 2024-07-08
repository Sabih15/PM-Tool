using AutoMapper;
using DAL.Models;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;
using PMTool.Models.DTOs;
using PMTool.Models.Request;
using PMTool.Models.Response;
using PMTool.Resources.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Services
{
    public class CommonService : ICommonService
    {
        #region Fields

        private readonly IRepository<User> userRepository;
        private readonly IRepository<Team> teamRepository;
        private readonly IRepository<TeamMember> teamMemberRepository;
        private readonly IRepository<TemporaryTeamMember> tempTeamRepository;
        private readonly IRepository<FAQ> faqRepository;
        private readonly IRepository<ActivityLog> activitylogRepository;
        private readonly IMapper mapper;

        #endregion

        #region Constructors

        public CommonService(IRepository<User> _userRepository, IRepository<Team> _teamRepository,
            IRepository<TeamMember> _teamMemberRepository, IRepository<TemporaryTeamMember> _tempTeamRepository, IRepository<FAQ> _faqRepository,
            IRepository<ActivityLog> _activitylogRepository, IMapper _mapper)
        {
            userRepository = _userRepository;
            teamRepository = _teamRepository;
            teamMemberRepository = _teamMemberRepository;
            tempTeamRepository = _tempTeamRepository;
            faqRepository = _faqRepository;
            activitylogRepository = _activitylogRepository;
            mapper = _mapper;
        }

        #endregion

        #region Methods

        public List<MemberDDL> GetMemberDDL(string query, int? currentUserId)
        {
            try
            {
                var result = new List<MemberDDL>();
                List<User> users;
                if (string.IsNullOrEmpty(query))
                {
                    users = userRepository.GetAll().Where(s => s.UserId != currentUserId).ToList();
                }
                else
                {
                    users = userRepository.GetAll().Where(s => (s.FullName.Contains(query) || s.Email.Contains(query)) && s.UserId != currentUserId).ToList();
                }
                result.AddRange(users.Select(s => new MemberDDL { Email = s.Email, Name = s.FullName }));
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TeamDDL>> GetTeamDDL(int? userId)
        {
            try
            {
                var result = new List<TeamDDL>();
                var teams = teamRepository.GetAll().Where(s => s.CreatedBy == userId).ToList();

                foreach (var team in teams)
                {
                    var members = await teamMemberRepository.GetAll().Include(s => s.MemberUser).Where(s => s.TeamId == team.TeamId && (s.CreatedBy != s.MemberUserId)).Select(s => s.MemberUser.Email).ToListAsync();
                    var tempMembers = await tempTeamRepository.GetAll().Where(s => s.TeamId == team.TeamId).Select(s => s.Email).ToListAsync();
                    var res = new TeamDDL();
                    res.TeamName = team.TeamName;
                    res.MemberEmails = new List<string>();
                    res.MemberEmails.AddRange(members);
                    res.MemberEmails.AddRange(tempMembers);
                    result.Add(res);
                }
                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task AddActivityLog(AddActivityLogRequest request, int? currentUserId)
        {
            try
            {
                var activity = mapper.Map<ActivityLog>(request);
                if (string.IsNullOrEmpty(request.ActivityDateTime))
                    activity.ActivityDateTime = DateTime.Now;
                activity.CreatedBy = currentUserId;
                await activitylogRepository.InsertAsync(activity);
            }
            catch (Exception)
            {

                throw;
            }
        }

        

        #endregion
    }
}
