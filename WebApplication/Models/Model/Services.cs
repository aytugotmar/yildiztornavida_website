using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication.Models.Model
{
    [Table("Services")]
    public class Services
    {
        [Key]
        public int ServiceId { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
    }
}