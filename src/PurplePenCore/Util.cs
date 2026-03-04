using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
    public class Util
    {
        // Get a bit from a uint. The bit number is interpreted mod 32.
        public static bool GetBit(uint u, int bitNumber)
        {
            return (u & (1 << (bitNumber & 0x1F))) != 0;
        }

        // Set a bit from a uint. The bit number is interpreted mod 32.
        public static uint SetBit(uint u, int bitNumber, bool newValue)
        {
            if (newValue)
                return u | (1U << (bitNumber & 0x1F));
            else
                return u & ~(1U << (bitNumber & 0x1F));
        }


    }
}
