using Tema6PSSC.Events;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tema6PSSC.Accomodation.EventProcessor
{
    // topicul "topic-denisa"
    // cu subscriptia "subscription-denisa" .
    //gazduieste serviciul worker
    //implementeaza interfata ihostedservice
    internal class Worker : IHostedService
    {
        private readonly IEventListener eventListener;
        
        //se injecteaza event listenerul din containerul de dependinte
        //event listenerul va primi toate event handlerele
        public Worker(IEventListener eventListener)
        {
            this.eventListener = eventListener;
        }

        //se apeleaza cand aplicatia porneste
        public Task StartAsync(CancellationToken cancellationToken)
        {

            Console.WriteLine("Worker started...");
            //topic si subscriptie
            //ascult pe acelasi topic pe care am publicat
            return eventListener.StartAsync("topic-denisa", "subscription-denisa", cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            
            Console.WriteLine("Worker stoped!");
            return eventListener.StopAsync(cancellationToken);
        }
    }
}
