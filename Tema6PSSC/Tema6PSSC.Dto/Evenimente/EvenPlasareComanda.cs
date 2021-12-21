using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tema6PSSC.Dto.Modele;

namespace Tema6PSSC.Dto.Evenimente
{
    public record EvenPlasareComanda
    {
        //pentru interactiunea cu exteriorul, vreau sa controlez ce se trimite, sa nu trimit detalii care nu conteaza(din domeniu)
        public List<ProdusDto> LinieComanda { get; init; }
    }
}
