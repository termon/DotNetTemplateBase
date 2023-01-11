
using Template.Data.Models;

namespace Template.Data.Services
{

    // This interface describes the operations that a UserService class implementation should provide
    public interface IUserService
    {
        // Initialise the repository - only to be used during development 
        void Initialise();

        // ---------------- User Management --------------
        IList<User> GetUsers();
        Paged<User> GetUsers(int page=1, int size=20, string orderBy="id", string direction="asc");
        User GetUser(int id);
        User GetUserByEmail(string email);
        bool IsEmailAvailable(string email, int userId);
        User AddUser(string name, string email, string password, Role role);
        User UpdateUser(User user);
        bool DeleteUser(int id);
        User Authenticate(string email, string password);
        string ForgotPassword(string email);
        User ResetPassword(string email, string token, string password);
        IList<string> GetValidPasswordResetTokens();
    }

}