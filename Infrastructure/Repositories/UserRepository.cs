using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly DataContext _context;
        private DbSet<User> table = null;

        public UserRepository(DataContext context) : base(context)
        {
            _context = context;
            table = _context.Set<User>();
        }
    }
}