using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GraffitiListener
{
    public static class OrdersSynchro
    {
        public static void DoSynchro(IEnumerable<OrderStruct> mesOrders, IEnumerable<OrderStruct> graffitiOrders)
        {
            ChangesInOrders.ClearChangeList();

            if (graffitiOrders == null) return;
            foreach (var graffitiOrder in graffitiOrders)
            {
                var matchingMesOrders = mesOrders.Where(o => o.OrderNo == graffitiOrder.OrderNo);
                if (!matchingMesOrders.Any())
                {
                    CreateOrderInMes(graffitiOrder);
                    continue;
                }

                var mesOrder = matchingMesOrders.First();
                ChangesInOrders.CompareTwoOrders(graffitiOrder, mesOrder);
            }
        }



        private static void CreateOrderInMes(OrderStruct graffitiOrder)
        {
            throw new NotImplementedException();
        }
    }
}
