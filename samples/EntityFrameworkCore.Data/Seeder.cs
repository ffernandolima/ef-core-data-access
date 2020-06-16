using EntityFrameworkCore.Models;
using System.Collections.Generic;

namespace EntityFrameworkCore.Data
{
    public static class Seeder
    {
        public static Blog SeedBlog(int idx)
        {
            var blog = new Blog
            {
                Url = $"/a/{idx}",
                Title = $"a{idx}",
                Type = new BlogType
                {
                    Description = $"z{idx}"
                },
                Posts = new List<Post>
                {
                    new Post
                    {
                        Title = "A",
                        Content = "A's content",
                        Comments = new List<Comment>
                        {
                            new Comment
                            {
                                Title = "A",
                                Content = "A's content",
                            },
                            new Comment
                            {
                                Title = "B",
                                Content = "B's content",
                            },
                            new Comment
                            {
                                Title = "C",
                                Content = "C's content",
                            }
                        }
                    },
                    new Post
                    {
                        Title = "B",
                        Content = "B's content",
                        Comments = new List<Comment>
                        {
                            new Comment
                            {
                                Title = "A",
                                Content = "A's content",
                            },
                            new Comment
                            {
                                Title = "B",
                                Content = "B's content",
                            },
                            new Comment
                            {
                                Title = "C",
                                Content = "C's content",
                            }
                        }
                    },
                    new Post
                    {
                        Title = "C",
                        Content = "C's content",
                        Comments = new List<Comment>
                        {
                            new Comment
                            {
                                Title = "A",
                                Content = "A's content",
                            },
                            new Comment
                            {
                                Title = "B",
                                Content = "B's content",
                            },
                            new Comment
                            {
                                Title = "C",
                                Content = "C's content",
                            }
                        }
                    },
                    new Post
                    {
                        Title = "D",
                        Content = "D's content",
                        Comments = new List<Comment>
                        {
                            new Comment
                            {
                                Title = "A",
                                Content = "A's content",
                            },
                            new Comment
                            {
                                Title = "B",
                                Content = "B's content",
                            },
                            new Comment
                            {
                                Title = "C",
                                Content = "C's content",
                            }
                        }
                    }
                }
            };

            return blog;
        }

        public static IList<Blog> SeedBlogs(int count = 50)
        {
            var blogs = new List<Blog>();

            for (int i = 1; i <= count; i++)
            {
                var blog = SeedBlog(i);

                blogs.Add(blog);
            }

            return blogs;
        }
    }
}
