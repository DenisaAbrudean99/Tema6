using LanguageExt;
using System;
using static LanguageExt.Prelude;

namespace Tema6PSSC.Domeniu.Modele
{
    public record Pret
    {
        public decimal Value { get; }
        public Pret(decimal value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new ExceptiePretInvalid($"{value:0.##} este un pret invalid.");
            }
        }

        public static Pret operator *(Pret a, Cantitate b) => new Pret((a.Value * b.Value));


        public override string ToString()
        {
            return $"{Value:0.##}";
        }

        public static Option<Pret> TryParsePret(decimal pretdecimal)
        {
            if (IsValid(pretdecimal))
            {
                return Some<Pret>(new(pretdecimal));
            }
            else
            {
                return None;
            }
        }
        public static Option<Pret> TryParsePret(string pretString)
        {
            if (decimal.TryParse(pretString, out decimal pret) && IsValid(pret))
            {
                return Some<Pret>(new(pret));
            }
            else
            {
                return None;
            }
        }

        private static bool IsValid(decimal pret) => pret >= 1;
    }
}
