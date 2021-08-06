using System;

namespace SEED_Encryption_Algoithm
{
    public class Subkey
    {
        public int RoundNumber { get; set; }

        public UInt32 K0 { get; set; }

        public UInt32 K1 { get; set; }
    }
}
