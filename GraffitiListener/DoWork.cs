﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraffitiListener
{
    public static class DoWork
    {
        private static IEnumerable<OrderStruct> MesOrders { get; set; }
        private static IEnumerable<OrderStruct> GraffitiOrders { get; set; }
        public static void SyncOrders()
        {
            //List<Task> tasksList = new List<Task>();
            //tasksList.Add(Task.Run(() => GetMesOrders()));
            //tasksList.Add(Task.Run(() => GetGraffitiOrders()));
            var g = GraffitiOrdersLoader.GetGraffitiOrdersFromDb().ToList();
            var mg = MesOrdersLoader.LoadMesOrders().ToList();
            //await Task.WhenAll(tasksList).ConfigureAwait(false);
            Synchronize();
        }

        private static void Synchronize()
        {
            OrdersSynchro.DoSynchro(MesOrders, GraffitiOrders);
        }

        private static void GetGraffitiOrders()
        {
            GraffitiOrders = GraffitiOrdersLoader.GetGraffitiOrdersFromDb();
        }

        private static void GetMesOrders()
        {
            MesOrders = MesOrdersLoader.LoadMesOrders();
        }
    }
}
