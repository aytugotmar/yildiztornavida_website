using System;
using System.Collections.Generic;
using WebApplication.Models.Model;

namespace WebApplication.Models.ViewModels
{
    public class ViewModel
    {
        public List<Admin> Admins { get; set; }
        public List<Category> Categories { get; set; }
        public List<Blog> Blogs { get; set; }
        public List<Author> Authors { get; set; }
        public List<About_Us> Abouts { get; set; }
        public List<Contact> Contacts { get; set; }
        public List<ID> IDs { get; set; }
        public List<Services> Services { get; set; }
    }
}