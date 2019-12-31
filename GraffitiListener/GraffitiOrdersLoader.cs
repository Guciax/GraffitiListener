using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace GraffitiListener
{
    public static class GraffitiOrdersLoader
    {
        public static IEnumerable<OrderStruct> GetGraffitiOrdersFromDb()
        {
            string connString = @"Host=10.0.10.50;Database=mst_test;Username=postgres;Password=1QxGjnoCbYnb;Persist Security Info=True";
            string querry = "select * from ws.pokaz_zlecenia_elektr_new";
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand(querry, conn))
                {
                    NpgsqlDataReader rdr = cmd.ExecuteReader();
                    OrderStruct newOrder = new OrderStruct();
                    while (rdr.Read())
                    {
                        string orderNo = SafeGetString(rdr, "cel_zlecenia_indeks");

                        if (newOrder.OrderNo != null)
                        {
                            if (newOrder.OrderNo != orderNo)
                            {
                                yield return newOrder;
                                newOrder = new OrderStruct();
                            }
                        }
                        else
                        {
                            newOrder.OrderNo = orderNo;
                            newOrder.ShippingQty = (int)SafeGetDouble(rdr, "cel_zlecenia_indeks");
                            string plannedDateInt = SafeGetInt(rdr, "data_zamkniecia_planowana").ToString(); //YYYYMMDD
                            string year = new string(new char[] { plannedDateInt[0], plannedDateInt[1], plannedDateInt[2], plannedDateInt[3] });
                            string month = new string(new char[] { plannedDateInt[4], plannedDateInt[5] });
                            string day = new string(new char[] { plannedDateInt[6], plannedDateInt[7] });

                            int yearInt = int.Parse(year);
                            int monthInt = int.Parse(month);
                            int dayInt = int.Parse(day);

                            DateTime shippingDay = new DateTime(yearInt, monthInt, dayInt);
                            newOrder.PlannedShippingDate = shippingDay;

                            int status = SafeGetSmallInt(rdr, "status");
                        }
                        
                        string rank1 = SafeGetString(rdr, "lp_8");
                        string rank2 = SafeGetString(rdr, "lp_9");
                        string rank3 = SafeGetString(rdr, "lp_10");
                        string rank4 = SafeGetString(rdr, "lp_11");
                        string vtCode = SafeGetString(rdr, "lp_12");
                        
                        string component12Nc = SafeGetString(rdr, "material_indeks");
                        double componnetQty = SafeGetDouble(rdr, "material_ilosc_tech");

                        ComponnetStruct newComp = new ComponnetStruct
                        {
                            Nc12 = component12Nc,
                            Qty = componnetQty
                        };

                        newOrder.ListOfComponents.Add(newComp);
                        
                        if(rank1!=null || rank2 != null || rank3 != null || rank4 != null)
                        {
                            List<string> Ranks = new List<string>();
                            if (rank1 != null) Ranks.Add(rank1);
                            if (rank2 != null) Ranks.Add(rank2);
                            if (rank3 != null) Ranks.Add(rank3);
                            if (rank4 != null) Ranks.Add(rank4);
                            string rank = string.Join("-", Ranks);
                            newOrder.LedsChoosenByPlanner.Add(new MST.MES.LedRankTools.LedRankStruct
                            {
                                Collective12Nc = component12Nc,
                                Rank = rank
                            });
                        }
                    }
                    if (newOrder.OrderNo != null)
                        yield return newOrder;
                }
                
            }
        }

        private static string SafeGetString(NpgsqlDataReader rdr, string colName)
        {
            int colIdx = rdr.GetOrdinal(colName);
            if (rdr.IsDBNull(colIdx)) return "";
            return rdr.GetString(colIdx);
        }

        private static int SafeGetInt(NpgsqlDataReader rdr, string colName)
        {
            int colIdx = rdr.GetOrdinal(colName);
            if (rdr.IsDBNull(colIdx)) return -1;
            return rdr.GetInt32(colIdx);
        }

        private static int SafeGetSmallInt(NpgsqlDataReader rdr, string colName)
        {
            int colIdx = rdr.GetOrdinal(colName);
            if (rdr.IsDBNull(colIdx)) return -1;
            return rdr.GetByte(colIdx);
        }

        private static double SafeGetDouble(NpgsqlDataReader rdr, string colName)
        {
            int colIdx = rdr.GetOrdinal(colName);
            if (rdr.IsDBNull(colIdx)) return -1;
            return rdr.GetDouble(colIdx);
        }

        private static DateTime SafeGetDateTime(NpgsqlDataReader rdr, string colName)
        {
            int colIdx = rdr.GetOrdinal(colName);
            if (rdr.IsDBNull(colIdx)) return DateTime.MinValue;
            return rdr.GetDateTime(colIdx);
        }
    }
}
