using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace GraffitiListener
{
    public static class OrdersSynchro
    {
        public static void DoSynchro(IEnumerable<OrderStruct> mesOrders, IEnumerable<OrderStruct> graffitiOrders)
        {
            ChangesInOrders.ClearChangeList();
            if (graffitiOrders == null) return;
            string ordersCreatedMessage = "";

            foreach (var graffitiOrder in graffitiOrders)
            {
                if (!graffitiOrder.ModelId.StartsWith("1010117")) continue;
                string errorMessage = $"Zlecenie nr {graffitiOrder.OrderNoGraffiti_46YearKey} / {graffitiOrder.OrderNoGraffiti_46PrimaryKey} nie zostało wstawione do produkcji z powodu:";
                bool error = false;
                if (!graffitiOrder.ListOfComponents.Any())
                {
                    //missong components
                    errorMessage += Environment.NewLine + "Brak komponentów przypisanych do zlecenia";
                    error = true;
                }

                if (graffitiOrder.LedsChoosenByPlanner.Where(l => string.IsNullOrEmpty(l.Rank)).Any())
                {
                    //missing vtypecode
                    errorMessage += Environment.NewLine + "Brak vTypeCode diody LED";
                    error = true;
                }
                if (graffitiOrder.OrderNoGraffiti_00PrimaryKey == 0)
                {
                    //missing 00code
                    errorMessage += Environment.NewLine + "Brak zlecenia na półwyrób";
                    error = true;
                }

                var matchingMesOrders = mesOrders.Where(o => o.OrderNo == graffitiOrder.OrderNo);
                if (!matchingMesOrders.Any())
                {
                    if (error)
                    {
                        if (!MsgForOrderWasSent(graffitiOrder.MesOrderNo))
                        {
                            Mail.EmailMsg.SendEmailMessage("Zlecenie nie mogło być wstawione do produkcji.", errorMessage);
                            MarkOrderMsgSent(graffitiOrder.MesOrderNo);
                        }
                    }
                    else
                    {
                        CreateOrderInMes(graffitiOrder);
                        continue;
                    }
                }
                var mesOrder = matchingMesOrders.First();
                ChangesInOrders.CompareTwoOrders(graffitiOrder, mesOrder);
            }
        }

        private static bool MsgForOrderWasSent(string orderNo)
        {
            string fileName = @"sent.txt";
            if (!File.Exists(fileName)) {
                File.Create(fileName).Dispose();
            }

            string[] fileLines = File.ReadAllLines(fileName);
            return fileLines.Contains(orderNo);
        }
        private static void MarkOrderMsgSent(string orderNo)
        {
            string fileName = @"sent.txt";
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Dispose();
            }

            File.AppendAllText(fileName, orderNo + Environment.NewLine);
        }

        private static void CreateOrderInMes(OrderStruct graffitiOrder)
        {
            if (graffitiOrder.ListOfComponents.Count == 0) return;
            var ledQtyTotal = graffitiOrder.ListOfComponents.Where(c => c.Nc12.StartsWith("4010460") || c.Nc12.StartsWith("4010560")).Select(c => c.Qty).Sum();
            var ledPerModule = (int)Math.Round(ledQtyTotal / graffitiOrder.ShippingQty);
            if (ledPerModule < 1) return;
            throw new NotImplementedException();
            MST.MES.SqlOperations.Kitting.InsertMstLedOrderForProductionPlanner(
                graffitiOrder.OrderNoGraffiti_46PrimaryKey.ToString("0000000"),
                graffitiOrder.ModelId,
                -1,
                graffitiOrder.ShippingQty,
                DateTime.Now.Date,
                graffitiOrder.PlannedShippingDate.Date.AddHours(12),
                graffitiOrder.LedsChoosenByPlanner.Count,
                ledPerModule, "", "");
                

        }
    }
}
