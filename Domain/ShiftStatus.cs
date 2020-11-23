using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public enum ShiftStatus
    {
        Open = 1,
        Assigned = 2,
        Approved = 3,
        ForSale = 4,
        Draft = 5,
        OnDuty = 6,
        PendingSwapAcceptance = 7,
        PendingApproval = 8,
        PunchclockStarted = 9,
        PunchclockFinished = 10,
        PunchclockApproved = 11
    }
}
