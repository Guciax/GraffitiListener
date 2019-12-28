using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace GraffitiListener
{
    public static class MesOrdersLoader
    {
        public static IEnumerable<OrderStruct> LoadMesOrders()
        {
            string query = $@"SELECT Diody_LED,Nr_Zlecenia_Produkcyjnego,NC12_wyrobu,Ilosc_wyrobu_zlecona WHERE ????";
            using (SqlConnection conn = new SqlConnection(@"Data Source=MSTMS010;Initial Catalog=MES;User Id=mes;Password=mes;"))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            OrderStruct newOrder = new OrderStruct
                            {
                                LedsChoosenByPlanner = MST.MES.LedRankTools.MesDbFieldToLedStruct(SafeGetString(rdr, "Diody_LED")),
                                ModelId = SafeGetString(rdr, "NC12_wyrobu"),
                                OrderNo = SafeGetString(rdr, "Nr_Zlecenia_Produkcyjnego"),
                                ShippingQty = (int)SafeGetFloat(rdr, "Ilosc_wyrobu_zlecona"),
                                PlannedShippingDate = SafeGetDateTime(rdr, "NC12_wyrobu")
                            };
                            yield return newOrder;
                        }
                    }
                }
            }
        }

        public static string SafeGetString(SqlDataReader reader, string colName)
        {
            int colIndex = reader.GetOrdinal(colName);
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return "";
        }
        public static DateTime SafeGetDateTime(SqlDataReader reader, string colName)
        {
            int colIndex = reader.GetOrdinal(colName);
            if (!reader.IsDBNull(colIndex))
                return reader.GetDateTime(colIndex);
            return DateTime.MinValue;
        }
        public static double SafeGetFloat(SqlDataReader reader, string colName)
        {
            if (reader == null) throw new NullReferenceException();
            int colIndex = reader.GetOrdinal(colName);
            if (!reader.IsDBNull(colIndex))
            {
                var val = reader.GetDouble(colIndex);
                return val;
            }
            return -1;
        }

    }
}
