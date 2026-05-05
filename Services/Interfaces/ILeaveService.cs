using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;

namespace HR_Management_System.Services.Interfaces
{
    public interface ILeaveService
    {
        Task<LeaveRequest> SubmitLeaveRequestAsync(LeaveRequestViewModel model);
        Task<bool> ApproveLeaveRequestAsync(LeaveApprovalViewModel model, int approvedBy);
        Task<bool> RejectLeaveRequestAsync(int leaveRequestId, int rejectedBy, string reason);
        Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync();
        Task<IEnumerable<LeaveRequest>> GetEmployeeLeaveRequestsAsync(int employeeId);
        Task<LeaveBalanceViewModel> GetLeaveBalanceAsync(int employeeId, int year);
        Task<LeaveBalance> InitializeLeaveBalanceAsync(int employeeId, int year);
        Task<decimal> CalculateLeaveEncashmentAsync(int employeeId, int leaveType, decimal days);
        Task<LeaveEncashmentResult> ProcessLeaveEncashmentAsync(LeaveEncashmentViewModel model);
        Task<bool> AccrueHomeLeaveAsync(int employeeId, int workingDays);
        Task<IEnumerable<PublicHoliday>> GetPublicHolidaysAsync(int year);
        Task<PublicHoliday> AddPublicHolidayAsync(PublicHoliday holiday);
        Task<int> GetPendingLeaveCountAsync();
    }
}
