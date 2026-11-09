using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace LoginRegisterV2
{
    /// <summary>One line of an order that is being built (before it is saved).</summary>
    [Serializable]
    public class OrderLine
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>Orders, order lines and the stock that moves with them.</summary>
    public static class OrderHelper
    {
        private static readonly string[] ValidStatuses = { "Pending", "Completed", "Cancelled" };

        public static string OrderNumber(object orderId)
        {
            if (orderId == null || orderId == DBNull.Value)
            {
                return "";
            }
            return "ORD-" + Convert.ToInt32(orderId).ToString("D6");
        }

        private const string ItemsSummarySql =
            @"STUFF((SELECT N', ' + i2.ItemName + N' x' + CAST(oi2.Quantity AS NVARCHAR(10))
                     FROM OrderItems oi2
                     INNER JOIN StockItems i2 ON i2.ItemId = oi2.ItemId
                     WHERE oi2.OrderId = o.OrderId
                     ORDER BY i2.ItemName
                     FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, N'')";

        // ------------------------------------------------------------------
        // Create
        // ------------------------------------------------------------------

        /// <summary>
        /// Saves a whole order (all lines) in ONE transaction. Stock is checked and deducted
        /// (oldest batch first) only if every line succeeds; otherwise nothing is saved.
        /// Returns null on success, or a user-facing message.
        /// </summary>
        public static string CreateOrder(string customerName, string status, List<OrderLine> lines, string placedBy, out int orderId)
        {
            orderId = 0;

            if (status != "Pending" && status != "Completed")
            {
                status = "Completed";
            }

            if (lines == null || lines.Count == 0)
            {
                return "Add at least one beverage to the order.";
            }

            // Combine repeated beverages into a single line
            Dictionary<int, int> merged = new Dictionary<int, int>();
            foreach (OrderLine line in lines)
            {
                if (line.Quantity <= 0)
                {
                    return "Every quantity must be at least 1.";
                }

                int current;
                merged.TryGetValue(line.ItemId, out current);
                merged[line.ItemId] = current + line.Quantity;
            }

            List<int> itemIds = new List<int>(merged.Keys);
            itemIds.Sort(); // same lock order every time avoids deadlocks

            using (SqlConnection conn = new SqlConnection(Db.ConnStr))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        int newOrderId;
                        using (SqlCommand cmd = new SqlCommand(
                            @"INSERT INTO Orders (CustomerName, Status, OrderDate, PlacedBy, GrandTotal, CompletedAt)
                              OUTPUT INSERTED.OrderId
                              VALUES (@customer, @status, GETDATE(), @by, 0, CASE WHEN @status = 'Completed' THEN GETDATE() ELSE NULL END)", conn, tx))
                        {
                            cmd.Parameters.Add(Db.P("@customer", customerName));
                            cmd.Parameters.Add(Db.P("@status", status));
                            cmd.Parameters.Add(Db.P("@by", placedBy));
                            newOrderId = (int)cmd.ExecuteScalar();
                        }

                        foreach (int itemId in itemIds)
                        {
                            int quantity = merged[itemId];

                            string name = null;
                            decimal price = 0;
                            int stock = 0;
                            bool active = false;
                            bool found = false;

                            using (SqlCommand cmd = new SqlCommand(
                                "SELECT ItemName, UnitPrice, Quantity, IsActive FROM StockItems WITH (UPDLOCK, ROWLOCK) WHERE ItemId = @id", conn, tx))
                            {
                                cmd.Parameters.Add(Db.P("@id", itemId));
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        found = true;
                                        name = reader.GetString(0);
                                        price = reader.GetDecimal(1);
                                        stock = reader.GetInt32(2);
                                        active = reader.GetBoolean(3);
                                    }
                                }
                            }

                            if (!found)
                            {
                                tx.Rollback();
                                return "One of the beverages in this order no longer exists.";
                            }
                            if (!active)
                            {
                                tx.Rollback();
                                return name + " is not available for sale.";
                            }
                            if (quantity > stock)
                            {
                                tx.Rollback();
                                return "Not enough stock for " + name + ". Only " + stock + " available, but " + quantity + " were requested.";
                            }

                            int orderItemId;
                            using (SqlCommand cmd = new SqlCommand(
                                @"INSERT INTO OrderItems (OrderId, ItemId, Quantity, UnitPrice)
                                  OUTPUT INSERTED.OrderItemId
                                  VALUES (@order, @item, @qty, @price)", conn, tx))
                            {
                                cmd.Parameters.Add(Db.P("@order", newOrderId));
                                cmd.Parameters.Add(Db.P("@item", itemId));
                                cmd.Parameters.Add(Db.P("@qty", quantity));
                                cmd.Parameters.Add(Db.P("@price", price));
                                orderItemId = (int)cmd.ExecuteScalar();
                            }

                            List<int[]> taken = InventoryHelper.DeductFifoTx(conn, tx, itemId, quantity);
                            SaveAllocations(conn, tx, orderItemId, taken);

                            using (SqlCommand cmd = new SqlCommand(
                                @"INSERT INTO StockTransactions (ItemId, TransactionType, Quantity, Reference, PerformedBy, TransactionDate, Notes)
                                  VALUES (@item, 'OUT', @qty, @ref, @by, GETDATE(), 'Sold in order')", conn, tx))
                            {
                                cmd.Parameters.Add(Db.P("@item", itemId));
                                cmd.Parameters.Add(Db.P("@qty", quantity));
                                cmd.Parameters.Add(Db.P("@ref", OrderNumber(newOrderId)));
                                cmd.Parameters.Add(Db.P("@by", placedBy));
                                cmd.ExecuteNonQuery();
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "UPDATE Orders SET GrandTotal = (SELECT ISNULL(SUM(LineTotal), 0) FROM OrderItems WHERE OrderId = @id) WHERE OrderId = @id", conn, tx))
                        {
                            cmd.Parameters.Add(Db.P("@id", newOrderId));
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        orderId = newOrderId;
                        return null;
                    }
                    catch (InvalidOperationException)
                    {
                        tx.Rollback();
                        return "The stock changed while the order was being saved, so nothing was recorded. Please check the stock and try again.";
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void SaveAllocations(SqlConnection conn, SqlTransaction tx, int orderItemId, List<int[]> taken)
        {
            foreach (int[] part in taken)
            {
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO OrderItemBatches (OrderItemId, BatchId, Quantity) VALUES (@oi, @batch, @qty)", conn, tx))
                {
                    cmd.Parameters.Add(Db.P("@oi", orderItemId));
                    cmd.Parameters.Add(Db.P("@batch", part[0]));
                    cmd.Parameters.Add(Db.P("@qty", part[1]));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ------------------------------------------------------------------
        // Status changes (stock follows the status)
        // ------------------------------------------------------------------

        /// <summary>
        /// Changes the status of an order. Stock is held for an order while it is Pending or Completed:
        /// cancelling returns the stock to the batches it came from; re-opening a cancelled order takes it again.
        /// Returns null on success, or a user-facing message.
        /// </summary>
        public static string UpdateOrderStatus(int orderId, string status, string performedBy)
        {
            if (Array.IndexOf(ValidStatuses, status) < 0)
            {
                return "Invalid order status.";
            }

            using (SqlConnection conn = new SqlConnection(Db.ConnStr))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        string currentStatus;
                        using (SqlCommand cmd = new SqlCommand("SELECT Status FROM Orders WITH (UPDLOCK, ROWLOCK) WHERE OrderId = @id", conn, tx))
                        {
                            cmd.Parameters.Add(Db.P("@id", orderId));
                            object result = cmd.ExecuteScalar();
                            if (result == null)
                            {
                                tx.Rollback();
                                return "That order was not found.";
                            }
                            currentStatus = result.ToString();
                        }

                        if (currentStatus == status)
                        {
                            tx.Rollback();
                            return null;
                        }

                        bool wasCancelled = currentStatus == "Cancelled";
                        bool willBeCancelled = status == "Cancelled";

                        if (willBeCancelled != wasCancelled)
                        {
                            List<int[]> lines = new List<int[]>(); // [orderItemId, itemId, quantity]
                            using (SqlCommand cmd = new SqlCommand("SELECT OrderItemId, ItemId, Quantity FROM OrderItems WHERE OrderId = @id", conn, tx))
                            {
                                cmd.Parameters.Add(Db.P("@id", orderId));
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        lines.Add(new int[] { reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2) });
                                    }
                                }
                            }

                            if (willBeCancelled)
                            {
                                foreach (int[] line in lines)
                                {
                                    ReturnStock(conn, tx, orderId, line[0], line[1], line[2], performedBy);
                                }
                            }
                            else
                            {
                                foreach (int[] line in lines)
                                {
                                    string name;
                                    int available;
                                    if (!InventoryHelper.ReadItemForUpdate(conn, tx, line[1], out name, out available))
                                    {
                                        tx.Rollback();
                                        return "This order can't be re-opened because a beverage in it no longer exists.";
                                    }
                                    if (line[2] > available)
                                    {
                                        tx.Rollback();
                                        return "This order can't be re-opened: " + name + " only has " + available + " in stock but the order needs " + line[2] + ".";
                                    }

                                    List<int[]> taken = InventoryHelper.DeductFifoTx(conn, tx, line[1], line[2]);
                                    SaveAllocations(conn, tx, line[0], taken);

                                    using (SqlCommand cmd = new SqlCommand(
                                        @"INSERT INTO StockTransactions (ItemId, TransactionType, Quantity, Reference, PerformedBy, TransactionDate, Notes)
                                          VALUES (@item, 'OUT', @qty, @ref, @by, GETDATE(), 'Order re-opened - stock taken again')", conn, tx))
                                    {
                                        cmd.Parameters.Add(Db.P("@item", line[1]));
                                        cmd.Parameters.Add(Db.P("@qty", line[2]));
                                        cmd.Parameters.Add(Db.P("@ref", OrderNumber(orderId)));
                                        cmd.Parameters.Add(Db.P("@by", performedBy));
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        string completedPart = "";
                        if (status == "Completed")
                        {
                            completedPart = ", CompletedAt = GETDATE()";
                        }
                        else if (status == "Pending")
                        {
                            completedPart = ", CompletedAt = NULL";
                        }

                        using (SqlCommand cmd = new SqlCommand("UPDATE Orders SET Status = @status" + completedPart + " WHERE OrderId = @id", conn, tx))
                        {
                            cmd.Parameters.Add(Db.P("@status", status));
                            cmd.Parameters.Add(Db.P("@id", orderId));
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return null;
                    }
                    catch (InvalidOperationException)
                    {
                        tx.Rollback();
                        return "The stock changed while saving, so the status was not changed. Please try again.";
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // Puts a cancelled order line back into the batches it was taken from
        private static void ReturnStock(SqlConnection conn, SqlTransaction tx, int orderId, int orderItemId, int itemId, int quantity, string performedBy)
        {
            List<int[]> allocations = new List<int[]>();
            using (SqlCommand cmd = new SqlCommand("SELECT BatchId, Quantity FROM OrderItemBatches WHERE OrderItemId = @oi", conn, tx))
            {
                cmd.Parameters.Add(Db.P("@oi", orderItemId));
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        allocations.Add(new int[] { reader.GetInt32(0), reader.GetInt32(1) });
                    }
                }
            }

            if (allocations.Count == 0)
            {
                // Older order with no batch record: return the stock as a fresh batch
                InventoryHelper.ReceiveStockTx(conn, tx, itemId, quantity, performedBy, "Order cancelled - stock returned", OrderNumber(orderId));
                return;
            }

            int total = 0;
            foreach (int[] part in allocations)
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE StockBatches SET QuantityRemaining = QuantityRemaining + @qty WHERE BatchId = @batch", conn, tx))
                {
                    cmd.Parameters.Add(Db.P("@qty", part[1]));
                    cmd.Parameters.Add(Db.P("@batch", part[0]));
                    cmd.ExecuteNonQuery();
                }
                total += part[1];
            }

            using (SqlCommand cmd = new SqlCommand("UPDATE StockItems SET Quantity = Quantity + @qty, UpdatedAt = GETDATE() WHERE ItemId = @item", conn, tx))
            {
                cmd.Parameters.Add(Db.P("@qty", total));
                cmd.Parameters.Add(Db.P("@item", itemId));
                cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand("DELETE FROM OrderItemBatches WHERE OrderItemId = @oi", conn, tx))
            {
                cmd.Parameters.Add(Db.P("@oi", orderItemId));
                cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO StockTransactions (ItemId, TransactionType, Quantity, Reference, PerformedBy, TransactionDate, Notes)
                  VALUES (@item, 'IN', @qty, @ref, @by, GETDATE(), 'Order cancelled - stock returned')", conn, tx))
            {
                cmd.Parameters.Add(Db.P("@item", itemId));
                cmd.Parameters.Add(Db.P("@qty", total));
                cmd.Parameters.Add(Db.P("@ref", OrderNumber(orderId)));
                cmd.Parameters.Add(Db.P("@by", performedBy));
                cmd.ExecuteNonQuery();
            }
        }

        // ------------------------------------------------------------------
        // Reading
        // ------------------------------------------------------------------

        /// <summary>Order history (newest first, up to 300). Every filter is optional.</summary>
        public static DataTable GetOrders(string search, string status, DateTime? fromDate, DateTime? toDate)
        {
            List<SqlParameter> ps = new List<SqlParameter>();
            string sql = @"SELECT TOP 300 o.OrderId, o.CustomerName, o.Status, o.OrderDate, o.CompletedAt, o.GrandTotal, o.PlacedBy,
                                  (SELECT ISNULL(SUM(oi.Quantity), 0) FROM OrderItems oi WHERE oi.OrderId = o.OrderId) AS TotalQty,
                                  " + ItemsSummarySql + @" AS ItemsSummary
                           FROM Orders o
                           WHERE 1 = 1";

            if (!string.IsNullOrWhiteSpace(status) && Array.IndexOf(ValidStatuses, status) >= 0)
            {
                sql += " AND o.Status = @status";
                ps.Add(Db.P("@status", status));
            }

            if (fromDate.HasValue)
            {
                sql += " AND o.OrderDate >= @fromDate";
                ps.Add(Db.P("@fromDate", fromDate.Value.Date));
            }

            if (toDate.HasValue)
            {
                sql += " AND o.OrderDate < @toDate";
                ps.Add(Db.P("@toDate", toDate.Value.Date.AddDays(1)));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string text = search.Trim();
                string digits = Regex.Replace(text, @"(?i)^ord-?", "");
                int orderNumber;

                if (int.TryParse(digits, out orderNumber))
                {
                    sql += " AND (o.OrderId = @num OR o.CustomerName LIKE @like)";
                    ps.Add(Db.P("@num", orderNumber));
                }
                else
                {
                    sql += " AND o.CustomerName LIKE @like";
                }
                ps.Add(Db.P("@like", "%" + InventoryHelper.EscapeLike(text) + "%"));
            }

            sql += " ORDER BY o.OrderDate DESC, o.OrderId DESC";
            return Db.Query(sql, ps.ToArray());
        }

        public static DataRow GetOrder(int orderId)
        {
            DataTable dt = Db.Query(
                @"SELECT o.OrderId, o.CustomerName, o.Status, o.OrderDate, o.CompletedAt, o.GrandTotal, o.PlacedBy
                  FROM Orders o WHERE o.OrderId = @id", Db.P("@id", orderId));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static DataTable GetOrderLines(int orderId)
        {
            return Db.Query(
                @"SELECT i.ItemName, oi.Quantity, oi.UnitPrice, oi.LineTotal
                  FROM OrderItems oi
                  INNER JOIN StockItems i ON i.ItemId = oi.ItemId
                  WHERE oi.OrderId = @id
                  ORDER BY i.ItemName", Db.P("@id", orderId));
        }

        public static DataTable GetRecentOrders(int top)
        {
            return Db.Query(
                @"SELECT TOP (@top) o.OrderId, o.CustomerName, o.Status, o.OrderDate, o.GrandTotal,
                         " + ItemsSummarySql + @" AS ItemsSummary
                  FROM Orders o
                  ORDER BY o.OrderDate DESC, o.OrderId DESC", Db.P("@top", top));
        }

        // Used by the sales report
        internal static string ItemsSummaryExpression
        {
            get { return ItemsSummarySql; }
        }
    }
}
