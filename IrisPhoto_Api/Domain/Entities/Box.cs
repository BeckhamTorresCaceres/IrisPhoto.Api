using IrisPhoto_Api.Domain.Enums;

namespace IrisPhoto_Api.Domain.Entities
{
    public class Box
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public PasswordType PasswordType { get; set; }
        public string Password { get; set; }
    }
}
