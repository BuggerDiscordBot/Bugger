using System;
using System.Collections.Generic;

namespace Bugger.Features.Blogs
{
    public class BlogItem
    {
        public Guid BlogId { get; set; }
        public ulong Author { get; set; }
        public List<ulong> Subscribers { get; set; }
        public string Name { get; set; }
    }
}
