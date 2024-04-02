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
    [Authorize(Roles = "Admin")]
    public class SettingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public INotyfService _notyfService;
        public SettingController(ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            INotyfService notyfService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _notyfService = notyfService;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = _context.Settings.ToList();
            if(settings.Count > 0)
            {
                var vm = new SettingVM()
                {
                    Id = settings[0].Id,
                    SiteName = settings[0].SiteName,
                    Title = settings[0].Title,
                    ShortDescription = settings[0].ShortDescription,
                    ThumbnailUrl = settings[0].ThumbnailUrl,
                    FacebookUrl = settings[0].FacebookUrl,
                    TwiterUrl = settings[0].TwiterUrl,
                    GithubUrl = settings[0].GithubUrl,
                };
                return View(vm);
            }

            var setting = new Setting()
            {
                SiteName = "Demo name",
            };
            await _context.Settings.AddAsync(setting);
            await _context.SaveChangesAsync();

            var createdSetting = _context.Settings.ToList();
            var createdVm = new SettingVM()
            {
                Id = settings[0].Id,
                SiteName = settings[0].SiteName,
                Title = settings[0].Title,
                ShortDescription = settings[0].ShortDescription,
                ThumbnailUrl = settings[0].ThumbnailUrl,
                FacebookUrl = settings[0].FacebookUrl,
                TwiterUrl = settings[0].TwiterUrl,
                GithubUrl = settings[0].GithubUrl,
            };
            return View(createdVm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SettingVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            };
            var setting = await _context.Settings.FirstOrDefaultAsync(x => x.Id == vm.Id);
            if (setting == null)
            {
                _notyfService.Error("Something went wrong !");
                return View(vm);
            }
            setting.SiteName = vm.SiteName;
            setting.Title = vm.Title;
            setting.ShortDescription = vm.ShortDescription;
            setting.ThumbnailUrl = vm.ThumbnailUrl;
            setting.FacebookUrl = vm.FacebookUrl;
            setting.TwiterUrl = vm.TwiterUrl;
            setting.GithubUrl = vm.GithubUrl;

            if(vm.Thumbnail != null)
            {
                setting.ThumbnailUrl = UploadImage(vm.Thumbnail);
            }
            await  _context.SaveChangesAsync();
            _notyfService.Success("Setting upload successfully!");
            return RedirectToAction("Index", "Setting", new { area = "Admin" });
        }
        private string UploadImage(IFormFile file)
        {
            string uniqueFileName = "";
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "thumbnails");
            uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(folderPath, uniqueFileName);
            using (FileStream fileStream = System.IO.File.Create(filePath))
            {
                file.CopyTo(fileStream);
            }
            return uniqueFileName;
        }
    }
}
