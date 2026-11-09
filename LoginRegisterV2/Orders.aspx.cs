
using System;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LoginRegisterV2
{
    public partial class Orders : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AccountHelper.Require(this))
            {
                return;
            }

            if (!IsPostBack)
            {
                BindOrders();

                int orderId;

                if (int.TryParse(Request.QueryString["order"], out orderId))
                {
                    ShowDetails(orderId);
                }
            }
        }

        private static DateTime? ParseDate(string text)
        {
            DateTime value;

            if (DateTime.TryParseExact(
                (text ?? "").Trim(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out value))
            {
                return value;
            }

            return null;
        }

        private void BindOrders()
        {
            try
            {
                DataTable orders = OrderHelper.GetOrders(
                    txtSearch.Text,
                    ddlStatus.SelectedValue,
                    ParseDate(txtFrom.Text),
                    ParseDate(txtTo.Text)
                );

                gvOrders.DataSource = orders;
                gvOrders.DataBind();

                lblCount.Text = orders.Rows.Count == 1
                    ? "1 order"
                    : orders.Rows.Count + " orders" +
                      (orders.Rows.Count >= 300
                          ? " (showing the latest 300)"
                          : "");
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't load the orders right now. Please refresh the page.";
            }
        }

        private void ClearMessages()
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            ClearMessages();
            BindOrders();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ClearMessages();

            txtSearch.Text = "";
            ddlStatus.SelectedIndex = 0;
            txtFrom.Text = "";
            txtTo.Text = "";

            BindOrders();
        }

        protected void gvOrders_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ViewOrder")
            {
                return;
            }

            ClearMessages();

            int orderId;

            if (int.TryParse(Convert.ToString(e.CommandArgument), out orderId))
            {
                ShowDetails(orderId);
            }
        }

        private void ShowDetails(int orderId)
        {
            try
            {
                DataRow order = OrderHelper.GetOrder(orderId);

                if (order == null)
                {
                    lblMessage.Text = "That order was not found.";
                    pnlDetails.Visible = false;
                    return;
                }

                hfOrderId.Value = orderId.ToString();

                lblDetailTitle.Text =
                    OrderHelper.OrderNumber(orderId) +
                    " - " +
                    Server.HtmlEncode(order["CustomerName"].ToString());

                string info =
                    "Placed " +
                    Convert.ToDateTime(order["OrderDate"])
                        .ToString("MMM d, yyyy h:mm tt");

                if (order["PlacedBy"] != DBNull.Value)
                {
                    info += " by " +
                        Server.HtmlEncode(order["PlacedBy"].ToString());
                }

                if (order["CompletedAt"] != DBNull.Value)
                {
                    info += " | Completed " +
                        Convert.ToDateTime(order["CompletedAt"])
                            .ToString("MMM d, yyyy h:mm tt");
                }

                lblDetailInfo.Text = info;

                gvLines.DataSource = OrderHelper.GetOrderLines(orderId);
                gvLines.DataBind();

                lblDetailTotal.Text =
                    "&#8369;" +
                    Convert.ToDecimal(order["GrandTotal"]).ToString("N2");

                ddlDetailStatus.SelectedValue =
                    order["Status"].ToString();

                pnlDetails.Visible = true;
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't open that order right now. Please try again.";
                pnlDetails.Visible = false;
            }
        }

        protected void btnCloseDetails_Click(object sender, EventArgs e)
        {
            ClearMessages();
            pnlDetails.Visible = false;
        }

        protected void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            ClearMessages();

            int orderId;

            if (!int.TryParse(hfOrderId.Value, out orderId))
            {
                pnlDetails.Visible = false;
                return;
            }

            try
            {
                string error = OrderHelper.UpdateOrderStatus(
                    orderId,
                    ddlDetailStatus.SelectedValue,
                    AccountHelper.CurrentEmail(Session)
                );

                if (error != null)
                {
                    lblMessage.Text = Server.HtmlEncode(error);
                }
                else
                {
                    lblSuccess.Text = "Order status updated.";
                }
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't update the order right now. Please try again.";
            }

            BindOrders();
            ShowDetails(orderId);
        }
    }
}
