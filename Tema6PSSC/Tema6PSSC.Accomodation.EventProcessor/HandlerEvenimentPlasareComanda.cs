using Tema6PSSC.Dto.Evenimente;
using Tema6PSSC.Events;
using Tema6PSSC.Events.Modele;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tema6PSSC.Domeniu.Modele.EvenimentPlasareComanda;

namespace Tema6PSSC.Accomodation.EventProcessor
{
    //din workflow public evenimentul 
    //aici receptionez 
    internal class HandlerEvenimentPlasareComanda : AbstractEventHandler<EvenPlasareComanda>
    {
        public override string[] EventTypes => new string[] { typeof(EvenPlasareComanda).Name };

        protected override Task<EventProcessingResult> OnHandleAsync(EvenPlasareComanda eventData)
        {
            //scriu mesaj in consola pentru a sti ca am primit mesajul
            Console.WriteLine("!!!!MESAJ PRIMIT!!!! ->"+eventData.ToString());
            return Task.FromResult(EventProcessingResult.Completed);
        }
    }
}
