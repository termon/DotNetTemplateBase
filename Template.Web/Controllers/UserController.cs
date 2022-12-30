
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

using Template.Data.Models;
using Template.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Template.Data.Security;
using Template.Web.ViewModels.User;

/**
 *  User Management Controller
 */
namespace Template.Web.Controllers
{
    public class UserController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly IMailService _mailer;
        private readonly IUserService _svc;

        public UserController(IUserService svc, IConfiguration config, IMailService mailer)
        {        
            _config = config;
            _mailer = mailer;
            _svc = svc;
        }

        // HTTP GET - Display Paged List of Users
        [Authorize]
        public ActionResult Index(int page=1, int size=10)
        {
            var paged = _svc.GetUsers(page, size);
            return View(paged);
        }

        // HTTP GET - Display Login page
        public IActionResult Login()
        {
            return View();
        }

        // HTTP POST - Login action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Email,Password")] LoginViewModel m)
        {
            var user = _svc.Authenticate(m.Email, m.Password);
            // check if login was unsuccessful and add validation errors
            if (user == null)
            {
                ModelState.AddModelError("Email", "Invalid Login Credentials");
                ModelState.AddModelError("Password", "Invalid Login Credentials");
                return View(m);
            }

            // Login Successful, so sign user in using cookie authentication
            await SignInCookie(user);

            Alert("Successfully Logged in", AlertType.info);

            return Redirect("/");
        }

        // HTTP GET - Display Register page
        public IActionResult Register()
        {
            return View();
        }

        // HTTP POST - Register action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register([Bind("Name,Email,Password,PasswordConfirm,Role")] RegisterViewModel m)       
        {
            if (!ModelState.IsValid)
            {
                return View(m);
            }
            // add user via service
            var user = _svc.AddUser(m.Name, m.Email,m.Password, m.Role);
            
            // check if error adding user and display warning
            if (user == null) {
                Alert("There was a problem Registering. Please try again", AlertType.warning);
                return View(m);
            }

            Alert("Successfully Registered. Now login", AlertType.info);
            return RedirectToAction(nameof(Login));
        }

        // HTTP GET - Display Update profile page
        [Authorize]
        public IActionResult UpdateProfile()
        {
           // use BaseClass helper method to retrieve Id of signed in user 
            var user = _svc.GetUser(User.GetSignedInUserId());
            var profileViewModel = new ProfileViewModel { 
                Id = user.Id, 
                Name = user.Name, 
                Email = user.Email,                 
                Role = user.Role
            };
            return View(profileViewModel);
        }

