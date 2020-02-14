using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace GraffitiListener
{
    public static class GraffitiOrdersLoader
    {
        public static List<OrderStruct> GetGraffitiOrdersFromDb()
        {
            var ordersTable = GetOrdersTable();
            var splittedByOrder = SplitTableByOrderNo(ordersTable);
            var result = SplittedOrdersDictToOrdersList(splittedByOrder).ToList();
            return result;
        }

        private static IEnumerable<OrderStruct> SplittedOrdersDictToOrdersList(Dictionary<string, DataTable> SplitTableByOrderNo)
        {
            foreach (var orderEntry in SplitTableByOrderNo)
            {
                OrderStruct newOrder = new OrderStruct();
                
                foreach (DataRow row in orderEntry.Value.Rows)
                {
                    if (row["cel_zlecenia_indeks"].ToString().EndsWith("00"))
                    {
                        newOrder.OrderNoGraffiti_00PrimaryKey = (int)row["idek"];
                        newOrder.OrderNoGraffiti_00YearKey = (int)row["nr"];

                        string compnc12 = row["material_indeks"].ToString();
                        var qty = row["material_ilosc_tech"];
                        var qtyOrder = int.Parse(row["ilosc_celu_zlecenia"].ToString());
                        newOrder.ShippingQty = qtyOrder;

                        newOrder.ListOfComponents.Add(new ComponnetStruct
                        {
                            Nc12 = compnc12,
                            Qty = (double)row["material_ilosc_tech"]
                        });

                        if (compnc12.StartsWith("4010460", System.StringComparison.CurrentCulture))
                        {
                            string vtype = row["VtCode"].ToString();

                            var rankList = MST.MES.LedRankTools.LedVtypeCodeToRankaArray(vtype);
                            foreach (var rank in rankList)
                            {
                                if(newOrder.LedsChoosenByPlanner == null)
                                {
                                    newOrder.LedsChoosenByPlanner = new List<MST.MES.LedRankTools.LedRankStruct>();
                                }
                                newOrder.LedsChoosenByPlanner.Add(new MST.MES.LedRankTools.LedRankStruct
                                {
                                    Collective12Nc = compnc12,
                                    Rank = rank
                                });
                            }
                            
                        }
                    }

                    if (row["cel_zlecenia_indeks"].ToString().EndsWith("46"))
                    {
                        newOrder.OrderNoGraffiti_46PrimaryKey = (int)row["idek"];
                        newOrder.OrderNoGraffiti_46YearKey = (int)row["nr"];
                        var ddd = row["Data realizacji"].ToString();

                        newOrder.PlannedShippingDate = DateTime.Parse(ddd);
                        newOrder.ModelId = row["cel_zlecenia_indeks"].ToString();
                    }
                }
                yield return newOrder;
            }
        }

        private static Dictionary<string, DataTable> SplitTableByOrderNo(DataTable ordersTable)
        {
            Dictionary<string, DataTable> SplittedByOrder = new Dictionary<string, DataTable>();


            foreach (DataRow row in ordersTable.Rows)
            {
                var orderNo = row["idek_zlecenia_glownego"].ToString();

                    if (!SplittedByOrder.ContainsKey(orderNo)) SplittedByOrder.Add(orderNo, ordersTable.Clone());
                    SplittedByOrder[orderNo].Rows.Add(row.ItemArray);

            }
            return SplittedByOrder;
        }
        public static DataTable GetOrdersTable()
        {
            DataTable result = new DataTable();
            using (NpgsqlConnection conn = new NpgsqlConnection(@"Host=10.0.10.50;Database=mst;Username=postgres;Password=1QxGjnoCbYnb;Persist Security Info=True;Socket Send Buffer Size=32768"))
            {
                conn.Open();
                using (Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand("Select * from ws.pokaz_zlecenia_elektr_new", conn))
                {
                    //cmd.CommandText = "schemat.nazwa funkcji"; ws.getrolka
                    NpgsqlDataAdapter adpt = new NpgsqlDataAdapter(cmd);
                    adpt.Fill(result);

                    result.Columns.Add("Data realizacji");
                    if (result.Rows.Count == 0) return result;
                    for (int r = 0; r < result.Rows.Count; r++)
                    {
                        for (int c = 0; c < result.Columns.Count; c++)
                        {
                            try
                            {
                                result.Rows[r][c] = result.Rows[r][c].ToString().Trim();
                            }
                            catch { }

                            if (c == 5)
                            {
                                int count = int.Parse(result.Rows[r][c].ToString());
                                result.Rows[r]["Data realizacji"] = new DateTime(1800, 12, 31).AddDays(count);
                            }
                        }
                    }
                    result.Columns["lp_12"].ColumnName = "VtCode";
                    result.Columns.Remove("lp_8");
                    result.Columns.Remove("lp_9");
                    result.Columns.Remove("lp_10");
                    result.Columns.Remove("lp_11");
                    result.Columns["idek_zlecenia_glownego"].SetOrdinal(0);
                    return result;
                }
            }
        }



        public static IEnumerable<OrderStruct> GetGraffitiOrdersFromDbOld()
        {
            string connString = @"Host=10.0.10.50;Database=mst;Username=postgres;Password=1QxGjnoCbYnb;Persist Security Info=True";
            string querry = "select * from ws.pokaz_zlecenia_elektr_new order by idek_zlecenia_glownego";
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
                            int plannedDateInt = SafeGetInt(rdr, "data_zamkniecia_planowana"); //YYYYMMDD
                            
                            DateTime shippingDay = new DateTime(1800, 12, 31).AddDays(plannedDateInt);
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
