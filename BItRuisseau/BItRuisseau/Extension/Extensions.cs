using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BItRuisseau.Extensions
{
        public static class Extensions
        {
            public static string Shorten(this string text, int max)
            {
                if (string.IsNullOrEmpty(text))
                    return text;

                return text.Length <= max
                    ? text
                    : text.Substring(0, max) + "...";
            }
        }
}
