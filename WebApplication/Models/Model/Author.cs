using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication.Models.Model
{
    [Table("Author")]
    public class Author
    {
        [Key]
        public int AuthorId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Bio { get; set; }
        public string ImageURL { get; set; }
        public ICollection<Blog> Blogs { get; set; }
    }
}