using LanguageExt;
using static LanguageExt.Prelude;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Tema6PSSC.Domeniu.Modele
{
    public record CodProdus
    {
        public const string Pattern = "^P[0-9]$";
        private static readonly Regex PatternRegex = new(Pattern);
        public string Value { get; }

        internal CodProdus(string value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new ExceptieCodProdusInvalid("");
            }
        }

        private static bool IsValid(string stringValue) => PatternRegex.IsMatch(stringValue);

        public override string ToString()
        {
            return Value;
        }

        public static Option<CodProdus> TryParseCodProdus(string stringValue)
        {
            if (IsValid(stringValue))
            {
                return Some<CodProdus>(new(stringValue));
            }
            else
            {
                return None;
            }
        }

    }
}
