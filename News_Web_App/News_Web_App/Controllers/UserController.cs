using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using News_Models.Model;
using News_Models.Repositories;
using News_Models.UserVM;
using News_Web_App.Commponents;
using System.Security.Claims;

namespace WebUI.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleConst.AdminRole)]
    public class UserController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<UserController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public UserController(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<UserController> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager
,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _unitOfWork=unitOfWork;
        }


        [Authorize(Roles = RoleConst.AdminRole)]

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userId = claim!.Value;
            var users = _userManager.Users.Where(u => u.Id != userId && u.IsDeleted != true).Select(user => new UserVM
            {
                Id = user.Id,
                Name = user.Name,
                Address = user.Address!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                Gender = user.Gender!,
                ShowInfo = user.ShowInfo, 
                UserName = user.UserName!,
                Roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult(),
                Lock = _userManager.Users.FirstOrDefault(u => u.Id == user.Id)!.LockoutEnd == null
                | _userManager.Users.FirstOrDefault(u => u.Id == user.Id)!.LockoutEnd < DateTime.Now ? "UNLOCK" : "LOCK",
            }).ToList();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string? id)
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddUserVM userModel)
        {

            if (ModelState.IsValid)
            {

                var user = new User();
                user.Name = userModel.Name;
                user.Gender = userModel.Gender;
                user.BirthDate = userModel.BirthDate;
                user.PhoneNumber = userModel.PhoneNumber;
                user.EmailConfirmed = true;
                user.Created = DateOnly.FromDateTime(DateTime.Now);

                await _userStore.SetUserNameAsync(user, userModel.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, userModel.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, userModel.Password);

                if (result.Succeeded)
                {
                    if (userModel.Roles is not null)
                    {
                        foreach (var role in userModel.Roles)
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "محرر");
                    }
                    TempData["SuccessRegister"] = "تم انشاء المستخدم بنجاح";

                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }
            return View();
        }

        public async Task<IActionResult> Details(string? id)
        {
            var user =  _userManager.Users.Select(user => new UserVM
            {
                Id = user.Id,
                Name = user.Name,
                Address = user.Address!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                Gender = user.Gender!,
                UserName = user.UserName!,
                Roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult(),
                Image = user.ImageUrl
            }).FirstOrDefault(x => x.Id == id);

            return View(user);
        }

        public async Task<IActionResult> Edit(string? id)
        {
            var user =  _userManager.Users.Select(user => new UserVM
            {
                Id = user.Id,
                Name = user.Name,
                Address = user.Address!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                Gender = user.Gender!,
                UserName = user.UserName!,
                Roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult(),
                Image = user.ImageUrl
            }).FirstOrDefault(x => x.Id == id);
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string? id, UserVM userVM)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == id);
            user!.PhoneNumber = userVM.PhoneNumber;
            user.Gender = userVM.Gender;
            user.UserName = userVM.UserName;
            user.Email = userVM.Email;
            user.Name = userVM.Name;
            user.Address = userVM.Address;
            var currentRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in userVM.Roles)
            {
                if (!currentRoles.Contains(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
            foreach (var role in currentRoles)
            {
                if (!userVM.Roles.Contains(role))
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }
            }
            await _userManager.UpdateAsync(user);
            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files.FirstOrDefault();


                using (var dataStream = new MemoryStream())
                {
                    await file!.CopyToAsync(dataStream);
                    user.ImageUrl = dataStream.ToArray();
                }
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Edit", "User", id);
        }

        public async Task<IActionResult> Delete(string? id)
        {


            var user = _userManager.Users.FirstOrDefault(user => user.Id == id);

            if (user == null)
            {
                return Json(new { success = false, message = "المستخدم غير موجود" });
            }

            user.IsDeleted = true;
            await _userManager.UpdateAsync(user);

            return Json(new { success = true, message = "تم حذف المستخدم بنجاح" });
        }

        public async Task<IActionResult> LockUser(string id)
        {
            var user =  _userManager.Users.FirstOrDefault(user => user.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.LockoutEnd == null || user.LockoutEnd < DateTime.Now)
            {
                user.LockoutEnd = DateTime.Now.AddYears(1);
                await _userManager.SetLockoutEndDateAsync(user, user.LockoutEnd);
                return Json(new { success = true, message = "تم وقف حساب المستخدم", isLocked = true });
            }
            else
            {
                user.LockoutEnd = DateTime.Now;
                await _userManager.SetLockoutEndDateAsync(user, user.LockoutEnd);
                return Json(new { success = true, message = "تم تفعيل حساب المستخدم", isLocked = false });
            }
        }
        public async Task<IActionResult> ShowProfile(string id)
        {
            var user = _userManager.Users.FirstOrDefault(user => user.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.ShowInfo)
            {
                user.ShowInfo =false;
                await _userManager.UpdateAsync(user);
                return Json(new { success = true, message = "تم اخفاء معلومات المستخدم", iSShow = false });
            }
            else
            {
                user.ShowInfo = true;
                await _userManager.UpdateAsync(user);
                return Json(new { success = true, message = "تم اظهار معلومات المستخدم", iSShow = true });
            }
        }
        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
        
    }
}
