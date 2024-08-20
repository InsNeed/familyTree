using FamilyTree.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using SQLitePCL;

namespace FamilyTree.Data
{
    public class DbInitializer
    {
        public static void Initialize(FamilyTreeContext context)
        {
            //context.Database.EnsureCreated();

            // Look for any students.
            if (context.Persons.Any())
            {
                return;   // DB has been seeded
            }

            var persons = new Person[]
            {
                new Person
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Birthday = DateTime.Parse("1995-04-25"),
                    Father = 1,
                    Mother = 2
                },
                new Person
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Birthday = DateTime.Parse("1988-09-14"),
                    Father = 3,
                    Mother = 4
                },
                new Person
                {
                    FirstName = "Michael",
                    LastName = "Johnson",
                    Birthday = DateTime.Parse("2000-02-28"),
                    Father = 5,
                    Mother = 6,

                },
                new Person
                {
                    FirstName = "Emily",
                    LastName = "Davis",
                    Birthday = DateTime.Parse("1992-12-05"),
                    Father = 7,
                    Mother = 8,
                }
            };

            foreach (Person s in persons)
            {
                context.Persons.Add(s);
            }
            context.SaveChanges();

            var events = new Event[]
            {
                new Event
                {
                    Date = DateTime.Parse("2024-01-01"),
                    Location = "New York",
                    Activitie = "New Year Celebration",
                    Person = new Person
                    {
                        LastName = "Smith",
                        FirstName = "John",
                        Birthday = DateTime.Parse("1990-05-15"),
                        Age = 34
                    }
                },
                new Event
                {
                    Date = DateTime.Parse("2024-02-14"),
                    Location = "Los Angeles",
                    Activitie = "Valentine's Day Party",
                    Person = new Person
                    {
                        LastName = "Johnson",
                        FirstName = "Emily",
                        Birthday = DateTime.Parse("1985-12-25"),
                        Age = 38
                    }
                }
            };

            foreach (Event e in events)
            {
                context.Events.Add(e);
            }
            context.SaveChanges();


            context.SaveChanges();
        }

    }
}
