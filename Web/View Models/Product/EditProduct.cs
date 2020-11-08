using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce_Backend_Task1.View_Models.Product
{
    public class EditProduct:BaseProduct
    {
        public Guid Id { get; set; }
        [MinLength(3)]
        public string Title { get; set; }
    }
}
