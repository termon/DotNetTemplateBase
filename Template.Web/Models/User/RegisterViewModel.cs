using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Template.Data.Entities;

namespace Template.Web.Models.User;

public class RegisterViewModel
{ 
    [Required]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [Remote(action: "VerifyEmailAvailable", controller: "User")]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again !")]
    public string PasswordConfirm  { get; set; }

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Role Role { get; set; }

}
