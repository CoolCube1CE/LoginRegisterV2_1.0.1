using System;
using System.Data;
using System.Web.UI;

namespace LoginRegisterV2
{
    public partial class DashboardOverview : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                LoadFigures();
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblError.Text = "Some dashboard figures could not be loaded right now. Please refresh the page.";
            }
        }

        private void LoadFigures()
        {
            DateTime today = DateTime.Today;

            // Today's sales (completed orders only)
            DataRow todayRow = ReportHelper.GetSummary(today, today.AddDays(1));
            if (todayRow != null)
            {
                lblTodaySales.Text = "₱" + Convert.ToDecimal(todayRow["TotalSales"]).ToString("N2");
                lblTodayOrders.Text = Convert.ToInt32(todayRow["OrderCount"]).ToString();
                lblTodayItems.Text = Convert.ToInt32(todayRow["ItemsSold"]) + " items sold";
            }

            // Inventory
            DataRow inv = ReportHelper.GetInventoryTotals();
            if (inv != null)
            {
                lblUnits.Text = Convert.ToInt32(inv["Units"]).ToString("N0");
                lblInventoryHint.Text = "units across " + Convert.ToInt32(inv["ItemCount"]) + " beverages";
                lblStockValue.Text = "₱" + Convert.ToDecimal(inv["StockValue"]).ToString("N2");
            }

            // Low stock
            DataTable low = InventoryHelper.GetLowStockItems();
            lblLowStockCount.Text = low.Rows.Count.ToString();
            gvLowStock.DataSource = low;
            gvLowStock.DataBind();

            // Best seller this month
            DateTime monthStart = new DateTime(today.Year, today.Month, 1);
            DataTable best = ReportHelper.GetSalesByBeverage(monthStart, monthStart.AddMonths(1));
            if (best.Rows.Count > 0)
            {
                lblBestSeller.Text = Server.HtmlEncode(best.Rows[0]["ItemName"].ToString());
                lblBestSellerHint.Text = Convert.ToInt32(best.Rows[0]["QuantitySold"]) + " sold";
            }

            gvRecentOrders.DataSource = OrderHelper.GetRecentOrders(6);
            gvRecentOrders.DataBind();

            gvRecentTransactions.DataSource = InventoryHelper.GetStockTransactions(6);
            gvRecentTransactions.DataBind();
        }
    }
}
