using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication.Models.Model
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        [Required]
        public string CommentText { get; set; }
        [Required]
        public bool isApproved { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public int AdminId { get; set; }
        public Admin Admin { get; set; }
        public int? BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}