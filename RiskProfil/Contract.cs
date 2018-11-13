using System;
using System.Collections.Generic;
using System.Linq;

namespace RiskProfil
{
    public class ContractRiadok : ICloneable
    {
        public double Strike { get; set; }
        public string Podklad { get; set; }

        public double PutBuyCena { get; set; }
        public int PocetPutBuy { get; set; }
        public double PutSellCena { get; set; }
        public int PocetPutSell { get; set; }

        public double CallBuyCena { get; set; }
        public int PocetCallBuy { get; set; }
        public double CallSellCena { get; set; }
        public int PocetCallSell { get; set; }

        public bool MaObchod { get; set; } = false;
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public static class ContractRiadokExtension
    {
        public static List<ContractRiadok> ToClonedList(this IEnumerable<ContractRiadok> source)
        {
            return source.Select(employee => employee.Clone() as ContractRiadok).ToList();
        }
    }
}
