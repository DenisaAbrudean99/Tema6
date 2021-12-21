using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tema6PSSC.Data.Modele
{
    public class LinieComandaDto
    {
        public int IdLinieComanda { get; set; }

        public int IdProdus { get; set; }

        public decimal? Cantitate { get; set; }
        public decimal? Pret { get; set; }
        public decimal? PretFinal { get; set; }
    }
}
