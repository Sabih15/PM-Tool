using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Linq;

namespace DAL
{
    public class PMToolDbContext : DbContext
    {
        public PMToolDbContext(DbContextOptions<PMToolDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<CardAttachment>()
                .Property(p => p.SizeInKB)
                .HasColumnType("decimal(19,2)");

            builder.Entity<User>()
                .Property(s => s.UserPublicId)
                .HasDefaultValueSql("newid()");

            builder.Entity<Team>()
                .Property(s => s.TeamPublicId)
                .HasDefaultValueSql("newid()");

            builder.Entity<Project>()
                .Property(s => s.ProjectPublicId)
                .HasDefaultValueSql("newid()");

            builder.Entity<TemporaryProjectMember>()
                .Property(s => s.TemporaryProjectMemberPublicId)
                .HasDefaultValueSql("newid()");

            builder.Entity<ProjectMember>()
                .Property(s => s.ProjectMemberPublicId)
                .HasDefaultValueSql("newid()");

            builder.Entity<TeamMember>()
                .Property(s => s.TeamMemberPublicId)
                .HasDefaultValueSql("newid()");

            builder.Entity<TemporaryTeamMember>()
                .Property(s => s.TemporaryTeamMemberPublicId)
                .HasDefaultValueSql("newid()");

        }

        public DbSet<Card> Card { get; set; }

        public DbSet<Challenge> Challenge { get; set; }
        
        public DbSet<CardAssignedMember> cardAssignedMember { get; set; }
        
        public DbSet<CardAttachment> cardAttachment { get; set; }
        
        public DbSet<ChallengeList> ChallengeList { get; set; }
        
        public DbSet<CheckList> CheckList { get; set; }

        public DbSet<Project> Project { get; set; }

        public DbSet<ProjectTeam> ProjectTeam { get; set; }
        
        public DbSet<ProjectMember> ProjectMember { get; set; }
        
        public DbSet<TemporaryProjectMember> TemporaryProjectMember { get; set; }
        
        public DbSet<Permission> Permission { get; set; }
        public DbSet<ActivityLog> ActivityLog { get; set; }
        public DbSet<Comments> Comments { get; set; }
        
        public DbSet<ProjectPermission> ProjectPermission { get; set; }
        
        public DbSet<ProjectMemberPermission> ProjectMemberPermission { get; set; }
        
        public DbSet<RefreshToken> RefreshToken { get; set; }
        
        public DbSet<Role> Role { get; set; }
        
        public DbSet<RolePermission> RolePermission { get; set; }
        
        public DbSet<User> User { get; set; }
        
        public DbSet<Team> Team { get; set; }
        
        public DbSet<TeamMember> TeamMember { get; set; }
        
        public DbSet<TemporaryTeamMember> TemporaryTeamMember { get; set; }
        public DbSet<NotificationType> NotificationType { get; set; }
        public DbSet<NotificationFieldType> NotificationFieldType { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<NotificationField> NotificationField { get; set; }
        public DbSet<FAQ> Faq { get; set; }
        public DbSet<UserFeedback> UserFeedback { get; set; }
        public DbSet<UserChallengeDuration> UserChallengeDuration { get; set; }

    }
}
