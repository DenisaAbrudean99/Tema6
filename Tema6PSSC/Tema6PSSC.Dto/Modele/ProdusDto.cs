using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tema6PSSC.Dto.Modele
{
   public record ProdusDto
    {
        
        public string CodProdus { get; init; }
        public decimal Pret { get; init; }
        public decimal Cantitate { get; init; }
        public decimal PretFinal { get; init; }
    }
}
