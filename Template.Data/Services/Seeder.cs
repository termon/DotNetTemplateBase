﻿
using Bogus;
using Template.Data.Entities;
using Template.Data.Repositories;
using Template.Data.Security;

namespace Template.Data.Services;

public static class Seeder
{
    // use this class to seed the database with dummy test data using an IUserService 
    public static void Seed(IUserService svc)
    {
        // seeder destroys and recreates the database - NOT to be called in production!!!
        svc.Initialise();

        // add users
        svc.AddUser("Administrator", "admin@mail.com", "password", Role.admin);
        svc.AddUser("Manager", "manager@mail.com", "password", Role.manager);
        svc.AddUser("Guest", "guest@mail.com", "password", Role.guest);

    }


    public static void SeedDb(DatabaseContext db = null)
    {
        // if db is null, create a new default instance using the factory
        db ??= DatabaseContextFactory.CreateContext();

        // seeder destroys and recreates the database - DO NOT call in production!!!
        db.Initialise();

        db.Users.Add(
            new User
            {
                Name = "Administrator",
                Email = "admin@mail.com",
                Password = Hasher.CalculateHash("password"),
                Role = Role.admin
            }
        );

        db.Users.Add(
            new User
            {
                Name = "Manager",
                Email = "manager@mail.com",
                Password = Hasher.CalculateHash("password"),
                Role = Role.manager
            }
        );

        db.Users.Add(
            new User
            {
                Name = "Guest",
                Email = "guest@mail.com",
                Password = Hasher.CalculateHash("password"),
                Role = Role.guest
            }
        );


        // use Bogus to generate random user data
        var faker = new Faker();
        for (int i = 1; i <= 20; i++)
        {
            db.Users.Add(
                new User
                {
                    Name = faker.Name.FullName(),
                    Email = faker.Internet.Email(),
                    Password = Hasher.CalculateHash("password"),
                    Role = Role.guest
                }
            );
        }

        db.SaveChanges();

    }

}