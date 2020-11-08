using Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class Product:Base
    {
        public string Title { get; set; }
        public Color Color { get; set; }
        public int Quantity { get; set; }
        public bool SoftDeleted { get; set; }
    }
}
