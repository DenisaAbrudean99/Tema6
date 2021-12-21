using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tema6PSSC.Domeniu.Modele
{
    public record ComandaPlasareProduse
    {
        public ComandaPlasareProduse(IReadOnlyCollection<ProdusAlesNevalidat> produsinitial)
        {
            ProdusInitial = produsinitial;
        }
        public IReadOnlyCollection<ProdusAlesNevalidat> ProdusInitial { get; }
      //  public string Adresa { get; }
    }
}
