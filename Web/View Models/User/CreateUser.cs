using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce_Backend_Task1.View_Models.User
{
    public class CreateUser:BaseUser
    {
        [Required]
        [MinLength(3)]
        public string FirstName { get; set; }
        [Required]
        [MinLength(3)]
        public string LastName { get; set; }
        [Required]
        [MinLength(3)]
        public string FullName { get; set; }
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "You should enter the password and number of letters should be not less than 8 letters and not more than 15 and consist of english small letters , big letters , numbers and special letters")]
        public string Password { get; set; }
        [Required]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "enter a valid email")]
        public string Email { get; set; }
        [Required]
        public Guid RoleId { get; set; }

    }
}
