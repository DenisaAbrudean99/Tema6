using Azure.Messaging.ServiceBus;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using LanguageExt;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Tema6PSSC.Events.ServiceBus
{//interfata cu o singura metoda- send async
    public class ServiceBusTopicEventSender : IEventSender, IAsyncDisposable
    {
        private readonly ServiceBusClient client;
        //pentru a refolosi sender-ul
        private readonly ConcurrentDictionary<string, ServiceBusSender> senders;
        private readonly JsonEventFormatter jsonEventFormatter = new JsonEventFormatter();

        public ServiceBusTopicEventSender(ServiceBusClient client)
        {
            this.client = client;
            senders = new ConcurrentDictionary<string, ServiceBusSender>();
        }

        public TryAsync<Unit> SendAsync<T>(string topicName, T @event) => async () =>
        {
            //obtinem senderul
            var sender = GetOrCreateSender(topicName);
            //transformam evenimentul in mesaj cu standardul cloud event
            CloudEvent cloudEvent = CreateCloudEvent<T>(topicName, @event);
            //convertesc in string, mesaj binar
            var encodedCloudEvent = jsonEventFormatter.EncodeStructuredModeMessage(cloudEvent, out var contentType);
            //mesajul encodat il trimit
            ServiceBusMessage message = new(encodedCloudEvent);
            //trimit in service bus mesajul
            await sender.SendMessageAsync(message);
            return Unit.Default;
        };

        private ServiceBusSender GetOrCreateSender(string topicName) =>
            senders.GetOrAdd(topicName, topic => client.CreateSender(topic));

        private static CloudEvent CreateCloudEvent<T>(string topicName, T eventPayload)
        {
            CloudEvent cloudEvent = new();
            cloudEvent.Id = Guid.NewGuid().ToString();
            cloudEvent.DataContentType = MediaTypeNames.Application.Json;
            cloudEvent.Data = eventPayload;
            cloudEvent.Time = DateTimeOffset.Now;
            cloudEvent.Type = typeof(T).Name;
            cloudEvent.Subject = topicName;
            cloudEvent.Source = new("https://www.upt.ro/");
            return cloudEvent;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var sender in senders.Values)
            {
                await sender.DisposeAsync();
            }
            senders.Clear();
        }
    }

}
