using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Tema6PSSC.Domeniu.Modele;

namespace Tema6PSSC.Api.Modele
{
    public class InputLinieComanda
    {
        [Required]
        [RegularExpression(CodProdus.Pattern)]
        public string Cod { get; set; }

        [Required]
        [Range(1, 500)]
        public decimal Pret { get; set; }

        [Required]
        [Range(1, 100)]
        public decimal Cantitate { get; set; }
    }
}
