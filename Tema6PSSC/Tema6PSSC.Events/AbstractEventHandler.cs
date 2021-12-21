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
    //pentru a implementa cazul concret cand un event handler proceseaza un anume tip de eveniment
    public abstract class AbstractEventHandler<T> : IEventHandler where T : notnull
    {
        public abstract string[] EventTypes { get; }

        public Task<EventProcessingResult> HandleAsync(CloudEvent cloudEvent)
        {
            //de la coud event il deserializez si il convertesc la un tip concret
            T eventData = DeserializeEvent(cloudEvent);
            return OnHandleAsync(eventData);
        }

        //in loc sa apelez handle async care are un eveniment, apelez on handle async care are un tip concret
        protected abstract Task<EventProcessingResult> OnHandleAsync(T eventData);

        private T DeserializeEvent(CloudEvent cloudEvent)
        {
            if (cloudEvent.Data is not null)
            {
                var json = ((JsonElement)cloudEvent.Data).GetRawText();
                var input = JsonSerializer.Deserialize<T>(json);
                if (input is not null)
                {
                    return input;
                }
                throw new NullReferenceException($"Deserializing event generated null value. {json}");
            }
            throw new NullReferenceException("CloudEvent Data cannot be null");
        }
    }
}
