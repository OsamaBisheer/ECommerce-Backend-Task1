using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce_Backend_Task1.View_Models.User
{
    public class FrontUser:BaseUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public bool EmailConfirmed { get; set; }
        public Guid Id { get; set; }
    }
}
