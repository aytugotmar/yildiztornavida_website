using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using WebApplication.Models.Model;

namespace WebApplication.Models.DataContext
{
    public class BlogDBContext : DbContext
    {
        public BlogDBContext() : base("name=BlogDB")
        {
          
        }
        public DbSet<About_Us> About_Us { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Blog> Blog { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Contact> Contact { get; set; }
        public DbSet<ID> ID { get; set; }
        public DbSet<Services> Services { get; set; }
        public DbSet<Author> Author { get; set; }
        public DbSet<Comment> Comment { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Çoğullaştırma kuralını kapat
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        internal object Include(string v)
        {
            throw new NotImplementedException();
        }
    }
}