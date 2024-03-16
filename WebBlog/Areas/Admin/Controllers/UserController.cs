using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBlog.Data;
using WebBlog.Models;
using WebBlog.ViewModels;

namespace WebBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly INotyfService _notyfService;
        public UserController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            INotyfService notyfService) {
            _userManager = userManager;
            _signInManager = signInManager;
            _notyfService = notyfService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var vm = users.Select(x => new UserVM()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                UserName = x.UserName,
            }).ToList();
            return View(vm);
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            if(!HttpContext.User.Identity.IsAuthenticated)
            {
                return View(new LoginVm());
            }
            return RedirectToAction("Index", "User", new {area = "Admin"});
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if(!ModelState.IsValid)
            {
                return View(vm);
            }
            var existUser = await _userManager.Users.FirstOrDefaultAsync(x=>x.UserName == vm.UserName);
            if(existUser == null)
            {
                _notyfService.Error("UserName does not exist!");
                return View(vm);
            }
            var verifyPassword = await _userManager.CheckPasswordAsync(existUser, vm.Password);
            if (!verifyPassword)
            {
                _notyfService.Error("Password does not match");
                return View(vm);
            }
            await _signInManager.PasswordSignInAsync(vm.UserName, vm.Password, vm.RememberMe, true);
            _notyfService.Success("Login Successful");
            return RedirectToAction("Index", "User", new {area = "Admin"});
        }

        [HttpPost]
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            _notyfService.Success("You are logout successfully!");
            return RedirectToAction("Index","Home", new {area = ""});
        }
    }
}
