using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace ECommerce_Backend_Task1.View_Models.Product
{
    public class CreateProduct:BaseProduct
    {
        [Required]
        [MinLength(3)]
        public string Title { get; set; }
    }
}
