using System.Diagnostics;
using AcmeWeeklyReportTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcmeWeeklyReportTest.Controllers
{
    public class HomeController : Controller
    {
        // 首頁：登入/未登入都可以看
        public IActionResult Index()
        {
            return View();
        }

        // 測試用：一定要登入才能進
        // 用來驗證 Cookie + [Authorize] 是否真的生效
        [Authorize]
        public IActionResult Secret()
        {
            return Content("?? 你看得到這頁，代表你已成功登入");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
