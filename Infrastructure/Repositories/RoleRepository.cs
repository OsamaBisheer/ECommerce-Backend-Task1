using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        private readonly DataContext _context;
        private DbSet<Role> table = null;

        public RoleRepository(DataContext context) : base(context)
        {
            _context = context;
            table = _context.Set<Role>();
        }
    }
}
