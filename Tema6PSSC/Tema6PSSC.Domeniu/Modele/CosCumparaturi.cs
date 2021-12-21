using CSharp.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Tema6PSSC.Domeniu.Modele
{
    [AsChoice]
    public static partial class CosCumparaturi
    {

        public interface ICosCumparaturi { }

        public record CosGol() : ICosCumparaturi
        {

        }


        public record CosNevalidat : ICosCumparaturi
        {
            public CosNevalidat(IReadOnlyCollection<ProdusAlesNevalidat> listaProduse)
            {
                ListaProduse = listaProduse;
            }

            public IReadOnlyCollection<ProdusAlesNevalidat> ListaProduse { get; }
        }

        public record CosInvalidat : ICosCumparaturi
        {
            internal CosInvalidat(IReadOnlyCollection<ProdusAlesNevalidat> listaProduse, string motiv)
            {
                ListaProduse = listaProduse;
                Motiv = motiv;
            }

            public IReadOnlyCollection<ProdusAlesNevalidat> ListaProduse { get; }
            public string Motiv { get; }
        }


        public record CosCumparaturiEsuat : ICosCumparaturi
        {
            internal CosCumparaturiEsuat(IReadOnlyCollection<ProdusAlesNevalidat> listaProduse, Exception exception)
            {
                ListaProduse = listaProduse;
                Exception = exception;
            }

            public IReadOnlyCollection<ProdusAlesNevalidat> ListaProduse { get; }
            public Exception Exception { get; }
        }

        public record CosValidat : ICosCumparaturi
        {
            internal CosValidat(IReadOnlyCollection<ProdusAlesValidat> listaProduse)
            {
                ListaProduse = listaProduse;
            }

            public IReadOnlyCollection<ProdusAlesValidat> ListaProduse { get; }
        }

        public record PretCalculatCosCumparaturi : ICosCumparaturi
        {
            internal PretCalculatCosCumparaturi(IReadOnlyCollection<LinieComanda> listaProduse)
            {
                ListaProduse = listaProduse;
            }

            public IReadOnlyCollection<LinieComanda> ListaProduse { get; }
        }


        public record CosPlatit : ICosCumparaturi
        {
            internal CosPlatit(IReadOnlyCollection<LinieComanda> listaProduse, string csv, DateTime dataPlata)
            {
                ListaProduse = listaProduse;
                DataPlata = dataPlata;
                Csv = csv;
            }
            public IReadOnlyCollection<LinieComanda> ListaProduse { get; }
            public DateTime DataPlata { get; }
            public string Csv { get; }
        }
    }
}
