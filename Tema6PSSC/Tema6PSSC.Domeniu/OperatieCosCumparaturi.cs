using Tema6PSSC.Domeniu.Modele;
using static LanguageExt.Prelude;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tema6PSSC.Domeniu.Modele.CosCumparaturi;

namespace Tema6PSSC.Domeniu
{
    public static class OperatieCosCumparaturi
    {
        public static Task<ICosCumparaturi> ValidareCosCumparaturi(Func<CodProdus, Option<CodProdus>> verificareExistentaProdus, CosNevalidat cosCumparaturi) =>


            cosCumparaturi.ListaProduse
                           .Select(produsNevalidat => ValidareProdus(verificareExistentaProdus, produsNevalidat))

                           .Aggregate(CreareListaGoala().ToAsync(), ProduseValideReducere)

                           .MatchAsync(
                                Right: produseValidate => new CosValidat(produseValidate),
                                LeftAsync: errorMessage => Task.FromResult((ICosCumparaturi)new CosInvalidat(cosCumparaturi.ListaProduse, errorMessage))
                            );


        private static EitherAsync<string, ProdusAlesValidat> ValidareProdus(Func<CodProdus, Option<CodProdus>> verificareExistentaProdus, ProdusAlesNevalidat produsnevalid) =>

            from codprodus in CodProdus.TryParseCodProdus(produsnevalid.CodProdus)
                                            .ToEitherAsync($"Cod produs invalid({produsnevalid.CodProdus}")

            from cantitate in Cantitate.TryParseCantitate(produsnevalid.Cantitate)
                                       .ToEitherAsync($"Cantitate invalida la produsul cu codul: {produsnevalid.CodProdus}")
            from pret in Pret.TryParsePret(produsnevalid.Pret)
                             .ToEitherAsync($"Pret invalid la produsul cu codul: {produsnevalid.CodProdus}")

            from existentaprodus in verificareExistentaProdus(codprodus)
                                            .ToEitherAsync($"Produsul {codprodus.Value} nu exista.")
      
            select new ProdusAlesValidat(codprodus, cantitate, pret);

        private static Either<string, List<ProdusAlesValidat>> CreareListaGoala() =>
         Right(new List<ProdusAlesValidat>());


        private static EitherAsync<string, List<ProdusAlesValidat>> ProduseValideReducere(EitherAsync<string, List<ProdusAlesValidat>> acc, EitherAsync<string, ProdusAlesValidat> urmatorul) =>
          from lista in acc
          from urmatorulProdus in urmatorul
          select lista.AdaugProdusValid(urmatorulProdus);

        private static List<ProdusAlesValidat> AdaugProdusValid(this List<ProdusAlesValidat> lista, ProdusAlesValidat produsValid)
        {
            lista.Add(produsValid);
            return lista;
        }



        public static ICosCumparaturi CalcularePretProduse(ICosCumparaturi produse) =>
            produse.Match(
                     whenCosGol: cosGol => cosGol,
                     whenCosNevalidat: cosNevalidat => cosNevalidat,
                     whenCosInvalidat: cosInvalidat => cosInvalidat,
                     whenPretCalculatCosCumparaturi: cosCalculat => cosCalculat,
                     whenCosCumparaturiEsuat: cosEsuat => cosEsuat,
                     whenCosPlatit: cosPlatit => cosPlatit,
                     whenCosValidat: CalcularePretProdus
      );

        private static ICosCumparaturi CalcularePretProdus(CosValidat cosValidat) =>
            new PretCalculatCosCumparaturi(cosValidat.ListaProduse
                                                    .Select(PretFINAL)
                                                    .ToList()
                                                    .AsReadOnly());

        private static LinieComanda PretFINAL(ProdusAlesValidat produsValid) =>
            new LinieComanda(produsValid.CodProdus,
                                  produsValid.Pret,
                                  produsValid.Cantitate,
                                  produsValid.Pret * produsValid.Cantitate);


        public static ICosCumparaturi Merge(ICosCumparaturi coscumparaturi, IEnumerable<LinieComanda> produseexistente) =>
            coscumparaturi.Match(
                whenCosGol: cosGol => cosGol,
               whenCosPlatit: cosPlatit => cosPlatit,
               whenCosNevalidat: cosNevalidat => cosNevalidat,
               whenCosInvalidat: cosInvalidat => cosInvalidat,
               whenCosValidat: CosValidat => CosValidat,
               whenCosCumparaturiEsuat: cosEsuat => cosEsuat,
               whenPretCalculatCosCumparaturi: pretCos => Merge(pretCos.ListaProduse, produseexistente)
         );
        private static PretCalculatCosCumparaturi Merge(IEnumerable<LinieComanda> listanoua, IEnumerable<LinieComanda> listaxistenta)
        {
            //verific ce note exista in baza de date, pornesc de la produsele noi, daca exista pun id-ul si il marchez pt upadate
            var produseActualizateSiNoi = listanoua.Select(produs => produs with { IdLinieComanda = listaxistenta.FirstOrDefault(p => p.CodProdus == produs.CodProdus)?.IdLinieComanda ?? 0, IsUpdated = true });
            //vad care produse sunt vechi
            var produseVechi = listaxistenta.Where(produs => !listanoua.Any(p => p.CodProdus == produs.CodProdus));
            //le combin cu Union
            var toateProdusele = produseActualizateSiNoi.Union(produseVechi)
                                               .ToList()
                                               .AsReadOnly();
            return new PretCalculatCosCumparaturi(toateProdusele);
        }
        public static ICosCumparaturi Plasare(ICosCumparaturi cos) =>
            cos.Match(
               whenCosGol: cosGol => cosGol,
               whenCosPlatit: cosPlatit => cosPlatit,
               whenCosNevalidat: cosNevalidat => cosNevalidat,
               whenCosInvalidat: cosInvalidat => cosInvalidat,
               whenCosValidat: CosValidat => CosValidat,
               whenCosCumparaturiEsuat: cosEsuat => cosEsuat,
               whenPretCalculatCosCumparaturi: GenerareExport
              );

        private static ICosCumparaturi GenerareExport(PretCalculatCosCumparaturi cosfinal) =>
            new CosPlatit(cosfinal.ListaProduse,
                          cosfinal.ListaProduse.Aggregate(new StringBuilder(), CreateCsvLine).ToString(),
                          DateTime.Now);

        private static StringBuilder CreateCsvLine(StringBuilder export, LinieComanda produs) =>
            export.AppendLine($"{produs.CodProdus.Value}, {produs.Cantitate}, {produs.Pret}, {produs.PretFinal}");

    }
}
