using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Management_System.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class PagesController : Controller
    {
        [Authorize(Roles = "Admin,HRManager")]
        [Route("~/Users")]
        [Route("~/Users/Index")]
        public IActionResult Users() => View("~/Views/Users/Index.cshtml");

        [Route("~/Employees")]
        [Route("~/Employees/Index")]
        public IActionResult Employees() => View("~/Views/Employees/Index.cshtml");

        [Route("~/Departments")]
        [Route("~/Departments/Index")]
        public IActionResult Departments() => View("~/Views/Departments/Index.cshtml");

        [Route("~/Attendances")]
        [Route("~/Attendances/Index")]
        public IActionResult Attendances() => View("~/Views/Attendances/Index.cshtml");

        [Route("~/Leaves")]
        [Route("~/Leaves/Index")]
        public IActionResult Leaves() => View("~/Views/Leaves/Index.cshtml");

        [Route("~/Payrolls")]
        [Route("~/Payrolls/Index")]
        public IActionResult Payrolls() => View("~/Views/Payrolls/Index.cshtml");

        [Route("~/Holidays")]
        [Route("~/Holidays/Index")]
        public IActionResult Holidays() => View("~/Views/Holidays/Index.cshtml");

        [Route("~/Notifications")]
        [Route("~/Notifications/Index")]
        public IActionResult Notifications() => View("~/Views/Notifications/Index.cshtml");

        [Authorize(Policy = "AdminOrHR")]
        [Route("~/MasterSheet")]
        [Route("~/MasterSheet/Index")]
        public IActionResult MasterSheet() => View("~/Views/MasterSheet/Index.cshtml");

        [Authorize(Policy = "AdminOrHR")]
        [Route("~/EmployeeHistory")]
        [Route("~/EmployeeHistory/Index")]
        public IActionResult EmployeeHistory() => View("~/Views/EmployeeHistory/Index.cshtml");

        [Authorize(Roles = "Admin")]
        [Route("~/Logs")]
        [Route("~/Logs/Index")]
        public IActionResult Logs() => View("~/Views/Logs/Index.cshtml");
    }
}