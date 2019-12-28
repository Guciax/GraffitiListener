using System;
using System.Collections.Generic;
using System.Text;

namespace GraffitiListener
{
    public static class ChangesInOrders
    {
        private static List<ChangesStruct> ChangesList { get; set; }
        public class ChangesStruct
        {
            public string OrderNo => GraffitiOrder.OrderNo;
            public int OryginalQty => MesOrder.ShippingQty;
            public int NewQty => GraffitiOrder.ShippingQty;
            public bool QuantityChanged
            {
                get { return NewQty != OryginalQty; }
            }
            public DateTime OryginalPlannedShippingDate => MesOrder.PlannedShippingDate;
            public DateTime NewPlannedShippingDate => GraffitiOrder.PlannedShippingDate;
            public bool PlannedShippingDateChanged
            {
                get { return OryginalPlannedShippingDate != NewPlannedShippingDate; }
            }
            public List<MST.MES.LedRankTools.LedRankStruct> OryginalPlannedLedDiode => MesOrder.LedsChoosenByPlanner;
            public List<MST.MES.LedRankTools.LedRankStruct> NewPlannedLedDiode => GraffitiOrder.LedsChoosenByPlanner;
            public OrderStruct GraffitiOrder { get; set; }
            public OrderStruct MesOrder { get; set; }
        }


        public static void CompareTwoOrders(OrderStruct graffitiOrder, OrderStruct mesOrder)
        {
            ChangesStruct changeData = new ChangesStruct
            {
                GraffitiOrder = graffitiOrder,
                MesOrder = mesOrder
            };
            if (changeData.QuantityChanged || changeData.PlannedShippingDateChanged)
            {
                ChangesList.Add(changeData);
            }
        }

        public static void ClearChangeList()
        {
            ChangesList = new List<ChangesStruct>();
        }

        public static void ApplyChanges()
        {
            foreach (var orderChange in ChangesList)
            {

            }
        }
    }
}
