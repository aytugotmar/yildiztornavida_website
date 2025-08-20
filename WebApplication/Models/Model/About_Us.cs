using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication.Models.Model
{
    [Table("About_Us")]
    public class About_Us
    {
        [Key]
        public int AboutId { get; set; }
        public string Text { get; set; }
    }
}