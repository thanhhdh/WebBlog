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
    public class TagController : Controller
    {
        private readonly ApplicationDbContext _context;
        public INotyfService _notyfService;
        public TagController(ApplicationDbContext context,
            INotyfService notyfService
           )
        {
            _context = context;
            _notyfService = notyfService;
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

    }
}
