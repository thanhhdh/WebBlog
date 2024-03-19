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
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public INotyfService _notyfService;
        public PostController(ApplicationDbContext context, 
            INotyfService notyfService,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _notyfService = notyfService;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var listPosts = new List<Post>();
            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var loggedInUserRole = await _userManager.GetRolesAsync(loggedInUser!);
            if (loggedInUserRole[0] == WebsiteRoles.WebsiteAdmin)
            {
                listPosts = await _context.Posts.Include(x => x.ApplicationUser).ToListAsync();
            } else
            {
                listPosts = await _context.Posts.Include(x => x.ApplicationUser).Where(x=>x.ApplicationUser!.Id == loggedInUser!.Id).ToListAsync();
            }
            var listOfPostVM = listPosts.Select(x => new PostVm()
            {
                Id = x.Id,
                Title = x.Title,
                CreatedDate = x.CreatedDate,
                ThumbnailUrl = x.ThumbnailUrl,
                AuthorName = x.ApplicationUser!.FirstName + " " + x.ApplicationUser.LastName
            }).ToList();
            return View(listOfPostVM);
        }



        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePostVM());
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreatePostVM vm)
        {
            if(!ModelState.IsValid)
            {
                return View(vm);
            }
            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var post = new Post();
            post.Title = vm.Title;
            post.Description = vm.Description;
            post.ShortDescription = vm.ShortDescription;
            post.ApplicationUserId = loggedInUser!.Id;
            if(post.Title!= null)
            {
                string slug = vm.Title!.Trim();
                slug = slug.Replace(" ", "-");
                post.Slug = slug + "-" + Guid.NewGuid();
            }
            if(vm.Thumbnail != null)
            {
                post.ThumbnailUrl = UploadImage(vm.Thumbnail);
            }

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
            _notyfService.Success("Created Post successfully!");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == id);
            var loggedInUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            var loggedInUserRole = await _userManager.GetRolesAsync(loggedInUser!);
            if (loggedInUserRole[0] == WebsiteRoles.WebsiteAdmin || loggedInUser?.Id == post?.ApplicationUserId)
            {
                _context.Posts.Remove(post!); 
                await _context.SaveChangesAsync();
                _notyfService.Success("Delete post successfully!");
                return RedirectToAction("Index", "Post", new {area = "Admin"});
            }
            return View();

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
