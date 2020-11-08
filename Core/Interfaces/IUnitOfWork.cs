using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository User { get;  }
        IRoleRepository Role { get;  }
        IProductRepository Product { get; }
        void Save();
    }
}
