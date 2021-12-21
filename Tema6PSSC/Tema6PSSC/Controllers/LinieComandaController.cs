using Tema6PSSC.Domeniu;
using Tema6PSSC.Domeniu.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using Tema6PSSC.Api.Modele;
using Tema6PSSC.Domeniu.Modele;

namespace Tema6PSSC.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //aici implementam actiunile
    //controller-ul identifica o resursa(liniile de comanda)
    public class LinieComandaController : ControllerBase
    {
        private ILogger<LinieComandaController> logger;

        public LinieComandaController(ILogger<LinieComandaController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ObtinereLiniiComanda([FromServices] IRepositoryLinieComanda liniecomandaRepository) =>
           //accesam notele din baza de date(try async- facem match sa vedem daca a fost cu succes sau nu)
            await liniecomandaRepository.ObtinereLiniiComandaExistente().Match(
               Succ: GetAllLinesHandleSuccess,
               Fail: GetAllLinesHandleError
            );

            private ObjectResult GetAllLinesHandleError(Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return base.StatusCode(StatusCodes.Status500InternalServerError, "UnexpectedError");
        }

        private OkObjectResult GetAllLinesHandleSuccess(List<Tema6PSSC.Domeniu.Modele.LinieComanda> liniicomanda) =>
        Ok(liniicomanda.Select(liniecomanda => new
        {
            CodProdus = liniecomanda.CodProdus.Value,
            liniecomanda.Pret,
            liniecomanda.Cantitate,
            liniecomanda.PretFinal
            
        }));

        [HttpPost]
        //primim liniile din body, un serviciu- instanta workflow-ului, se uita la constructor, vede ca are 3 dependinte, le cauta si ele trebuie inregistrate
        public async Task<IActionResult> PublicareLiniiComanda([FromServices] PlasareProdusWorkFlow plasareProdusWorkFlow, [FromBody] InputLinieComanda[] inputliniicomanda)
        {
            //intrarile si iesirile din sistem nu trb sa contina modelele din domeniu
            //vreau sa controlez foarte bine ce vine si pleaca din exterior
            //trebuie mapat intre cele 2
            var liniinevalidate = inputliniicomanda.Select(MapInputLineToUnvalidatedLine)
                                          .ToList()
                                          .AsReadOnly();
            ComandaPlasareProduse comanda = new(liniinevalidate);
            var result = await plasareProdusWorkFlow.ExecuteAsync(comanda);
            //rezultatul workflow-ului
            return result.Match<IActionResult>(
                whenEvenimentPlasareComandaEsuata: failedEvent => StatusCode(StatusCodes.Status500InternalServerError, failedEvent.Motiv),
                whenEvenimentPlasareComandaReusita: successEvent => Ok()
            );
        }
            private static ProdusAlesNevalidat MapInputLineToUnvalidatedLine(InputLinieComanda liniecomanda) => new ProdusAlesNevalidat(
            CodProdus: liniecomanda.Cod,
            Cantitate: liniecomanda.Cantitate,
            Pret: liniecomanda.Pret);
        }
    }

