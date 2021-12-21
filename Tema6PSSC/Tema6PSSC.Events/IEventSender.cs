using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageExt;
namespace Tema6PSSC.Events
{
    public interface IEventSender
    {
        //interfata cu o metoda ce primeste topicul si evenimentul
        //daca apare eroare, primim rezultat de eroare
        TryAsync<Unit> SendAsync<T>(string topicName, T @event);
    }
}
