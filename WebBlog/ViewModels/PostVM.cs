using WebBlog.Models;

namespace WebBlog.ViewModels
{
    public class PostVm
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? AuthorName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ThumbnailUrl { get; set; }


    }
}
