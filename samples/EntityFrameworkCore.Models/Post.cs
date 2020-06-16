using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.Models
{
    public class Post
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Blog Blog { get; set; }
        public IList<Comment> Comments { get; set; } = new List<Comment>();
        public bool ShouldSerializeComments() => Comments?.Any() ?? false;
    }
}
