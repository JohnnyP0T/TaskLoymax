using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaskLoymax.WebApi.Models;

namespace TaskLoymax.WebApi.Infrastructure
{
    public class ClientContext : DbContext
    {
        public ClientContext(DbContextOptions<ClientContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; } = null!;
    }
}
