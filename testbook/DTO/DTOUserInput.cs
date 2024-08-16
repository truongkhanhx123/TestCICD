using System.ComponentModel.DataAnnotations;

namespace testbook.DTO
{
    public class DtoUserInput
    {
        [Required(ErrorMessage = "Account is required.")]
        //[StringLength(20, MinimumLength = 3, ErrorMessage = "Account must be between 3 and 20 characters long.")]
        [RegularExpression("^[a-zA-Z0-9]{3,20}$", ErrorMessage = "Account must only contain letters and digits and Account must be between 3 and 20 characters long.")]
        public required string Account { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 64 characters long.")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,64}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "UserName is required.")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "UserName must be between 3 and 25 characters long.")]
        [RegularExpression(@"^[\p{L}0-9 ]*$", ErrorMessage = "UserName must only contain letters, digits, and spaces.")]
        public required string UserName { get; set; }
    }
}
