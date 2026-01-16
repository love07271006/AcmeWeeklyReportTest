using AcmeWeeklyReportTest.Data;
using AcmeWeeklyReportTest.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AcmeWeeklyReportTest.Helpers;

namespace AcmeWeeklyReportTest.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }
        #region 把明碼轉雜湊Action(暫時新增)
        //[HttpGet]
        //public async Task<IActionResult> MigratePasswords()
        //{
        //    var users = await _db.Users.ToListAsync();

        //    foreach (var u in users)
        //    {
        //        // 如果本來就沒有密碼，跳過
        //        if (string.IsNullOrWhiteSpace(u.PasswordHash))
        //            continue;

        //        // 目前 PasswordHash 裡放的是「明碼」
        //        var plain = u.PasswordHash;

        //        // 轉成真正的 Hash
        //        u.PasswordHash = PasswordHelper.Hash(plain);
        //    }

        //    await _db.SaveChangesAsync();
        //    return Content("OK - password migrated");
        //}
        #endregion

        /// <summary>
        /// 登入(HttpGet)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login()
        {
            //已登入就不要再顯示登入頁，直接導到主頁/清單頁
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "WorkLog"); // 或 Home/Index

            ViewData["Title"] = "登入";
            return View(new LoginViewModel());
        }

        /// <summary>
        /// 登入(HttpPost)
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            ViewData["Title"] = "登入";

            if (!ModelState.IsValid)
                return View(vm);

            // 1) 先用帳號找到使用者（而且必須啟用）
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Account == vm.Account && u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "帳號或密碼錯誤");
                return View(vm);
            }


            // 2) 驗證密碼（先用明碼比對示範，不改DB結構）
            // 目前欄位叫 PasswordHash，但若裡面存的是明碼，這樣就能先跑通
            #region 第一版密碼驗證(明碼暫時註解)
            //if (user.PasswordHash != vm.Password)
            //{
            //    ModelState.AddModelError(string.Empty, "帳號或密碼錯誤");
            //    return View(vm);
            //}
            #endregion
            if (!PasswordHelper.Verify(user.PasswordHash, vm.Password))
            {
                ModelState.AddModelError(string.Empty, "帳號或密碼錯誤");
                return View(vm);
            }
            
            #region 把Claims 包起來,加密寫成 Cookie,回傳給瀏覽器
            // 3) 登入成功：寫入 Cookie（Claims 內放 user.Sn）
            var role = (user.Account == "test02")
                        ? "Manager"
                        : "User";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Sn.ToString()),
                new Claim(ClaimTypes.Name, user.Account),
                new Claim("UserName", user.UserName),
                new Claim(ClaimTypes.Role, role)
            };
            //把Claim的資料包成identity(登入身分資訊)
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //把「身分資料（Identity）」包成「使用者（Principal）」
            var principal = new ClaimsPrincipal(identity);
            //正式告訴 ASP.NET Core：這個 Request 要登入這個使用者
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            #endregion

            TempData["Msg"] = $"歡迎 {user.UserName}";
            //return RedirectToAction("Index", "Home"); //導到登入頁首頁
            return RedirectToAction("Index", "WorkLog");//導到週報系統個人首頁
        }

        /// <summary>
        /// 登出(HttpPost)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
