using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tema6PSSC.Data.Modele
{
    public class AntetComandaDto
    {
        public int IdComanda { get; set; }
        public string Adresa { get; set; }
        public decimal? Total { get; set; }
    }
}
