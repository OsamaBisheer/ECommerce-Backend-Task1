using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class User:Base
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool EmailConfirmed { get; set; }

        [ForeignKey("Role")]
        public Guid RoleId { get; set; }

        public virtual Role Role { get; set; }
    }
}
