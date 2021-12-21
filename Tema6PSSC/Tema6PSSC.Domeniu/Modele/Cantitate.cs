using LanguageExt;
using System;
using static LanguageExt.Prelude;

namespace Tema6PSSC.Domeniu.Modele
{
    
        public record Cantitate
        {
            public decimal Value { get; }
            public Cantitate(decimal value)
            {
                if (IsValid(value))
                {
                    Value = value;
                }
                else
                {
                    throw new ExceptieCantitateInvalida($"{value:0.##} este o cantitate invalida.");
                }
            }
            public Cantitate Round()
            {
                var roundedValue = Math.Round(Value);
                return new Cantitate(roundedValue);
            }
            public override string ToString()
            {
                return $"{Value:0.##}";
            }


        public static Option<Cantitate> TryParseCantitate(decimal cantitatedecimal)
        {
            if (IsValid(cantitatedecimal))
            {
                return Some<Cantitate>(new(cantitatedecimal));
            }
            else
            {
                return None;
            }
        }

        public static Option<Cantitate> TryParseCantitate(string cantitateString)
            {
                if (decimal.TryParse(cantitateString, out decimal cantitateNumerica) && IsValid(cantitateNumerica))
                {
                    return Some<Cantitate>(new(cantitateNumerica));
                }
                else
                {
                    return None;
                }
            }

            private static bool IsValid(decimal cantitateNumerica) => cantitateNumerica >= 1 && cantitateNumerica <= 100;
        }
    }

