
using Xunit;
using Template.Data.Models;
using Template.Data.Services;

using Microsoft.EntityFrameworkCore;
using Template.Data.Repositories;

namespace Template.Test
{
    public class ServiceTests
    {
        private readonly IUserService service;

        public ServiceTests()
        {
            // configure the data context options to use sqlite for testing
            var options = DatabaseContext.OptionsBuilder                            
                            .UseSqlite("Filename=test.db")
                            //.LogTo(Console.WriteLine)
                            .Options;

            // create service with new context
            service = new UserServiceDb(new DatabaseContext(options));
            service.Initialise();
        }

         [Fact]
        public void EmptyDb_ShouldReturn_NoUsers()
        {
            // act
            var users = service.GetUsers();

            // assert
            Assert.Equal(0, users.Count);
        }
        
        [Fact]
        public void Adding_Users_ShouldWork()
        {
            // arrange
            service.AddUser("admin", "admin@mail.com", "admin", Role.admin );
            service.AddUser("guest", "guest@mail.com", "guest", Role.guest);

            // act
            var users = service.GetUsers();

            // assert
            Assert.Equal(2, users.Count);
        }

        [Fact]
        public void Updating_User_ShouldWork()
        {
            // arrange
            var user = service.AddUser("admin", "admin@mail.com", "admin", Role.admin );
            
            // act
            user.Name = "administrator";
            user.Email = "admin@mail.com";            
            var updatedUser = service.UpdateUser(user);

            // assert
            Assert.Equal("administrator", user.Name);
            Assert.Equal("admin@mail.com", user.Email);
        }

        [Fact]
        public void Login_WithValidCredentials_ShouldWork()
        {
            // arrange
            service.AddUser("admin", "admin@mail.com", "admin", Role.admin );
            
            // act            
            var user = service.Authenticate("admin@mail.com","admin");

            // assert
            Assert.NotNull(user);
           
        }

        [Fact]
        public void Login_WithInvalidCredentials_ShouldNotWork()
        {
            // arrange
            service.AddUser("admin", "admin@mail.com", "admin", Role.admin );

            // act      
            var user = service.Authenticate("admin@mail.com","xxx");

            // assert
            Assert.Null(user);
           
        }

        [Fact]
        public void ForgotPasswordRequest_ForValidUser_ShouldGenerateToken()
        {
            // arrange
            service.AddUser("admin", "admin@mail.com", "admin", Role.admin );

            // act      
            var token = service.ForgotPassword("admin@mail.com");

            // assert
            Assert.NotNull(token);
           
        }

        [Fact]
        public void ForgotPasswordRequest_ForInValidUser_ShouldReturnNull()
        {
            // arrange
          
            // act      
            var token = service.ForgotPassword("admin@mail.com");

            // assert
            Assert.Null(token);
           
        }

        [Fact]
        public void ResetPasswordRequest_WithValidUserAndToken_ShouldReturnUser()
        {
            // arrange
            service.AddUser("admin", "admin@mail.com", "admin", Role.admin );
            var token = service.ForgotPassword("admin@mail.com");
            
            // act      
            var user = service.ResetPassword("admin@mail.com", token, "password");
        
            // assert
            Assert.NotNull(user);
            Assert.Equal("password", user.Password);          
        }

        [Fact]
        public void ResetPasswordRequest_WithValidUserAndExpiredToken_ShouldReturnNull()
        {
            // arrange
            service.AddUser("admin", "admin@mail.com", "admin", Role.admin );
            var expiredToken = service.ForgotPassword("admin@mail.com");
            var token = service.ForgotPassword("admin@mail.com");
            
            // act      
            var user = service.ResetPassword("admin@mail.com", expiredToken, "password");
        
            // assert
            Assert.Null(user);  
        }

        [Fact]
        public void ResetPasswordRequest_WithInValidUserAndValidToken_ShouldReturnNull()
        {
            // arrange
            service.AddUser("admin", "admin@mail.com", "admin", Role.admin );          
            var token = service.ForgotPassword("admin@mail.com");
            
            // act      
            var user = service.ResetPassword("unknown@mail.com", token, "password");
        
            // assert
            Assert.Null(user);  
        }

        [Fact]
        public void ResetPasswordRequests_WhenAllCompleted_ShouldExpireAllTokens()
        {
            // arrange
            service.AddUser("admin", "admin@mail.com", "admin", Role.admin );       
            service.AddUser("guest", "guest@mail.com", "guest", Role.guest );          

            // create token and reset password - token then invalidated
            var token1 = service.ForgotPassword("admin@mail.com");
            var user1 = service.ResetPassword("admin@mail.com", token1, "password");

            // create token and reset password - token then invalidated
            var token2 = service.ForgotPassword("guest@mail.com");
            var user2 = service.ResetPassword("guest@mail.com", token2, "password");
         
            // act  
            // retrieve valid tokens 
            var tokens = service.GetValidPasswordResetTokens();   

            // assert
            Assert.Empty(tokens);
        }

    }
}
