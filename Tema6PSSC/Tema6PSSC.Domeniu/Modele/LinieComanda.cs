using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tema6PSSC.Domeniu.Modele
{
    public record LinieComanda(CodProdus CodProdus, Pret Pret, Cantitate Cantitate, Pret PretFinal)
    {
        public int IdLinieComanda { get; set; }
        public bool IsUpdated { get; set; }
    }
}
