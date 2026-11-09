using System;
using System.Data;
using System.Globalization;
using System.Web.UI;

namespace LoginRegisterV2
{
    public partial class SalesReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AccountHelper.RequireAdmin(this))
            {
                return;
            }

            if (!IsPostBack)
            {
                txtRefDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                txtStart.Text = DateTime.Today.AddDays(-6).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                txtEnd.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                LoadReport();
            }
        }

        private static DateTime? ParseDate(string text)
        {
            DateTime value;
            if (DateTime.TryParseExact((text ?? "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
            {
                return value;
            }
            return null;
        }

        private void ShowRelevantInputs()
        {
            bool custom = ddlPeriod.SelectedValue == "Custom";
            pnlCustom.Visible = custom;
            pnlRefDate.Visible = !custom;
        }

        protected void ddlPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowRelevantInputs();
            LoadReport();
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        protected void btnToday_Click(object sender, EventArgs e)
        {
            txtRefDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            txtStart.Text = DateTime.Today.AddDays(-6).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            txtEnd.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            LoadReport();
        }

        private void LoadReport()
        {
            lblMessage.Text = "";
            ShowRelevantInputs();

            string period = ddlPeriod.SelectedValue;
            DateTime reference = ParseDate(txtRefDate.Text) ?? DateTime.Today;
            DateTime? customStart = ParseDate(txtStart.Text);
            DateTime? customEnd = ParseDate(txtEnd.Text);

            if (period == "Custom")
            {
                if (!customStart.HasValue || !customEnd.HasValue)
                {
                    lblMessage.Text = "Please choose both a start date and an end date.";
                    return;
                }
                if (customEnd.Value < customStart.Value)
                {
                    lblMessage.Text = "The end date must be on or after the start date.";
                    return;
                }
            }
            else if (!ParseDate(txtRefDate.Text).HasValue)
            {
                lblMessage.Text = "Please pick a valid date.";
                return;
            }

            try
            {
                ReportRange range = ReportHelper.GetRange(period, reference, customStart, customEnd);
                lblRange.Text = Server.HtmlEncode(range.Label);

                decimal totalSales = 0;
                int orders = 0;
                int items = 0;

                DataRow summary = ReportHelper.GetSummary(range.Start, range.EndExclusive);
                if (summary != null)
                {
                    totalSales = Convert.ToDecimal(summary["TotalSales"]);
                    orders = Convert.ToInt32(summary["OrderCount"]);
                    items = Convert.ToInt32(summary["ItemsSold"]);
                }

                lblTotalSales.Text = "&#8369;" + totalSales.ToString("N2");
                lblTotalOrders.Text = orders.ToString("N0");
                lblItemsSold.Text = items.ToString("N0");

                DataTable byBeverage = ReportHelper.GetSalesByBeverage(range.Start, range.EndExclusive);
                gvByBeverage.DataSource = byBeverage;
                gvByBeverage.DataBind();

                if (byBeverage.Rows.Count > 0)
                {
                    lblBestSeller.Text = Server.HtmlEncode(byBeverage.Rows[0]["ItemName"].ToString());
                    lblBestSellerHint.Text = Convert.ToInt32(byBeverage.Rows[0]["QuantitySold"]) + " sold &middot; &#8369;" +
                        Convert.ToDecimal(byBeverage.Rows[0]["Revenue"]).ToString("N2");
                }
                else
                {
                    lblBestSeller.Text = "-";
                    lblBestSellerHint.Text = "No sales in this period";
                }

                // A per-day table only makes sense when the period is longer than one day
                bool multiDay = (range.EndExclusive - range.Start).TotalDays > 1;
                pnlDaily.Visible = multiDay;
                if (multiDay)
                {
                    gvByDay.DataSource = ReportHelper.GetSalesByDay(range.Start, range.EndExclusive);
                    gvByDay.DataBind();
                }

                gvOrders.DataSource = ReportHelper.GetSalesOrders(range.Start, range.EndExclusive);
                gvOrders.DataBind();
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't build the report right now. Please try again.";
            }
        }
    }
}
