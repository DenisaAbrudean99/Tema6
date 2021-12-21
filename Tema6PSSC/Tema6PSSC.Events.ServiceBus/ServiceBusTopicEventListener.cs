using Azure.Messaging.ServiceBus;
using CloudNative.CloudEvents.SystemTextJson;
using Tema6PSSC.Events.Modele;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tema6PSSC.Events.ServiceBus
{
    public class ServiceBusTopicEventListener : IEventListener
    {
     
        private ServiceBusProcessor? processor;
        private readonly ServiceBusClient client;
        private readonly Dictionary<string, IEventHandler> eventHandlers;
        private readonly ILogger<ServiceBusTopicEventListener> logger;
        private readonly JsonEventFormatter formatter = new();

        //servicebusclient-pt a ne conecta la service bus, logger- scriem mesaje, lista de eventhandler- are o clasa handleasync si proceseaza evenimentul
        //creaza un dictionar pe baza listei de event handler si cand primeste mesaj va apela handler-ul corect
        public ServiceBusTopicEventListener(ServiceBusClient client, ILogger<ServiceBusTopicEventListener> logger, IEnumerable<IEventHandler> eventHandlers)
        {
            this.client = client;
            this.eventHandlers = eventHandlers.SelectMany(handler => handler.EventTypes
                                                                            .Select(eventType => (eventType, handler)))
                                                                            .ToDictionary(pair => pair.eventType, pair => pair.handler);
            this.logger = logger;
        }

        public Task StartAsync(string topicName, string subscriptionName, CancellationToken cancellationToken)
        {
          
            var options = new ServiceBusProcessorOptions
            {
                //nu vreau sa fac autocomplete la mesaje, vreau sa controlez eu
                AutoCompleteMessages = false,

                // cate thread-uri concurente folosesc, logica de procesare va executa concurent intr-un singur proces
                MaxConcurrentCalls = 2
            };

            //imi creez un procesator care stie subscriptia si topicul la care se conecteaza si are nevoie de niste optiuni
            processor = client.CreateProcessor(topicName, subscriptionName, options);
            //inregistrez call back-uri- cand este inregistrat un mesaj sau cand apare eroareeroare
            processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
            processor.ProcessErrorAsync += Processor_ProcessErrorAsync;
            //se incheie partea de start
            return processor.StartProcessingAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //apelez stopul de la procesator
            await processor!.StopProcessingAsync(cancellationToken);
            //sterg call back-urile de la cele 2 actiuni
            processor.ProcessMessageAsync -= Processor_ProcessMessageAsync;
            processor.ProcessErrorAsync -= Processor_ProcessErrorAsync;
        }

        private Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            //scriu in logger eroarea
            logger.LogError(arg.Exception, $"{arg.ErrorSource}, {arg.FullyQualifiedNamespace}, {arg.EntityPath}");
            return Task.CompletedTask;
        }

        private async Task Processor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            //apeleaza un handler in functie de tipul mesajului
            //pentru a  nu intra in bucla infinita, ma asigur ca nu procesez de mai mult de 5 ori un mesaj, ca sa nu blocheze coada

            if (await EnsureMaxDeliveryCountAsync(arg))
            {
                await ProcessMessageAsCloudEventAsync(arg);
            }
        }

        private async Task<bool> EnsureMaxDeliveryCountAsync(ProcessMessageEventArgs arg)
        {
            bool canContinue = true;
            if (arg.Message.DeliveryCount > 5)
            {
                //daca e mai mare ca 5, pun mesajul intr-o coada separata si nu il mai procesez
                logger.LogError($"Retry count exceeded {arg.Message.MessageId}");
                await arg.DeadLetterMessageAsync(arg.Message, "Retry count exeeded");
                canContinue = false;
            }
            return canContinue;
        }

        private async Task ProcessMessageAsCloudEventAsync(ProcessMessageEventArgs arg)
        {
            //extragem mesajul
            //deserializam mesajul
            var data = arg.Message.Body;
            var cloudEvent = formatter.DecodeStructuredModeMessage(data.ToStream(), null, null);
            //pe baza tipului obtinem un handler corespunzator ( in dictionar)
            if (eventHandlers.TryGetValue(cloudEvent.Type!, out var handler))
            {
                //dupa ce l-am gasit invoc acel handler si primesc un rezultat
                var result = await InvokeHandlerAsync(cloudEvent, handler);
                await InterpretResult(result, arg);
            }
            else
            {
                logger.LogError($"No handler found for {cloudEvent.Type}");
            }
        }

        private async Task<EventProcessingResult> InvokeHandlerAsync(CloudNative.CloudEvents.CloudEvent cloudEvent, IEventHandler handler)
        {
            // in caz ca se arunca o exceptie
            try
            {
                return await handler.HandleAsync(cloudEvent);
            }
            catch (Exception ex)
            {
                //unexpected error
                logger.LogError(ex, ex.Message);
                return EventProcessingResult.Failed;
            }
        }

        private Task InterpretResult(EventProcessingResult result, ProcessMessageEventArgs arg) => result switch
        {

            //verifica daca mesajul este compelted, mesajul e procesat si il stergem din coada 
            EventProcessingResult.Completed => HandleProcessSuccessAsync(arg),
            //daca avem mesaj de retry, mesajul va ramane in coada si pot sa il reprocesez
            
            
           EventProcessingResult.Retry => HandleProcessRetryAsync(arg),
           //daca apare o eroare din care nu mai pot sa revin, el va fi mutat in deadletter si nu va mai fi procesat
            _ => HandleProcessErrorAsync(arg)
        };

        private Task HandleProcessErrorAsync(ProcessMessageEventArgs arg)
        {
            logger.LogError($"Event processing has failed {arg.Message.MessageId}");
            return arg.DeadLetterMessageAsync(arg.Message, "Processing of event has failed");
        }

        private Task HandleProcessRetryAsync(ProcessMessageEventArgs arg)
        {
            logger.LogWarning($"Event processing indicated retry {arg.Message.MessageId}");
            return arg.AbandonMessageAsync(arg.Message);
        }

        private Task HandleProcessSuccessAsync(ProcessMessageEventArgs arg)
        {
            logger.LogInformation($"Event processing has succedded {arg.Message.MessageId}");
            return arg.CompleteMessageAsync(arg.Message);
        }
    }
}
