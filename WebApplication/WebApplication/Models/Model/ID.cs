using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication.Models.Model
{
    [Table("ID")]
    public class ID
    {
        [Key]
        public int IdentityId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Keywords { get; set; }
        [Required]
        public string Description { get; set; }
        public string LogoURL { get; set; }
        [Required]
        public string Slogan { get; set; }
    }
}