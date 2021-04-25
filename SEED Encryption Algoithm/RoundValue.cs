using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEED_Encryption_Algoithm
{
    public class RoundValue
    {
        public int roundNumber { get; set; }

        public UInt32 Ki0 { get; set; }

        public UInt32 Ki1 { get; set; }

        public UInt32 L0 { get; set; }

        public UInt32 L1 { get; set; }

        public UInt32 R0 { get; set; }

        public UInt32 R1 { get; set; }
    }
}
