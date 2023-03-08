using System.ComponentModel.DataAnnotations;

namespace Template.Web.Models.User;
public class ForgotPasswordViewModel
{
    [Required]
    public string Email { get; set; }
    
}
