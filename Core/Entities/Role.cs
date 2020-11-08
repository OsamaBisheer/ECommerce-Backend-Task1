using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Entities
{
    public class Role:Base
    {
        [Required]
        [MinLength(3)]
        public string Name { get; set; }

        public virtual List<User> Users { get; set; }
    }
}
