using System;
using System.Data;

namespace LoginRegisterV2
{
    /// <summary>The date range a report covers.</summary>
    public class ReportRange
    {
        public DateTime Start;          // inclusive
        public DateTime EndExclusive;   // exclusive
        public string Label;
    }

    /// <summary>Sales reports and dashboard figures. Only Completed orders count as sales.</summary>
    public static class ReportHelper
    {
        public static ReportRange GetRange(string period, DateTime referenceDate, DateTime? customStart, DateTime? customEnd)
        {
            DateTime d = referenceDate.Date;
            ReportRange range = new ReportRange();

            switch (period)
            {
                case "Weekly":
                    int diff = ((int)d.DayOfWeek + 6) % 7; // Monday = 0
                    range.Start = d.AddDays(-diff);
                    range.EndExclusive = range.Start.AddDays(7);
                    range.Label = "Week of " + range.Start.ToString("MMM d") + " to " + range.EndExclusive.AddDays(-1).ToString("MMM d, yyyy");
                    break;
                case "Monthly":
                    range.Start = new DateTime(d.Year, d.Month, 1);
                    range.EndExclusive = range.Start.AddMonths(1);
                    range.Label = range.Start.ToString("MMMM yyyy");
                    break;
                case "Quarterly":
                    int quarter = (d.Month - 1) / 3;
                    range.Start = new DateTime(d.Year, quarter * 3 + 1, 1);
                    range.EndExclusive = range.Start.AddMonths(3);
                    range.Label = "Q" + (quarter + 1) + " " + d.Year + " (" + range.Start.ToString("MMM") + " - " + range.EndExclusive.AddDays(-1).ToString("MMM") + ")";
                    break;
                case "Yearly":
                    range.Start = new DateTime(d.Year, 1, 1);
                    range.EndExclusive = range.Start.AddYears(1);
                    range.Label = "Year " + d.Year;
                    break;
                case "Custom":
                    DateTime s = (customStart ?? d).Date;
                    DateTime e = (customEnd ?? s).Date;
                    if (e < s)
                    {
                        DateTime swap = s;
                        s = e;
                        e = swap;
                    }
                    range.Start = s;
                    range.EndExclusive = e.AddDays(1);
                    range.Label = s.ToString("MMM d, yyyy") + " to " + e.ToString("MMM d, yyyy");
                    break;
                default:
                    range.Start = d;
                    range.EndExclusive = d.AddDays(1);
                    range.Label = d.ToString("dddd, MMMM d, yyyy");
                    break;
            }

            return range;
        }

        // ---- sales ----

        public static DataRow GetSummary(DateTime start, DateTime endExclusive)
        {
            DataTable dt = Db.Query(
                @"SELECT COUNT(*) AS OrderCount,
                         ISNULL(SUM(o.GrandTotal), 0) AS TotalSales,
                         ISNULL(SUM(q.Qty), 0) AS ItemsSold
                  FROM Orders o
                  OUTER APPLY (SELECT SUM(oi.Quantity) AS Qty FROM OrderItems oi WHERE oi.OrderId = o.OrderId) q
                  WHERE o.Status = 'Completed' AND o.CompletedAt >= @s AND o.CompletedAt < @e",
                Db.P("@s", start), Db.P("@e", endExclusive));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static DataTable GetSalesByBeverage(DateTime start, DateTime endExclusive)
        {
            return Db.Query(
                @"SELECT i.ItemName, SUM(oi.Quantity) AS QuantitySold, SUM(oi.LineTotal) AS Revenue
                  FROM OrderItems oi
                  INNER JOIN Orders o ON o.OrderId = oi.OrderId
                  INNER JOIN StockItems i ON i.ItemId = oi.ItemId
                  WHERE o.Status = 'Completed' AND o.CompletedAt >= @s AND o.CompletedAt < @e
                  GROUP BY i.ItemId, i.ItemName
                  ORDER BY SUM(oi.Quantity) DESC, SUM(oi.LineTotal) DESC, i.ItemName ASC",
                Db.P("@s", start), Db.P("@e", endExclusive));
        }

        public static DataTable GetSalesByDay(DateTime start, DateTime endExclusive)
        {
            return Db.Query(
                @"SELECT CAST(o.CompletedAt AS DATE) AS SaleDate, COUNT(*) AS OrderCount, SUM(o.GrandTotal) AS Sales
                  FROM Orders o
                  WHERE o.Status = 'Completed' AND o.CompletedAt >= @s AND o.CompletedAt < @e
                  GROUP BY CAST(o.CompletedAt AS DATE)
                  ORDER BY CAST(o.CompletedAt AS DATE) DESC",
                Db.P("@s", start), Db.P("@e", endExclusive));
        }

        public static DataTable GetSalesOrders(DateTime start, DateTime endExclusive)
        {
            return Db.Query(
                @"SELECT o.OrderId, o.CompletedAt, o.CustomerName, o.GrandTotal,
                         (SELECT ISNULL(SUM(oi.Quantity), 0) FROM OrderItems oi WHERE oi.OrderId = o.OrderId) AS TotalQty,
                         " + OrderHelper.ItemsSummaryExpression + @" AS ItemsSummary
                  FROM Orders o
                  WHERE o.Status = 'Completed' AND o.CompletedAt >= @s AND o.CompletedAt < @e
                  ORDER BY o.CompletedAt DESC, o.OrderId DESC",
                Db.P("@s", start), Db.P("@e", endExclusive));
        }

        // ---- dashboard ----

        public static DataRow GetInventoryTotals()
        {
            DataTable dt = Db.Query(
                @"SELECT COUNT(*) AS ItemCount,
                         ISNULL(SUM(Quantity), 0) AS Units,
                         ISNULL(SUM(CAST(Quantity AS DECIMAL(18,2)) * UnitPrice), 0) AS StockValue
                  FROM StockItems WHERE IsActive = 1");
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }
    }
}
