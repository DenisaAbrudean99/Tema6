using Tema6PSSC.Domeniu.Modele;
using Tema6PSSC.Domeniu.Repositories;
using LanguageExt;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Tema6PSSC.Data.Repositories
{
    public class RepositoryProduse : IRepositoryProduse
    {
        private readonly ContextProduse contextProduse;
        public RepositoryProduse(ContextProduse contextProduse)
        {
            this.contextProduse = contextProduse;
        }

        public TryAsync<List<CodProdus>> ObtinereProduseExistente(IEnumerable<string> produsDeVerificat) => async () =>
        {
            var produse = await contextProduse.Produse
                                              .Where(produs => produsDeVerificat.Contains(produs.Cod))
                                              .AsNoTracking()
                                              .ToListAsync();
            return produse.Select(produs => new CodProdus(produs.Cod))
                           .ToList();

        };


    }
}
