using Tema6PSSC.Domeniu.Modele;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tema6PSSC.Domeniu.Modele.CosCumparaturi;

namespace Tema6PSSC.Domeniu.Repositories
{
    public interface IRepositoryProduse
    {
        TryAsync<List<CodProdus>> ObtinereProduseExistente(IEnumerable<string> produsDeVerificat);
       

    }
}
