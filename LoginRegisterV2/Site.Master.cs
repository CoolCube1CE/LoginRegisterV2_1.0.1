using System;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LoginRegisterV2
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool isLoggedIn = Session["UserEmail"] != null;

            navBar.Visible = isLoggedIn;
            profileBar.Visible = isLoggedIn;

            if (!isLoggedIn)
            {
                return;
            }
            bool isAdmin = AccountHelper.IsAdmin(Session);
            lnkNavStockTransactions.Visible = isAdmin;
            lnkNavSales.Visible = isAdmin;

            string fullName = Session["FullName"] == null ? "" : Session["FullName"].ToString();
            string email = Session["UserEmail"].ToString();

            lblProfileName.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(fullName) ? email : fullName);
            lblProfileRole.Text = Server.HtmlEncode(Session["AccessType"] == null ? "User" : Session["AccessType"].ToString());

            HighlightActiveLink();
            ShowLowStockBanner();
        }

        private void ShowLowStockBanner()
        {
            try
            {
                int low = InventoryHelper.CountLowStock();
                if (low > 0)
                {
                    pnlLowStock.Visible = true;
                    lblLowStock.Text = low == 1
                        ? "1 beverage is low on stock or sold out."
                        : low + " beverages are low on stock or sold out.";
                }
            }
            catch (Exception ex)
            {
                // the banner is a convenience: never stop the page because of it
                Db.LogError(ex);
            }
        }

        private void HighlightActiveLink()
        {
            string currentPage = Path.GetFileName(Request.Path);

            MarkActive(lnkNavDashboard, currentPage, "Dashboard.aspx");
            MarkActive(lnkNavInventory, currentPage, "Inventory.aspx");
            MarkActive(lnkNavStockTransactions, currentPage, "StockTransactions.aspx");
            MarkActive(lnkNavNewOrder, currentPage, "NewOrder.aspx");
            MarkActive(lnkNavOrders, currentPage, "Orders.aspx");
            MarkActive(lnkNavSales, currentPage, "SalesReport.aspx");
        }

        private void MarkActive(HyperLink link, string currentPage, params string[] matches)
        {
            bool isActive = matches.Any(match => string.Equals(currentPage, match, StringComparison.OrdinalIgnoreCase));
            link.CssClass = isActive ? "app-nav__link is-active" : "app-nav__link";
        }

        protected void lnkChangePassword_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ChangePasswordOTP.aspx");
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }


        protected bool IsLoggedInUser
        {
            get { return Session["UserEmail"] != null; }
        }

        protected bool IsAdminUser
        {
            get { return AccountHelper.IsAdmin(Session); }
        }
    }
    }

