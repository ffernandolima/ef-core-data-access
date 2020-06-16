
namespace EntityFrameworkCore.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Post Post { get; set; }
    }
}
