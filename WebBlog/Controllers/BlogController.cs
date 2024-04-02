using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBlog.Data;
using WebBlog.Models;
using WebBlog.ViewModels;

namespace WebBlog.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        public INotyfService _notyfService;
        public BlogController(ApplicationDbContext context,
            INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        [HttpGet("[controller]/{slug}")]
        public IActionResult Post(string slug)
        {
            if(slug == null)
            {
                _notyfService.Error("Post not found");
                return View();
            }
            var post = _context.Posts.Include(x => x.ApplicationUser).FirstOrDefault(x => x.Slug == slug);
            if (post == null)
            {
                _notyfService.Error("Post not found");
                return View();
            }
            var vm = new BlogPostVM()
            {
                Id = post.Id,
                Title = post.Title,
                AuthorName = post.ApplicationUser!.FirstName + " " + post.ApplicationUser.LastName,
                CreatedDate = post.CreatedDate,
                ThumbnailUrl = post.ThumbnailUrl,
                Description = post.Description,
                ShortDescription = post.ShortDescription,
            };
            return View(vm);
        }
    }
}
