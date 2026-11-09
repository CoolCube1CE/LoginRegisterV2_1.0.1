using System;
using System.Globalization;
using System.Web.UI;

namespace LoginRegisterV2
{
    public partial class StockTransactions : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AccountHelper.RequireAdmin(this))
            {
                return;
            }

            if (!IsPostBack)
            {
                BindAll(null);
            }
        }

        private void BindAll(string keepSelectedItem)
        {
            try
            {
                ddlItem.DataSource = InventoryHelper.GetProductsForDropdown();
                ddlItem.DataBind();

                if (!string.IsNullOrEmpty(keepSelectedItem) && ddlItem.Items.FindByValue(keepSelectedItem) != null)
                {
                    ddlItem.SelectedValue = keepSelectedItem;
                }

                gvBatches.DataSource = InventoryHelper.GetActiveBatches();
                gvBatches.DataBind();

                gvHistory.DataSource = InventoryHelper.GetStockTransactions(200);
                gvHistory.DataBind();
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't load the stock records right now. Please refresh the page.";
            }
        }

        protected void btnRecord_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";

            string selected = ddlItem.SelectedValue;

            int itemId;
            if (!int.TryParse(selected, out itemId))
            {
                lblMessage.Text = "Please choose a beverage.";
                BindAll(null);
                return;
            }

            int quantity;
            if (!int.TryParse(txtQty.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out quantity) || quantity < 1 || quantity > 1000000)
            {
                lblMessage.Text = "Quantity must be a whole number from 1 to 1,000,000.";
                BindAll(selected);
                return;
            }

            string notes = txtNotes.Text.Trim();
            if (notes.Length > 250)
            {
                lblMessage.Text = "Notes can be up to 250 characters.";
                BindAll(selected);
                return;
            }

            try
            {
                string by = AccountHelper.CurrentEmail(Session);
                string error;

                if (ddlType.SelectedValue == "IN")
                {
                    error = InventoryHelper.ReceiveStock(itemId, quantity, by, notes);
                }
                else
                {
                    error = InventoryHelper.RemoveStock(itemId, quantity, by, notes);
                }

                if (error != null)
                {
                    lblMessage.Text = Server.HtmlEncode(error);
                }
                else
                {
                    lblSuccess.Text = ddlType.SelectedValue == "IN"
                        ? quantity + " unit(s) added as a new batch."
                        : quantity + " unit(s) removed from the oldest batch(es).";
                    txtQty.Text = "";
                    txtNotes.Text = "";
                }
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't record the movement right now. Please try again.";
            }

            BindAll(selected);
        }
    }
}
