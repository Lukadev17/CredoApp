using CredoApp.Enums;
using System.Text.Json.Serialization;

namespace CredoApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        [JsonIgnore]
        public string PasswordHash { get; set; } 
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        public string PersonalNumber { get; set; }  
        public DateTime BirthDate { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
    }
}
