using AutoMapper;
using DAL.Models;
using PMTool.General;
using PMTool.Models.DTOs;
using PMTool.Models.Request;
using PMTool.Resources.Request;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Mappers
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<CreateProjectReq, Project>()
                .ForMember(d => d.ProjectName, _ => _.MapFrom(s => s.Name.Trim()))
                .ForMember(d => d.TotalChallenges, _ => _.MapFrom(s => s.TotalChallenges))
                .ForMember(d => d.Description, _ => _.MapFrom(s => s.Description.Trim()))
                .ForMember(d => d.DueDate, _ => _.MapFrom(s => s.DueDate))
                .ForMember(d => d.CreatedBy, _ => _.Ignore())
                ;

            CreateMap<SignUpReq, User>()
                .ForMember(d => d.Gender, _ => _.Ignore())
                .ForMember(d => d.Password, _ => _.MapFrom(s => (EncryptDecrypt.Encrypt(s.Password)).Trim()))
                .ForMember(d => d.ResetCode, _ => _.MapFrom(s => Helper.GetResetKey(4)))
                .ForMember(d => d.IsVerified, _ => _.MapFrom(s => false))
                .ForMember(d => d.IsSocialUser, _ => _.MapFrom(s => false))
                .ForMember(d => d.IsLocked, _ => _.MapFrom(s => false))
                .ForMember(d => d.IsReset, _ => _.MapFrom(s => false))
                .ForMember(d => d.IsProfileCompleted, _ => _.MapFrom(s => true))
                .ReverseMap()
                ;

            CreateMap<User, UserDto>()
                .ForMember(d => d.Email, _ => _.MapFrom(s => s.Email))
                .ForMember(d => d.FullName, _ => _.MapFrom(s => s.FullName))
                .ForMember(d => d.PublicId, _ => _.MapFrom(s => s.UserPublicId))
                .ForMember(d => d.RoleName, _ => _.MapFrom(s => s.Role.RoleName))
                .ForMember(d => d.Picture, _ => _.MapFrom(s => s.PictureURL))
                ;

            CreateMap<SocialLoginReq, User>()
                .ForMember(d => d.Email, _ => _.MapFrom(s => s.email.Trim()))
                .ForMember(d => d.FullName, _ => _.MapFrom(s => s.name.Trim()))
                .ForMember(d => d.PictureURL, _ => _.MapFrom(s => s.photoUrl.Trim()))
                .ForMember(d => d.IsActive, _ => _.MapFrom(s => true))
                .ForMember(d => d.IsVerified, _ => _.MapFrom(s => true))
                .ForMember(d => d.IsSocialUser, _ => _.MapFrom(s => true))
                .ForMember(d => d.Provider, _ => _.MapFrom(s => s.provider))
                ;

            CreateMap<User, UsersDto>()
                .ForMember(d => d.SocialUser, _ => _.MapFrom(s => s.IsSocialUser ? "Yes" : "No"))
                .ForMember(d => d.RoleName, _ => _.MapFrom(s => s.Role.RoleName))
                ;

            CreateMap<Role, GeneralDto>()
                .ForMember(d => d.Id, _ => _.MapFrom(s => s.RoleId))
                .ForMember(d => d.Name, _ => _.MapFrom(s => s.RoleName))
                ;

            //Rest of the fields are mapped automatically
            CreateMap<Team, UpsertTeamReq>()
                .ForMember(d => d.TeamId, _ => _.MapFrom(s => s.TeamPublicId))
                .ForMember(d => d.Description, _ => _.MapFrom(s => s.Description))
                .ForMember(d => d.TeamName, _ => _.MapFrom(s => s.TeamName.Trim()))
                ;

            CreateMap<UpsertTeamReq, Team>()
                .ForMember(d => d.TeamId, _ => _.Ignore())
                .ForMember(d => d.TeamPublicId, _ => _.MapFrom(s => s.TeamId))

                ;

            CreateMap<Project, ProjectListDto>()
                .ForMember(d => d.Id, _ => _.MapFrom(s => s.ProjectPublicId))
                .ForMember(d => d.Name, _ => _.MapFrom(s => s.ProjectName.Trim()))
                .ForMember(d => d.Description, _ => _.MapFrom(s => s.Description))
                .ForMember(d => d.TotalChallenges, _ => _.MapFrom(s => s.TotalChallenges))
                .ForMember(d => d.StartDate, _ => _.MapFrom(s => s.CreatedDate.ToString()))
                .ForMember(d => d.EndDate, _ => _.MapFrom(s => s.DueDate.ToString()))
                ;

            CreateMap<Project, EditProjectDto>()
                .ForMember(d => d.ProjectId, _ => _.MapFrom(s => s.ProjectPublicId.ToString()))
                .ForMember(d => d.Name, _ => _.MapFrom(s => s.ProjectName))
                .ForMember(d => d.Description, _ => _.MapFrom(s => s.Description))
                .ForMember(d => d.DueDate, _ => _.MapFrom(s => s.DueDate.Value))
                .ForMember(d => d.TotalChallenges, _ => _.MapFrom(s => s.TotalChallenges))
                ;

            CreateMap<CheckList, CheckListItemDto>()
                .ForMember(s => s.CheckListId, _ => _.MapFrom(s => s.CheckListId))
                .ForMember(s => s.CheckListText, _ => _.MapFrom(s => s.CheckListName))
                .ForMember(s => s.Status, _ => _.MapFrom(s => s.IsCompleted.Value))
                ;

            CreateMap<CardAttachment, CardAttachmentDTO>()
                .ForMember(s => s.AttachmentId, _ => _.MapFrom(s => s.CardAttachmentId))
                ;

            CreateMap<AddCommentRequest, Comments>()
                .ForMember(d => d.CardId, _ => _.MapFrom(s => s.CardId))
                .ForMember(d => d.CommentDateTime, _ => _.MapFrom(s => DateTime.ParseExact(s.CommentDateTime, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture)))
                .ForMember(d => d.CommentText, _ => _.MapFrom(s => s.CommentText))
                ;

            CreateMap<AddCheckListRequest, CheckList>()
                .ForMember(d => d.CardId, _ => _.MapFrom(s => s.CardId))
                .ForMember(d => d.CheckListName, _ => _.MapFrom(s => s.Name))
                .ForMember(d => d.GroupId, _ => _.MapFrom(s => s.GroupId))
                .ForMember(d => d.IsCompleted, _ => _.MapFrom(s => false))
                .ForMember(d => d.ParentCheckListId, _ => _.MapFrom(s => s.ParentCheckListId))
                ;

            CreateMap<AddActivityLogRequest, ActivityLog>()
                .ForMember(d => d.ActivityDateTime, _ => _.MapFrom(s => s.ActivityDateTime))
                .ForMember(d => d.ActivityText, _ => _.MapFrom(s => s.ActivityText))
                .ForMember(d => d.CardId, _ => _.MapFrom(s => s.CardId))
                .ForMember(d => d.FromListId, _ => _.MapFrom(s => s.FromListId))
                .ForMember(d => d.ToListId, _ => _.MapFrom(s => s.ToListId))
                ;

            CreateMap<Team, TeamListDto>()
                .ForMember(d => d.TeamId, _ => _.MapFrom(s => s.TeamPublicId.Value))
                .ForMember(d => d.TeamName, _ => _.MapFrom(s => s.TeamName))
                .ForMember(d => d.Description, _ => _.MapFrom(s => s.Description))
                .ForMember(d => d.ShowMore, _ => _.MapFrom(s => false))
                .ForMember(d => d.TotalMembers, _ => _.Ignore())
                ;

            CreateMap<FAQ, FaqDto>()
                .ForMember(d => d.Question, _ => _.MapFrom(s => s.Question))
                .ForMember(d => d.Answer, _ => _.MapFrom(s => s.Answer))
                ;

            CreateMap<ProjectPermission, ProjectPermissionDto>()
                .ForMember(d => d.PermissionId, _ => _.MapFrom(s => s.ProjectPermissionId))
                .ForMember(d => d.PermissionName, _ => _.MapFrom(s => s.ProjectPermissionDisplayName))
                .ForMember(d => d.PermissionDescription, _ => _.MapFrom(s => s.ProjectPermissionDescription))
                .ForMember(d => d.PermissionCode, _ => _.MapFrom(s => s.ProjectPermissionCode))
                ;

            CreateMap<ProjectMemberPermission, ProjectMemberPermissionDto>()
                .ForMember(d => d.MemberEmail, _ => _.MapFrom(s => s.MemberUser.Email))
                .ForMember(d => d.MemberName, _ => _.MapFrom(s => s.MemberUser.FullName))
                .ForMember(d => d.PermissionId, _ => _.MapFrom(s => s.ProjectPermission.ProjectPermissionId))
                .ForMember(d => d.PermissionDisplayName, _ => _.MapFrom(s => s.ProjectPermission.ProjectPermissionDisplayName))
                ;

            CreateMap<Project, ProjectReportDto>()
                .ForMember(d => d.ChallengeCount, _ => _.MapFrom(s => s.TotalChallenges))
                .ForMember(d => d.DueDate, _ => _.MapFrom(s => s.DueDate))
                .ForMember(d => d.StartDate, _ => _.MapFrom(s => s.CreatedDate))
                .ForMember(d => d.Name, _ => _.MapFrom(s => s.ProjectName))
                ;

            CreateMap<Challenge, ChallengeReportDto>()
                .ForMember(d => d.CompleteDate, _ => _.MapFrom(s => s.CompleteDate))
                .ForMember(d => d.StartDate, _ => _.MapFrom(s => s.UnlockDate))
                .ForMember(d => d.Name, _ => _.MapFrom(s => s.ChallengeName))
                .ForMember(d => d.Status, _ => _.MapFrom(s => s.IsCompleted == true ? "Completed" : "Incomplete"))
                ;
        }
    }
}
