using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FamilyTree.Models;
using FamilyTree.Models.ViewModel;
using FamilyTree.Models.RegionModel;

namespace FamilyTree.Data
{
    public class FamilyTreeContext : DbContext
    {
        public FamilyTreeContext (DbContextOptions<FamilyTreeContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Event> Events { get; set; }
		public DbSet<Region> Regions { get; set; }

        public DbSet<Province> Provinces { get; set; }
		public DbSet<City> Cities { get; set; }
		public DbSet<Area> Areas { get; set; }
		public DbSet<Street> Streets { get; set; }
		public DbSet<Village> Villages { get; set; }




		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().ToTable("Persons");

            modelBuilder.Entity<Event>().ToTable("Events");

            modelBuilder.Entity<Region>().ToTable("Regions");

            modelBuilder.Entity<Province>().ToTable("Provinces");
			modelBuilder.Entity<City>().ToTable("Cities");
			modelBuilder.Entity<Area>().ToTable("Areas");
			modelBuilder.Entity<Street>().ToTable("Streets");
			modelBuilder.Entity<Village>().ToTable("Villages");


		}


	}
}
