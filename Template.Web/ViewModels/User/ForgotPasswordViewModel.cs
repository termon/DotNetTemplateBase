using System.ComponentModel.DataAnnotations;

namespace Template.Web.ViewModels.User
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string Email { get; set; }
        
    }
}