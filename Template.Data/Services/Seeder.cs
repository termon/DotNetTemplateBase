
using Bogus;
using Template.Data.Entities;

namespace Template.Data.Services
{
    public static class Seeder
    {
        // use this class to seed the database with dummy test data using an IUserService 
        public static void Seed(IUserService svc)
        {
            // seeder destroys and recreates the database - NOT to be called in production!!!
            svc.Initialise();

            // add users
            svc.AddUser("Administrator", "admin@mail.com", "admin", Role.admin);
            svc.AddUser("Manager", "manager@mail.com", "manager", Role.manager);
            svc.AddUser("Guest", "guest@mail.com", "guest", Role.guest); 
        
            // optionally add some fake users
            // var faker = new Faker();
            // for(int i=1; i<=20; i++)
            // {
            //     var s = svc.AddUser(
            //         faker.Name.FullName(),
            //         faker.Internet.Email(),
            //         "password",
            //         Role.guest
            //     );
            // }
        }
    }

}