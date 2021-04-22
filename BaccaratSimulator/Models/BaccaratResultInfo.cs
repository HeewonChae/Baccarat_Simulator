using BaccaratSimulator.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaccaratSimulator.Models
{
    public class BaccaratResultInfo
    {
        public int PlayerValue => PlayerCards?.Sum(elem => elem.BaccaratNumber) % 10 ?? 0;
        public int BankerValue => BankerCards?.Sum(elem => elem.BaccaratNumber) % 10 ?? 0;

        public int GameSeq { get; set; }
        public List<TrumpCardInfo> PlayerCards { get; set; }
        public List<TrumpCardInfo> BankerCards { get; set; }
        public BaccaratResultType ResultType { get; set; }
        public PatternType Big_Road { get; set; } // 원매
        public PatternType Big_Eye_Boy { get; set; } // 중국점 1군
        public PatternType Small_Road { get; set; } // 중국점 2군
        public PatternType Cockroach_Pig { get; set; } // 중국점 3군
    }
}