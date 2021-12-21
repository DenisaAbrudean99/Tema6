using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageExt;
using Tema6PSSC.Domeniu.Modele;
using static Tema6PSSC.Domeniu.Modele.EvenimentPlasareComanda;
using static Tema6PSSC.Domeniu.OperatieCosCumparaturi;
using static Tema6PSSC.Domeniu.Modele.CosCumparaturi;
using Tema6PSSC.Domeniu.Repositories;
using static LanguageExt.Prelude;
using Microsoft.Extensions.Logging;
using Tema6PSSC.Events;
using Tema6PSSC.Dto.Evenimente;
using Tema6PSSC.Dto.Modele;
namespace Tema6PSSC.Domeniu
{
    public class PlasareProdusWorkFlow
    {
        private readonly IRepositoryProduse repositoryProduse;
        private readonly IRepositoryLinieComanda repositoryLinieComanda;

        private readonly ILogger<PlasareProdusWorkFlow> logger;
        private readonly IEventSender eventSender;

        public PlasareProdusWorkFlow(IRepositoryProduse repositoryProduse, IRepositoryLinieComanda repositoryLinieComanda, ILogger<PlasareProdusWorkFlow> logger, IEventSender eventSender)
        {

            this.repositoryProduse = repositoryProduse;
            this.repositoryLinieComanda = repositoryLinieComanda;
            this.logger = logger;
            this.eventSender = eventSender;
        }

        public async Task<IEvenimentPlasareComanda> ExecuteAsync(ComandaPlasareProduse comanda)
        {

            CosGol cosgol = new CosGol();
            CosNevalidat cosnevalidat = new CosNevalidat(comanda.ProdusInitial);
            //aducem produsele si liniile de comanda din baza de date
            //convertesc la either pentru ca vreau sa combin toate datele, sa fie la fel
            var rezultat = from produse in repositoryProduse.ObtinereProduseExistente(cosnevalidat.ListaProduse.Select(produs => produs.CodProdus))
                            //cos cumparaturi esuat- se genereaza o exceptie
                           .ToEither(ex => new CosCumparaturiEsuat(cosnevalidat.ListaProduse, ex) as ICosCumparaturi)
                           from liniiExistente in repositoryLinieComanda.ObtinereLiniiComandaExistente()
                                                    .ToEither(ex => new CosCumparaturiEsuat(cosnevalidat.ListaProduse, ex) as ICosCumparaturi)
                           let verificareExistentaProdus = (Func<CodProdus, Option<CodProdus>>)(produs => VerificareExistentaProdus(produse, produs))

                           from produsePlatite in ExecutaWorkflowAsync(cosnevalidat, liniiExistente, verificareExistentaProdus).ToAsync()
                           //salvam in baza de date
                           from salvareRezultate in repositoryLinieComanda.SalvareLinieComanda(produsePlatite)
                                        .ToEither(ex => new CosCumparaturiEsuat(cosnevalidat.ListaProduse, ex) as ICosCumparaturi)

                            //pentru a publica evenimentul- trimitem o notificare sa spunem ca s-au publicat liniile
                           let liniecomanda = produsePlatite.ListaProduse.Select(produs =>
                           new ProdusPlasat(
                                                          produs.CodProdus,
                                                          Pret: produs.Pret,
                                                          Cantitate: produs.Cantitate,
                                                          PretFinal: produs.PretFinal))

                           let evenimentCuSucces = new EvenimentPlasareComandaReusita(liniecomanda, produsePlatite.DataPlata)

                           let publicareEveniment = new EvenPlasareComanda()
                           {
                               LinieComanda = liniecomanda.Select(p => new ProdusDto()
                               {
                                    
                                    CodProdus = p.CodProdus.Value,
                                    Pret =p.Pret.Value,
                                    Cantitate =p.Cantitate.Value,
                                    PretFinal =p.PretFinal.Value
                               }).ToList()
                           }
                           from rezultatPublicareEveniment in eventSender.SendAsync("topic-denisa", publicareEveniment )
                                               .ToEither(ex => new CosCumparaturiEsuat(cosnevalidat.ListaProduse, ex) as ICosCumparaturi)

                           select evenimentCuSucces;
          //in functie de rezultat, raspundem la api cu un mesaj
          
            return await rezultat.Match(
                 Left: produse => GenerareEvenimentEsuat(produse) as IEvenimentPlasareComanda,
                 Right: produsePlatite => produsePlatite
             );

        }

        private async Task<Either<ICosCumparaturi, CosPlatit>> ExecutaWorkflowAsync(CosNevalidat cosNevalidat,
                                                                                     IEnumerable<LinieComanda> linieExistenta,
                                                                                      Func<CodProdus, Option<CodProdus>> verificareExistentaProdus
                                                                                         )
        {

            ICosCumparaturi produse = await ValidareCosCumparaturi(verificareExistentaProdus, cosNevalidat);


            produse = CalcularePretProduse(produse);
            produse = Merge(produse, linieExistenta);
            produse = Plasare(produse);



            return produse.Match<Either<ICosCumparaturi, CosPlatit>>(

                 whenCosGol: cosGol => Left(cosGol as ICosCumparaturi),
                 whenCosNevalidat: rezultatNevalid => Left(rezultatNevalid as ICosCumparaturi),
                 whenCosInvalidat: rezultatInvalid => Left(rezultatInvalid as ICosCumparaturi),
                 whenCosValidat: rezultatValid => Left(rezultatValid as ICosCumparaturi),
                 whenCosCumparaturiEsuat: cosEsuat => Left(cosEsuat as ICosCumparaturi),
                 whenPretCalculatCosCumparaturi: pretcalculatCos => Left(pretcalculatCos as ICosCumparaturi),
                 whenCosPlatit: rezultatPlatit => Right(rezultatPlatit)
            );
        }


        private Option<CodProdus> VerificareExistentaProdus(IEnumerable<CodProdus> produse, CodProdus codProdus)
        {
            //any ne spune daca exista in baza de date un produs cu codul respectiv
            if (produse.Any(p => p == codProdus))
            {
                return Some(codProdus);
            }
            else
            {
                return None;
            }
        }



          //se uita ce tipuri de eroare avem
        private EvenimentPlasareComandaEsuata GenerareEvenimentEsuat(ICosCumparaturi produse) =>
          produse.Match<EvenimentPlasareComandaEsuata>(
                 whenCosGol: cosGol => new($"Invalid state {nameof(CosGol)}"),
                 whenCosNevalidat: rezultatNevalid => new($"Invalid state {nameof(CosNevalidat)}"),
                 whenCosInvalidat: rezultatInvalid => new(rezultatInvalid.Motiv),
                 whenCosValidat: rezultatValid => new($"Invalid state {nameof(CosValidat)}"),
                 whenCosCumparaturiEsuat: cosEsuat =>
                 {
                     logger.LogError(cosEsuat.Exception, cosEsuat.Exception.Message);
                     return new(cosEsuat.Exception.Message);
                 },
                 whenPretCalculatCosCumparaturi: pretcalculatCos => new($"Invalid state {nameof(PretCalculatCosCumparaturi)}"),
                 whenCosPlatit: rezultatPlatit => new($"Invalid state {nameof(CosPlatit)}"));
    }
}
