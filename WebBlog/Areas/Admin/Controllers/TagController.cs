using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using WebBlog.Data;
using WebBlog.Models;
using WebBlog.Utilites;
using WebBlog.ViewModels;

namespace WebBlog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TagController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public INotyfService _notyfService;
        public TagController(ApplicationDbContext context,
            INotyfService notyfService,
            UserManager<ApplicationUser> userManager
           )
        {
            _context = context;
            _notyfService = notyfService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var tags = await _context.Tags.ToListAsync();
            var vm = tags.Select(x => new TagVM()
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.slug
            }).ToList();
            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new TagVM());
        }

        [HttpPost]
        public async Task<IActionResult> Create(TagVM vm)
        {
            var existTag = await _context.Tags.FirstOrDefaultAsync(x => x.Title == vm.Title);
            if (existTag != null)
            {
                _notyfService.Error("Tag already exists");
                return View(vm);
            }
            var tag = new Tag();

            tag.Title = vm.Title;
            tag.slug = vm.Slug;

            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();
            _notyfService.Success("Tag added successfully!");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == id);
            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var loggedInUserRole = await _userManager.GetRolesAsync(loggedInUser!);
            if (loggedInUserRole[0] == WebsiteRoles.WebsiteAdmin)
            {
                _context.Tags.Remove(tag!);
                await _context.SaveChangesAsync();
                _notyfService.Success("Delete tag successfully!");
                return RedirectToAction("Index", "Tag", new { area = "Admin" });
            }
            return View();
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var tag =  _context.Tags.FirstOrDefault(x => x.Id == id);
            if (tag == null)
            {
                _notyfService.Error("Tag not found!");
                return View();
            }
            var vm = new TagVM()
            {
                Title = tag.Title,
                Slug = tag.slug
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(TagVM vm)
        {
            if(!ModelState.IsValid)
            {
                return View(vm);
            }
            var tag = _context.Tags.FirstOrDefault(x => x.Id == vm.Id);
            if (tag == null)
            {
                _notyfService.Error("Tag not found!");
                return View();
            }
            tag.Title = vm.Title;
            tag.slug = vm.Slug;
            await _context.SaveChangesAsync();
            _notyfService.Success("Tag updated successfully!");
            return RedirectToAction("Index", "Tag", new { area = "Admin" });
        }

    }
}
