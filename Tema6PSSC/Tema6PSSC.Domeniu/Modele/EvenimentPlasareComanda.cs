using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharp.Choices;

namespace Tema6PSSC.Domeniu.Modele
{
    [AsChoice]

    public static partial class EvenimentPlasareComanda
    {

        public interface IEvenimentPlasareComanda { }


        public record EvenimentPlasareComandaReusita : IEvenimentPlasareComanda
        {
            public IEnumerable<ProdusPlasat> Produse { get; }

            public DateTime DataPlasare { get; }

            internal EvenimentPlasareComandaReusita(IEnumerable<ProdusPlasat> produse, DateTime dataplasare)
            {
                Produse = produse;
                DataPlasare = dataplasare;

            }
        }


        public record EvenimentPlasareComandaEsuata : IEvenimentPlasareComanda
        {
            public string Motiv { get; }

            internal EvenimentPlasareComandaEsuata(string motiv)
            {
                Motiv = motiv;
            }
        }
    }
}
