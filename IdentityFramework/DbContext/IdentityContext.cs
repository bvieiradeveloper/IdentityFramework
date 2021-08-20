using IdentityFramework.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityFramework.DbContext
{
    public class IdentityContext : IdentityDbContext<UserBase>
    {
        public IdentityContext(DbContextOptions options) : base(options)
        {

        }
    }
}
