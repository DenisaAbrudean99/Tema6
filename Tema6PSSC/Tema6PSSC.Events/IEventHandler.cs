using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Tema6PSSC.Events.Modele;
namespace Tema6PSSC.Events
{
    public interface IEventHandler
    {
        //ne spune ce eveniment poate interpreta o anumita clasa
        string[] EventTypes { get; }
        //primeste un eveniment si trebuie sa il proceseze
        Task<EventProcessingResult> HandleAsync(CloudEvent cloudEvent);
    }
}
