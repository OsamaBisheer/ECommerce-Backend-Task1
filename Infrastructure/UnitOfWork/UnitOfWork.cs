using Core.Interfaces;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        //private IGenericRepository<T> _entity;
        private IProductRepository _product;
        private IRoleRepository _role;
        private IUserRepository _user;

        public UnitOfWork(DataContext context)
        {
            _context = context;
        }

       

        public IUserRepository User {
            get
            {
                return _user ?? (_user = new UserRepository(_context));
            }
        }

        public IProductRepository Product
        {
            get
            {
                return _product ?? (_product = new ProductRepository(_context));
            }
        }

        public IRoleRepository Role
        {
            get
            {
                return _role ?? (_role = new RoleRepository(_context));
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

