using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace LoginRegisterV2
{
    /// <summary>Beverages (StockItems), stock batches (FIFO) and stock movements.</summary>
    public static class InventoryHelper
    {
        // ------------------------------------------------------------------
        // Beverages
        // ------------------------------------------------------------------

        private const string ProductColumns =
            @"i.ItemId, i.ItemName, i.Category, i.SKU, i.Quantity, i.ReorderLevel, i.UnitPrice, i.ImagePath, i.IsActive,
              CASE WHEN i.Quantity <= 0 THEN 'Out of stock'
                   WHEN i.Quantity <= i.ReorderLevel THEN 'Low stock'
                   ELSE 'In stock' END AS StockStatus";

        /// <summary>filter: "", "low", "out", "inactive" (blank = active beverages only is NOT applied; inactive are shown too).</summary>
        public static DataTable GetProducts(string search, string filter)
        {
            List<SqlParameter> ps = new List<SqlParameter>();
            string sql = "SELECT " + ProductColumns + " FROM StockItems i WHERE 1 = 1";

            if (!string.IsNullOrWhiteSpace(search))
            {
                sql += " AND (i.ItemName LIKE @like OR i.Category LIKE @like OR i.SKU LIKE @like)";
                ps.Add(Db.P("@like", "%" + EscapeLike(search.Trim()) + "%"));
            }

            if (filter == "low")
            {
                sql += " AND i.IsActive = 1 AND i.Quantity > 0 AND i.Quantity <= i.ReorderLevel";
            }
            else if (filter == "out")
            {
                sql += " AND i.IsActive = 1 AND i.Quantity <= 0";
            }
            else if (filter == "inactive")
            {
                sql += " AND i.IsActive = 0";
            }

            sql += " ORDER BY i.IsActive DESC, i.ItemName ASC";
            return Db.Query(sql, ps.ToArray());
        }

        /// <summary>Beverages that can be sold (active only), optionally filtered by a search text.</summary>
        public static DataTable GetProductsForSale(string search)
        {
            List<SqlParameter> ps = new List<SqlParameter>();
            string sql = "SELECT " + ProductColumns + " FROM StockItems i WHERE i.IsActive = 1";

            if (!string.IsNullOrWhiteSpace(search))
            {
                sql += " AND (i.ItemName LIKE @like OR i.Category LIKE @like OR i.SKU LIKE @like)";
                ps.Add(Db.P("@like", "%" + EscapeLike(search.Trim()) + "%"));
            }

            sql += " ORDER BY i.ItemName ASC";
            return Db.Query(sql, ps.ToArray());
        }

        public static DataTable GetProductsForDropdown()
        {
            return Db.Query("SELECT ItemId, ItemName + N'  (' + CAST(Quantity AS NVARCHAR(12)) + N' in stock)' AS Label FROM StockItems WHERE IsActive = 1 ORDER BY ItemName");
        }

        public static DataRow GetProduct(int itemId)
        {
            DataTable dt = Db.Query("SELECT " + ProductColumns + " FROM StockItems i WHERE i.ItemId = @id", Db.P("@id", itemId));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public static DataTable GetLowStockItems()
        {
            return Db.Query(@"SELECT ItemName, SKU, Quantity, ReorderLevel
                              FROM StockItems
                              WHERE IsActive = 1 AND Quantity <= ReorderLevel
                              ORDER BY Quantity ASC, ItemName ASC");
        }

        public static int CountLowStock()
        {
            return Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM StockItems WHERE IsActive = 1 AND Quantity <= ReorderLevel"));
        }

        public static string AddProduct(string name, string category, string sku, decimal price, int reorderLevel, int openingQty, string imagePath, string performedBy)
        {
            using (SqlConnection conn = new SqlConnection(Db.ConnStr))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        if (CountInTx(conn, tx, "SELECT COUNT(*) FROM StockItems WHERE ItemName = @v", name) > 0)
                        {
                            tx.Rollback();
                            return "A beverage named \"" + name + "\" already exists.";
                        }
                        if (!string.IsNullOrEmpty(sku) && CountInTx(conn, tx, "SELECT COUNT(*) FROM StockItems WHERE SKU = @v", sku) > 0)
                        {
                            tx.Rollback();
                            return "The SKU \"" + sku + "\" is already used by another beverage.";
                        }

                        int newId;
                        using (SqlCommand cmd = new SqlCommand(
                            @"INSERT INTO StockItems (ItemName, Category, SKU, Quantity, ReorderLevel, UnitPrice, ImagePath, IsActive, CreatedAt, UpdatedAt)
                              OUTPUT INSERTED.ItemId
                              VALUES (@name, @category, @sku, 0, @reorder, @price, @image, 1, GETDATE(), GETDATE())", conn, tx))
                        {
                            cmd.Parameters.Add(Db.P("@name", name));
                            cmd.Parameters.Add(Db.P("@category", string.IsNullOrEmpty(category) ? null : category));
                            cmd.Parameters.Add(Db.P("@sku", string.IsNullOrEmpty(sku) ? null : sku));
                            cmd.Parameters.Add(Db.P("@reorder", reorderLevel));
                            cmd.Parameters.Add(Db.P("@price", price));
                            cmd.Parameters.Add(Db.P("@image", imagePath));
                            newId = (int)cmd.ExecuteScalar();
                        }

                        if (openingQty > 0)
                        {
                            ReceiveStockTx(conn, tx, newId, openingQty, performedBy, "Opening stock when the beverage was added", null);
                        }

                        tx.Commit();
                        return null;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>Updates the details of a beverage. A null imagePath keeps the current photo.</summary>
        public static string UpdateProduct(int itemId, string name, string category, string sku, decimal price, int reorderLevel, string imagePath)
        {
            using (SqlConnection conn = new SqlConnection(Db.ConnStr))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        if (CountInTx(conn, tx, "SELECT COUNT(*) FROM StockItems WHERE ItemName = @v AND ItemId <> " + itemId, name) > 0)
                        {
                            tx.Rollback();
                            return "Another beverage is already named \"" + name + "\".";
                        }
                        if (!string.IsNullOrEmpty(sku) && CountInTx(conn, tx, "SELECT COUNT(*) FROM StockItems WHERE SKU = @v AND ItemId <> " + itemId, sku) > 0)
                        {
                            tx.Rollback();
                            return "The SKU \"" + sku + "\" is already used by another beverage.";
                        }

                        string sql = @"UPDATE StockItems
                                       SET ItemName = @name, Category = @category, SKU = @sku, UnitPrice = @price,
                                           ReorderLevel = @reorder, UpdatedAt = GETDATE()";
                        if (imagePath != null)
                        {
                            sql += ", ImagePath = @image";
                        }
                        sql += " WHERE ItemId = @id";

                        using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
                        {
                            cmd.Parameters.Add(Db.P("@name", name));
                            cmd.Parameters.Add(Db.P("@category", string.IsNullOrEmpty(category) ? null : category));
                            cmd.Parameters.Add(Db.P("@sku", string.IsNullOrEmpty(sku) ? null : sku));
                            cmd.Parameters.Add(Db.P("@price", price));
                            cmd.Parameters.Add(Db.P("@reorder", reorderLevel));
                            cmd.Parameters.Add(Db.P("@id", itemId));
                            if (imagePath != null)
                            {
                                // an empty string means "remove the photo"
                                cmd.Parameters.Add(Db.P("@image", imagePath.Length == 0 ? null : imagePath));
                            }
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return null;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public static void SetProductActive(int itemId, bool active)
        {
            Db.Execute("UPDATE StockItems SET IsActive = @a, UpdatedAt = GETDATE() WHERE ItemId = @id",
                Db.P("@a", active), Db.P("@id", itemId));
        }

        /// <summary>
        /// Deletes a beverage that has never been ordered. A beverage that already appears in orders is kept
        /// (deleting it would change past sales); it can be deactivated instead. Returns null on success.
        /// </summary>
        public static string DeleteProduct(int itemId, out string photoToRemove)
        {
            photoToRemove = null;

            using (SqlConnection conn = new SqlConnection(Db.ConnStr))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        int orderCount;
                        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM OrderItems WHERE ItemId = @id", conn, tx))
                        {
                            cmd.Parameters.Add(Db.P("@id", itemId));
                            orderCount = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        if (orderCount > 0)
                        {
                            tx.Rollback();
                            return "This beverage is part of " + orderCount + " order line(s), so it can't be deleted without changing your sales history. Use Deactivate to hide it from ordering instead.";
                        }

                        using (SqlCommand cmd = new SqlCommand("SELECT ImagePath FROM StockItems WHERE ItemId = @id", conn, tx))
                        {
                            cmd.Parameters.Add(Db.P("@id", itemId));
                            object path = cmd.ExecuteScalar();
                            photoToRemove = (path == null || path == DBNull.Value) ? null : path.ToString();
                        }

                        ExecInTx(conn, tx, "DELETE FROM StockTransactions WHERE ItemId = @id", itemId);
                        ExecInTx(conn, tx, "DELETE FROM StockBatches WHERE ItemId = @id", itemId);
                        ExecInTx(conn, tx, "DELETE FROM StockItems WHERE ItemId = @id", itemId);

                        tx.Commit();
                        return null;
                    }
                    catch
                    {
                        tx.Rollback();
                        photoToRemove = null;
                        throw;
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // Stock in / out and FIFO batches
        // ------------------------------------------------------------------

        /// <summary>Adds stock as a NEW batch (first in, first out).</summary>
        public static string ReceiveStock(int itemId, int quantity, string performedBy, string notes)
        {
            if (quantity <= 0)
            {
                return "Quantity must be at least 1.";
            }

            using (SqlConnection conn = new SqlConnection(Db.ConnStr))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        if (CountInTx(conn, tx, "SELECT COUNT(*) FROM StockItems WHERE ItemId = " + itemId, "x") == 0)
                        {
                            tx.Rollback();
                            return "That beverage no longer exists.";
                        }

                        ReceiveStockTx(conn, tx, itemId, quantity, performedBy, notes, null);
                        tx.Commit();
                        return null;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>Removes stock (damaged, expired, correction...). Takes from the oldest batch first.</summary>
        public static string RemoveStock(int itemId, int quantity, string performedBy, string reason)
        {
            if (quantity <= 0)
            {
                return "Quantity must be at least 1.";
            }

            using (SqlConnection conn = new SqlConnection(Db.ConnStr))
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        string name;
                        int available;
                        if (!ReadItemForUpdate(conn, tx, itemId, out name, out available))
                        {
                            tx.Rollback();
                            return "That beverage no longer exists.";
                        }

                        if (quantity > available)
                        {
                            tx.Rollback();
                            return "Not enough stock. " + name + " only has " + available + " in stock.";
                        }

                        DeductFifoTx(conn, tx, itemId, quantity);

                        using (SqlCommand cmd = new SqlCommand(
                            @"INSERT INTO StockTransactions (ItemId, TransactionType, Quantity, Reference, PerformedBy, TransactionDate, Notes)
                              VALUES (@item, 'OUT', @qty, 'Stock Out', @by, GETDATE(), @notes)", conn, tx))
                        {
                            cmd.Parameters.Add(Db.P("@item", itemId));
                            cmd.Parameters.Add(Db.P("@qty", quantity));
                            cmd.Parameters.Add(Db.P("@by", performedBy));
                            cmd.Parameters.Add(Db.P("@notes", string.IsNullOrWhiteSpace(reason) ? null : reason));
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return null;
                    }
                    catch (InvalidOperationException)
                    {
                        tx.Rollback();
                        return "The stock records for this beverage changed while saving. Please try again.";
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>Batches that still have stock, oldest first (the order they will be sold in).</summary>
        public static DataTable GetActiveBatches()
        {
            return Db.Query(@"SELECT b.BatchId, i.ItemName, b.ReceivedDate, b.QuantityReceived, b.QuantityRemaining, b.Notes, b.ReceivedBy
                              FROM StockBatches b
                              INNER JOIN StockItems i ON i.ItemId = b.ItemId
                              WHERE b.QuantityRemaining > 0
                              ORDER BY i.ItemName ASC, b.ReceivedDate ASC, b.BatchId ASC");
        }

        public static DataTable GetStockTransactions(int top)
        {
            return Db.Query(@"SELECT TOP (@top) t.TransactionDate, i.ItemName, t.TransactionType, t.Quantity, t.Reference, t.Notes, t.PerformedBy
                              FROM StockTransactions t
                              INNER JOIN StockItems i ON i.ItemId = t.ItemId
                              ORDER BY t.TransactionDate DESC, t.TransactionId DESC",
                Db.P("@top", top));
        }

        // ---- transaction-level building blocks (also used by OrderHelper) ----

        internal static int ReceiveStockTx(SqlConnection conn, SqlTransaction tx, int itemId, int quantity, string performedBy, string notes, string reference)
        {
            int batchId;
            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO StockBatches (ItemId, QuantityReceived, QuantityRemaining, ReceivedDate, Notes, ReceivedBy)
                  OUTPUT INSERTED.BatchId
                  VALUES (@item, @qty, @qty, GETDATE(), @notes, @by)", conn, tx))
            {
                cmd.Parameters.Add(Db.P("@item", itemId));
                cmd.Parameters.Add(Db.P("@qty", quantity));
                cmd.Parameters.Add(Db.P("@notes", string.IsNullOrWhiteSpace(notes) ? null : notes));
                cmd.Parameters.Add(Db.P("@by", performedBy));
                batchId = (int)cmd.ExecuteScalar();
            }

            using (SqlCommand cmd = new SqlCommand(
                "UPDATE StockItems SET Quantity = Quantity + @qty, UpdatedAt = GETDATE() WHERE ItemId = @item", conn, tx))
            {
                cmd.Parameters.Add(Db.P("@qty", quantity));
                cmd.Parameters.Add(Db.P("@item", itemId));
                cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO StockTransactions (ItemId, TransactionType, Quantity, Reference, PerformedBy, TransactionDate, Notes)
                  VALUES (@item, 'IN', @qty, @ref, @by, GETDATE(), @notes)", conn, tx))
            {
                cmd.Parameters.Add(Db.P("@item", itemId));
                cmd.Parameters.Add(Db.P("@qty", quantity));
                cmd.Parameters.Add(Db.P("@ref", string.IsNullOrEmpty(reference) ? "Batch #" + batchId : reference));
                cmd.Parameters.Add(Db.P("@by", performedBy));
                cmd.Parameters.Add(Db.P("@notes", string.IsNullOrWhiteSpace(notes) ? null : notes));
                cmd.ExecuteNonQuery();
            }

            return batchId;
        }

        /// <summary>Locks the beverage row and returns its name and current stock.</summary>
        internal static bool ReadItemForUpdate(SqlConnection conn, SqlTransaction tx, int itemId, out string name, out int quantity)
        {
            name = null;
            quantity = 0;

            using (SqlCommand cmd = new SqlCommand(
                "SELECT ItemName, Quantity FROM StockItems WITH (UPDLOCK, ROWLOCK) WHERE ItemId = @id", conn, tx))
            {
                cmd.Parameters.Add(Db.P("@id", itemId));
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return false;
                    }
                    name = reader.GetString(0);
                    quantity = reader.GetInt32(1);
                    return true;
                }
            }
        }

        /// <summary>
        /// Takes quantity from the oldest batches first. Returns the [batchId, quantityTaken] pairs.
        /// Throws InvalidOperationException if the batches do not hold enough (nothing is ever taken below zero).
        /// </summary>
        internal static List<int[]> DeductFifoTx(SqlConnection conn, SqlTransaction tx, int itemId, int quantity)
        {
            List<int[]> batches = new List<int[]>();
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT BatchId, QuantityRemaining
                  FROM StockBatches WITH (UPDLOCK, ROWLOCK)
                  WHERE ItemId = @item AND QuantityRemaining > 0
                  ORDER BY ReceivedDate ASC, BatchId ASC", conn, tx))
            {
                cmd.Parameters.Add(Db.P("@item", itemId));
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        batches.Add(new int[] { reader.GetInt32(0), reader.GetInt32(1) });
                    }
                }
            }

            List<int[]> taken = new List<int[]>();
            int needed = quantity;

            foreach (int[] batch in batches)
            {
                if (needed <= 0)
                {
                    break;
                }

                int take = Math.Min(batch[1], needed);

                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE StockBatches SET QuantityRemaining = QuantityRemaining - @take WHERE BatchId = @batch AND QuantityRemaining >= @take", conn, tx))
                {
                    cmd.Parameters.Add(Db.P("@take", take));
                    cmd.Parameters.Add(Db.P("@batch", batch[0]));
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException("Batch changed while selling.");
                    }
                }

                taken.Add(new int[] { batch[0], take });
                needed -= take;
            }

            if (needed > 0)
            {
                throw new InvalidOperationException("Not enough stock in the batches.");
            }

            using (SqlCommand cmd = new SqlCommand(
                "UPDATE StockItems SET Quantity = Quantity - @qty, UpdatedAt = GETDATE() WHERE ItemId = @item AND Quantity >= @qty", conn, tx))
            {
                cmd.Parameters.Add(Db.P("@qty", quantity));
                cmd.Parameters.Add(Db.P("@item", itemId));
                if (cmd.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("Stock total is lower than the quantity requested.");
                }
            }

            return taken;
        }

        // ------------------------------------------------------------------
        // Photos
        // ------------------------------------------------------------------

        public const string PlaceholderImage = "~/Content/img/beverage-placeholder.svg";

        public static string ImageUrl(object path)
        {
            string p = path as string;
            return string.IsNullOrWhiteSpace(p) ? PlaceholderImage : p;
        }

        /// <summary>
        /// Validates and saves an uploaded beverage photo. Returns an error message (or null).
        /// savedPath is null when nothing was uploaded.
        /// </summary>
        public static string SavePhoto(HttpPostedFile file, out string savedPath)
        {
            savedPath = null;

            if (file == null || file.ContentLength == 0)
            {
                return null;
            }

            if (file.ContentLength > 2 * 1024 * 1024)
            {
                return "The photo is too large. Please use an image smaller than 2 MB.";
            }

            string ext = Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif" && ext != ".webp")
            {
                return "Only JPG, PNG, GIF or WEBP photos are allowed.";
            }

            byte[] head = new byte[12];
            int read = file.InputStream.Read(head, 0, head.Length);
            file.InputStream.Position = 0;

            if (!LooksLikeImage(head, read, ext))
            {
                return "That file does not look like a valid image.";
            }

            string folder = HttpContext.Current.Server.MapPath("~/Uploads/Beverages");
            Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(folder, fileName));

            savedPath = "~/Uploads/Beverages/" + fileName;
            return null;
        }

        private static bool LooksLikeImage(byte[] head, int length, string ext)
        {
            if (length < 12)
            {
                return false;
            }

            bool jpeg = head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF;
            bool png = head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47;
            bool gif = head[0] == 0x47 && head[1] == 0x49 && head[2] == 0x46 && head[3] == 0x38;
            bool webp = head[0] == 0x52 && head[1] == 0x49 && head[2] == 0x46 && head[3] == 0x46
                        && head[8] == 0x57 && head[9] == 0x45 && head[10] == 0x42 && head[11] == 0x50;

            if (ext == ".jpg" || ext == ".jpeg") return jpeg;
            if (ext == ".png") return png;
            if (ext == ".gif") return gif;
            return webp;
        }

        /// <summary>Deletes an uploaded photo file (ignores the placeholder and anything outside Uploads).</summary>
        public static void DeletePhoto(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("~/Uploads/Beverages/", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                string full = HttpContext.Current.Server.MapPath(path);
                if (File.Exists(full))
                {
                    File.Delete(full);
                }
            }
            catch
            {
                // an undeletable photo should never break the page
            }
        }

        // ------------------------------------------------------------------
        // Small helpers
        // ------------------------------------------------------------------

        public static string EscapeLike(string text)
        {
            return text.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        }

        private static int CountInTx(SqlConnection conn, SqlTransaction tx, string sql, string value)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.Add(Db.P("@v", value));
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static void ExecInTx(SqlConnection conn, SqlTransaction tx, string sql, int id)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.Add(Db.P("@id", id));
                cmd.ExecuteNonQuery();
            }
        }
    }
}
