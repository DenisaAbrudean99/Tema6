using Tema6PSSC.Domeniu.Modele;
using Tema6PSSC.Domeniu.Repositories;
using LanguageExt;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using static Tema6PSSC.Domeniu.Modele.CosCumparaturi;
using Tema6PSSC.Data.Modele;
using static LanguageExt.Prelude;


namespace Tema6PSSC.Data.Repositories
{
    public class RepositoryLinieComanda : IRepositoryLinieComanda
    {
        private readonly ContextProduse dbContext;
        public RepositoryLinieComanda(ContextProduse dbContext)
        {
            this.dbContext = dbContext;
        }

        public TryAsync<List<LinieComanda>> ObtinereLiniiComandaExistente() => async () => (await (

            from c in dbContext.LinieComenzi
            join p in dbContext.Produse on c.IdProdus equals p.IdProdus
            select new { p.Cod, c.IdLinieComanda, c.Pret, c.Cantitate, c.PretFinal })
            .AsNoTracking()
        //se executa query
            .ToListAsync())
            .Select(rezultat => new LinieComanda(
                                                CodProdus: new(rezultat.Cod),
                                                Pret: new(rezultat.Pret ?? 0m),
                                                Cantitate: new(rezultat.Cantitate ?? 0m),
                                                PretFinal: new(rezultat.Pret * rezultat.Cantitate ?? 0m))
            {
                IdLinieComanda = rezultat.IdLinieComanda
            })
                                                .ToList();

        public TryAsync<Unit> SalvareLinieComanda(CosPlatit produsecos) => async () =>
        {
            //citim lista de produse sa populez codul produsului
            var produse = (await dbContext.Produse.ToListAsync()).ToLookup(produs => produs.Cod);
            var produsenoi = produsecos.ListaProduse
                                    .Where(c => c.IsUpdated && c.IdLinieComanda == 0)
                                    .Select(c => new LinieComandaDto()
                                    {
                                        IdProdus = produse[c.CodProdus.Value].Single().IdProdus,
                                        Cantitate = c.Cantitate.Value,
                                        Pret = c.Pret.Value,
                                        PretFinal = c.PretFinal.Value

                                    });
            ;
            var produseactualizate = produsecos.ListaProduse.Where(p => p.IsUpdated && p.IdLinieComanda > 0)
                                    .Select(p => new LinieComandaDto()
                                    {
                                        IdLinieComanda = p.IdLinieComanda,
                                        IdProdus = produse[p.CodProdus.Value].Single().IdProdus,
                                        Cantitate = p.Cantitate.Value,
                                        Pret = p.Pret.Value,
                                        PretFinal = p.PretFinal.Value
                                    });
            //produsele noi sunt adaugate cu add range
            dbContext.AddRange(produsenoi);
            //cele existente le modific
            foreach (var entity in produseactualizate)
            {
                dbContext.Entry(entity).State = EntityState.Modified;
            }
            //entity framework o sa vada ce produse am
            await dbContext.SaveChangesAsync();

            return unit;
        };
    }
}
