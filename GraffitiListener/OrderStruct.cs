using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<ComponnetStruct> ListOfComponents = new List<ComponnetStruct>();
        public int OrderNoGraffiti_46PrimaryKey { get; set; }
        public int OrderNoGraffiti_00PrimaryKey { get; set; }
        public int OrderNoGraffiti_00YearKey { get; set; }
        public int OrderNoGraffiti_46YearKey { get; set; }
        public string MesOrderNo { get; set; }
        public bool DataComplete
        {
            get
            {
                if (ListOfComponents.Count == 0) return false;
                if (LedsChoosenByPlanner.Where(l => l.Rank != "").Count() == 0) return false;
                if (OrderNoGraffiti_00PrimaryKey == null) return false;
                return true;
            }
        }
    }

    public class ComponnetStruct
    {
        public string Nc12 { get; set; }
        public double Qty { get; set; }
    }

}
