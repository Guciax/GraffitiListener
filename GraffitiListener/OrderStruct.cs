using System;
using System.Collections.Generic;
using System.Text;

namespace GraffitiListener
{
    public class OrderStruct
    {
        public string OrderNo { get; set; }
        public string ModelId { get; set; }
        public int ShippingQty { get; set; }
        public List<MST.MES.LedRankTools.LedRankStruct> LedsChoosenByPlanner { get; set; }
        public DateTime PlannedShippingDate { get; set; }
        public List<ComponnetStruct> ListOfComponents { get; set; }
    }

    public class ComponnetStruct
    {
        public string Nc12 { get; set; }
        public double Qty { get; set; }
    }

}