        // HTTP POST - Update profile action
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind("Id,Name,Email,Role")] ProfileViewModel m)       
        {
            var user = _svc.GetUser(m.Id);
            // check if form is invalid and redisplay
            if (!ModelState.IsValid || user == null)
            {
                return View(m);
            } 

            // update user details and call service
            user.Name = m.Name;
            user.Email = m.Email;
            user.Role = m.Role;        
            var updated = _svc.UpdateUser(user);

            // check if error updating service
            if (updated == null) {
                Alert("There was a problem Updating. Please try again", AlertType.warning);
                return View(m);
            }

            Alert("Successfully Updated Account Details", AlertType.info);
            
            // sign the user in with updated details)
            await SignInCookie(user);

            return RedirectToAction("Index","Home");
        }

        // HTTP GET - Allow admin to update a User
        [Authorize(Roles="admin")]
        public IActionResult Update(int id)
        {
           // retrieve user 
            var user = _svc.GetUser(id);
            var profileViewModel = new ProfileViewModel { 
                Id = user.Id, 
                Name = user.Name, 
                Email = user.Email,                 
                Role = user.Role
            };
            return View(profileViewModel);
        }

        // HTTP POST - Update User action
        [Authorize(Roles="admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update([Bind("Id,Name,Email,Role")] ProfileViewModel m)       
        {
            var user = _svc.GetUser(m.Id);
            // check if form is invalid and redisplay
            if (!ModelState.IsValid || user == null)
            {
                return View(m);
            } 

            // update user details and call service
            user.Name = m.Name;
            user.Email = m.Email;
            user.Role = m.Role;        
            var updated = _svc.UpdateUser(user);

            // check if error updating service
            if (updated == null) {
                Alert("There was a problem Updating. Please try again", AlertType.warning);
                return View(m);
            }

            Alert("Successfully Updated User Account Details", AlertType.info);                       

            return RedirectToAction("Index","User");
        }
        
        // HTTP GET - Display update password page
        [Authorize]
        public IActionResult UpdatePassword()
        {
            // use BaseClass helper method to retrieve Id of signed in user 
            var user = _svc.GetUser(User.GetSignedInUserId());
            var passwordViewModel = new PasswordViewModel { 
                Id = user.Id, 
                Password = user.Password, 
                PasswordConfirm = user.Password, 
            };
            return View(passwordViewModel);
        }

        // HTTP POST - Update Password action
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword([Bind("Id,OldPassword,Password,PasswordConfirm")] PasswordViewModel m)       
        {
            var user = _svc.GetUser(m.Id);
            if (!ModelState.IsValid || user == null)
            {
                return View(m);
            }  
            // update the password
            user.Password = m.Password; 
            // save changes      
            var updated = _svc.UpdateUser(user);
            if (updated == null) {
                Alert("There was a problem Updating the password. Please try again", AlertType.warning);
                return View(m);
            }

            Alert("Successfully Updated Password", AlertType.info);
            // sign the user in with updated details
            await SignInCookie(user);

            return RedirectToAction("Index","Home");
        }

        // HTTP POST - Logout action
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }


        // HTTP GET - Display Forgot password page
        public IActionResult ForgotPassword()
        {
            return View();            
        }

        // HTTP POST - Forgot password action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword([Bind("Email")] ForgotPasswordViewModel m)
        {           
            var token = _svc.ForgotPassword(m.Email);
            if (token == null)
            {
                // No such account. Alert only for testing
                Alert("No account found", AlertType.warning);
                return RedirectToAction(nameof(Login));
            }
            
            // build reset password url and email html message
            var url = $"{Request.Scheme}://{Request.Host}/User/ResetPassword?token={token}&email={m.Email}";
            var message = @$" 
                <h3>Password Reset</h3>
                <a href='{url}'>
                   {url}
                </a>
            ";
            
            // send email containing reset token
            if (!_mailer.SendMail( "Password Reset Request", message, m.Email ))
            {
                Alert("There was a problem sending a password reset email", AlertType.warning);
                return RedirectToAction(nameof(ForgotPassword));    
            }
            
            Alert("Password Reset Token sent to your registered email account", AlertType.info);
            return RedirectToAction(nameof(ResetPassword));
        }

        // HTTP GET - Display Reset password page
        public IActionResult ResetPassword(string token, string email)
        {
            return View();            
        }

        // HTTP POST - ResetPassword action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword([Bind("Email,Password,Token")] ResetPasswordViewModel m)
        {
            // verify reset request
            var user = _svc.ResetPassword(m.Email, m.Token, m.Password);
            if (user == null)
            {
                Alert("Invalid Password Reset Request", AlertType.warning);
                return RedirectToAction(nameof(ResetPassword));         
            }

            Alert("Password reset successfully", AlertType.success);
            return RedirectToAction(nameof(Login));          
        }
        
        // HTTP GET - Display not authorised and not authenticated pages
        public IActionResult ErrorNotAuthorised() => View();
        public IActionResult ErrorNotAuthenticated() => View();

        // -------------------------- Helper Methods ------------------------------

        // Called by Remote Validation attribute on RegisterViewModel to verify email address is available
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyEmailAvailable(string email, int id)
        {
            // check if email is available, or owned by user with id 
            if (!_svc.IsEmailAvailable(email,id))
            {
                return Json($"A user with this email address {email} already exists.");
            }
            return Json(true);                  
        }

        // Called by Remote Validation attribute on ChangePassword to verify old password
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyPassword(string oldPassword)
        {
            // use BaseClass helper method to retrieve Id of signed in user 
            var id = User.GetSignedInUserId();            
            // check if email is available, unless already owned by user with id
            var user = _svc.GetUser(id);
            if (user == null || !Hasher.ValidateHash(user.Password, oldPassword))
            {
                return Json($"Please enter current password.");
            }
            return Json(true);                  
        }

        // Sign user in using Cookie authentication scheme
        private async Task SignInCookie(User user)
        {
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                AuthBuilder.BuildClaimsPrincipal(user)
            );
        }
    }
}