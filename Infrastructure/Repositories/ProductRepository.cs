using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly DataContext _context;
        private DbSet<Product> table = null;

        public ProductRepository(DataContext context) : base(context)
        {
            _context = context;
            table = _context.Set<Product>();
        }

        public override Product GetById(object id, params string[] includes)
        {
            var product = base.GetById(id, includes);

            Product result = null;

            if (product != null && !product.SoftDeleted) result = product;

            return result;
        }

        public override List<Product> Get(Func<Product, bool> filter = null, params string[] includes)
        {
            return base.Get(filter, includes).Where(p=>!p.SoftDeleted).ToList();
        }
    }
}