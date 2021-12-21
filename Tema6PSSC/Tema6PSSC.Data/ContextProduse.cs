using Tema6PSSC.Data.Modele;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Tema6PSSC.Data
{
    public class ContextProduse : DbContext
    {
        public ContextProduse(DbContextOptions<ContextProduse> options) : base(options)
        {
        }
        //ne trebuie o proprietate DbSet pentru fiecare tabel
        //pt a interactiona cu baza de date
        public DbSet<ProdusDto> Produse { get; set; }
        public DbSet<AntetComandaDto> AntetComenzi { get; set; }
        public DbSet<LinieComandaDto> LinieComenzi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProdusDto>().ToTable("Produs").HasKey(p => p.IdProdus);
            modelBuilder.Entity<LinieComandaDto>().ToTable("LinieComanda").HasKey(l => l.IdLinieComanda);
            modelBuilder.Entity<AntetComandaDto>().ToTable("AntetComenzi").HasKey(a => a.IdComanda);

        }
    }
}
