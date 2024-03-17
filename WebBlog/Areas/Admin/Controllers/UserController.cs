using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBlog.Data;
using WebBlog.Models;
using WebBlog.Utilites;
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
                Email = x.Email,
            }).ToList();
            foreach (var user in vm)
            {
                var singleUser = await _userManager.FindByIdAsync(user.Id);
                var role = await _userManager.GetRolesAsync(singleUser);
                user.Role = role.FirstOrDefault();
            }
            return View(vm);
        }

        [Authorize(Roles ="Admin")]
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var existUser = await _userManager.FindByIdAsync(id);
            if(existUser == null)
            {
                _notyfService.Error("User does not exist!");
                return View();
            }
            var vm = new ResetPasswordVM()
            {
                Id = existUser.Id,
                UserName = existUser.UserName,
            };
            return View(vm);

        } 

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid) { return View(vm); };

            var existUser = await _userManager.FindByIdAsync(vm.Id);
            if(existUser == null)
            {
                _notyfService.Error("User does not exists!");
                return View(vm); ;
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(existUser);
            var result = await _userManager.ResetPasswordAsync(existUser, token, vm.NewPassword);
            if (result.Succeeded)
            {
                _notyfService.Success("Password reset successfully!");
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Register()
        {
            return View(new RegisterVM());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if(!ModelState.IsValid)
            {
                return View(vm);
            }
            var checkUserByEmail = await _userManager.FindByEmailAsync(vm.Email);
            if(checkUserByEmail != null)
            {
                _notyfService.Error("Email already exists");
                return View(vm);
            }
            var checkUserByUserName = await _userManager.FindByNameAsync(vm.UserName);
            if (checkUserByUserName != null)
            {
                _notyfService.Error("Username already exists");
                return View(vm);
            }
            var applicationUser = new ApplicationUser()
            {
                Email = vm.Email,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                UserName = vm.UserName,
            };
            var result = await _userManager.CreateAsync(applicationUser, vm.Password);
            if(result.Succeeded)
            {
                if(vm.IsAdmin)
                {
                    await _userManager.AddToRoleAsync(applicationUser, WebsiteRoles.WebsiteAdmin);
                } else
                {
                    await _userManager.AddToRoleAsync(applicationUser, WebsiteRoles.WebsiteAuthor);
                }
                _notyfService.Success("User registered successfully!");
                RedirectToAction("Index", "User", new { area = "Admin" });
            }
            return View(vm); 
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            if(!HttpContext.User.Identity.IsAuthenticated)
            {
                return View(new LoginVM());
            }
            return RedirectToAction("Index", "User", new {area = "Admin"});
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginVM vm)
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
            return RedirectToAction("Index", "Post", new {area = "Admin"});
        }

        [HttpPost]
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            _notyfService.Success("You are logout successfully!");
            return RedirectToAction("Index","Home", new {area = ""});
        }

        [HttpGet("AccessDenied")]
        [Authorize]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
