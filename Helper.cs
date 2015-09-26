using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace di320fm
{
    static class Helper
    {
        public static string RandomString(int length)
        {
            if (length == 0)
                return String.Empty;

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "cannot be negative");

            var charArray = new char[length];

            //48 90

            const int lowerBound = (int)'A';
            const int upperBound = (int)'Z';

            var rnd = new Random(Guid.NewGuid().GetHashCode());

            for (var i = 0; i < charArray.Length; i++)
            {
                charArray[i] = (char) rnd.Next(lowerBound, upperBound + 1);
            }

            return new string(charArray);
        }
    }
}
