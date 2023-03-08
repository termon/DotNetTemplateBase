using System;
namespace Template.Data.Entities
{
   
    public class ForgotPassword
    {
        public int Id { get; set; }
        public string Email { get; set; }

        public string Token { get; set; } = Guid.NewGuid().ToString();
  
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddHours(1);

    }
}
