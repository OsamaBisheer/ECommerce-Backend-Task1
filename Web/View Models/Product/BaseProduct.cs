using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce_Backend_Task1.View_Models.Product
{
    public class BaseProduct
    {
        [Range(0, 2)]
        public Color Color { get; set; }
        public int Quantity { get; set; }
    }
}
