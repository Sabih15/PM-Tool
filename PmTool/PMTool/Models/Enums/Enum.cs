using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.Enums
{
    public enum CardStatus
    {
        InProgress = 1,
        Completed = 2,
    }

    public enum NotificationTypes
    {
        Comment,
        AddedInProject,
        AddedInTeam,
        AssignedCard,
        ProjectDueToday,
        ProjectDueTomorrow,
        CardDueToday,
        CardDueTomorrow,
        ChallengeMarkComplete,
        CardStatusChange
    }

    public enum NotificationFieldTypes
    {
        ProjectName,
        TeamName,
        ChallengeName,
        CardName,
        CardStatus,
        ProjectDueTime,
        CardDueTime
    }

    public enum FileTypes
    {
        Image,
        Pdf,
        Word,
        Excel
    }

    public enum NotificationListeners
    {
        commentMention,
        addedInProject,
        addedInTeam,
        assignedCard,
        challengeComplete,
        cardStatusChange,
        cardDueToday,
        cardDueTomorrow,
        projectDueToday,
        projectDueTomorrow,
        updateNotificationPanel
    }
}
