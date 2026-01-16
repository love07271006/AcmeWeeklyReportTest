using AcmeWeeklyReportTest.Data;
using AcmeWeeklyReportTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcmeWeeklyReportTest.Controllers
{
    [Authorize] // 🔒 一定要登入才能用
    public class WorkLogController : Controller
    {
        private readonly AppDbContext _db;

        public WorkLogController(AppDbContext db)
        {
            _db = db;
        }
        private bool TryGetUserSn(out int userSn)
        {
            userSn = 0;
            var userSnStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userSnStr, out userSn);
        }

        private Task<WorkLog?> FindOwnedWorkLogAsync(int id, int userSn)
        {
            // ✅ Owner Check：Sn + UserSn 一起查
            return _db.WorkLogs.FirstOrDefaultAsync(x => x.Sn == id && x.UserSn == userSn);
        }

        /// <summary>
        /// 首頁
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index(WorkLogIndexViewModel vm)
        {
            // 讀取登入成功時(AuthController Login)寫入 Cookie 的 Claim,ClaimTypes.NameIdentifier → 使用者在資料庫的主鍵(UserSn)
            var userSnStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userSnStr, out var userSn))
                return RedirectToAction("Login", "Auth");

            //var q = _db.WorkLogs.AsQueryable();
            var q = _db.WorkLogs
                    .Include(w => w.UserSnNavigation)
                    .AsQueryable();
            // 只看自己的
            //q = q.Where(w => w.UserSn == userSn);
            if (!User.IsInRole("Manager"))
            {
                q = q.Where(w => w.UserSn == userSn);
            }

            // 日期區間（比 StartDate）
            if (vm.StartFrom.HasValue)
            {
                var from = DateOnly.FromDateTime(vm.StartFrom.Value);
                q = q.Where(w => w.StartDate >= from);
            }
            if (vm.StartTo.HasValue)
            {
                var to = DateOnly.FromDateTime(vm.StartTo.Value);
                q = q.Where(w => w.StartDate <= to);
            }

            // 狀態
            if (vm.Status.HasValue)
            {
                q = q.Where(w => w.Status == (int)vm.Status.Value);
            }

            // 關鍵字（WorkItem / Remark）
            if (!string.IsNullOrWhiteSpace(vm.Keyword))
            {
                var kw = vm.Keyword.Trim();
                q = q.Where(w => w.WorkItem.Contains(kw) || (w.Remark != null && w.Remark.Contains(kw)));
            }

            vm.Items = await q
                .OrderByDescending(w => w.StartDate)
                .ThenByDescending(w => w.Sn)
                .ToListAsync();

            return View(vm);
        }

        /// <summary>
        /// 明細(HttpGet)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // 1) 取得登入者 userSn
            if (!TryGetUserSn(out var userSn))
                return RedirectToAction("Login", "Auth");

            WorkLog? entity;

            // 2) 角色分流
            if (User.IsInRole("Manager"))
            {
                // ✅ 主管：可看任何人的資料（不做 Owner Check）
                entity = await _db.WorkLogs
                    .Include(w => w.UserSnNavigation)
                    .FirstOrDefaultAsync(w => w.Sn == id);
            }
            else
            {
                // ✅ 一般使用者：只能看自己的資料
                entity = await FindOwnedWorkLogAsync(id, userSn);
            }

            if (entity == null)
                return NotFound();

            // 3) 丟給 View
            return View(entity);
        }

        /// <summary>
        /// 新增資料(HttpGet)(目前主管無效)
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            // ViewModel 本身已經設定預設值
            return View(new WorkLogCreateViewModel());
        }

        /// <summary>
        /// 新增資料(HttpPost)(目前主管無效)
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkLogCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // 從登入資訊拿 UserSn
            var userSnStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userSnStr, out var userSn))
                return RedirectToAction("Login", "Auth");

            var entity = new WorkLog
            {
                UserSn = userSn,
                StartDate = DateOnly.FromDateTime(vm.StartDate),
                EndDate = DateOnly.FromDateTime(vm.EndDate),
                WorkItem = vm.WorkItem.Trim(),
                Status = (int)vm.Status,
                WorkHours = vm.WorkHours,
                Remark = string.IsNullOrWhiteSpace(vm.Remark) ? null : vm.Remark.Trim(),
                CreatedAt = DateTime.Now,
                UpdatedAt = null
            };

            _db.WorkLogs.Add(entity);
            await _db.SaveChangesAsync();

            TempData["Msg"] = "新增成功";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 編輯(HttpGet)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "User,Admin")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // ✅ 1) 取得登入者 UserSn
            if (!TryGetUserSn(out var userSn))
                return RedirectToAction("Login", "Auth");

            // ✅ 2) Owner Check：這筆必須是我的
            var entity = await FindOwnedWorkLogAsync(id, userSn);
            if (entity == null)
                return NotFound();

            // ✅ 3) Entity -> ViewModel（DateOnly -> DateTime、int -> enum）
            var vm = new WorkLogCreateViewModel
            {
                StartDate = entity.StartDate.ToDateTime(TimeOnly.MinValue),
                EndDate = entity.EndDate.ToDateTime(TimeOnly.MinValue),
                WorkItem = entity.WorkItem,
                Status = (WorkStatus)entity.Status,
                WorkHours = entity.WorkHours,
                Remark = entity.Remark
            };

            return View(vm);
        }

        /// <summary>
        /// 編輯(HttpPost)
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [Authorize(Roles = "User,Admin")]
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkLogCreateViewModel vm)
        {
            // ✅ 1) 取得登入者 UserSn
            if (!TryGetUserSn(out var userSn))
                return RedirectToAction("Login", "Auth");

            // ✅ 2) 先做 Owner Check（避免別人用 POST 改我的資料）
            var entity = await FindOwnedWorkLogAsync(id, userSn);
            if (entity == null)
                return NotFound();

            // ✅ 3) 後端驗證（你已經加了日期/工時卡控）
            if (!ModelState.IsValid)
                return View(vm);

            // ✅ 4) ViewModel -> Entity（DateTime -> DateOnly、enum -> int）
            entity.StartDate = DateOnly.FromDateTime(vm.StartDate);
            entity.EndDate = DateOnly.FromDateTime(vm.EndDate);
            entity.WorkItem = vm.WorkItem;
            entity.Status = (int)vm.Status;
            entity.WorkHours = vm.WorkHours;
            entity.Remark = vm.Remark;
            entity.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            TempData["Msg"] = "更新成功";
            return RedirectToAction("Index");
        }


        /// <summary>
        /// 刪除(HttpGet)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userSnStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userSnStr, out var userSn))
                return RedirectToAction("Login", "Auth");

            var entity = await _db.WorkLogs
                .FirstOrDefaultAsync(w => w.Sn == id && w.UserSn == userSn);

            if (entity == null)
                return NotFound();

            return View(entity);
        }

        /// <summary>
        /// 刪除(HttpPost)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "User,Admin")]
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // ✅ 1) 取得登入者 userSn
            if (!TryGetUserSn(out var userSn))
                return RedirectToAction("Login", "Auth");

            // ✅ 2) Owner Check：只能刪自己的資料
            var entity = await FindOwnedWorkLogAsync(id, userSn);
            if (entity == null)
                return NotFound();

            // ✅ 3) 刪除
            _db.WorkLogs.Remove(entity);
            await _db.SaveChangesAsync();

            TempData["Msg"] = "刪除成功";
            return RedirectToAction("Index");
        }

    }
}
