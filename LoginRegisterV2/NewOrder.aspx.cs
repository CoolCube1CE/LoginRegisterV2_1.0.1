using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LoginRegisterV2
{
    public partial class NewOrder : System.Web.UI.Page
    {
        // The order being built lives in the session until it is saved or cleared
        private List<OrderLine> Cart
        {
            get
            {
                List<OrderLine> cart = Session["Cart"] as List<OrderLine>;
                if (cart == null)
                {
                    cart = new List<OrderLine>();
                    Session["Cart"] = cart;
                }
                return cart;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AccountHelper.Require(this))
            {
                return;
            }

            if (!IsPostBack)
            {
                // A fresh one-time token per page view stops the same order being saved twice
                string token = Guid.NewGuid().ToString("N");
                hfToken.Value = token;
                Session["OrderToken"] = token;

                ShowPlacedMessage();
                BindAll();
            }
        }

        private void ShowPlacedMessage()
        {
            int placedId;
            if (!int.TryParse(Request.QueryString["placed"], out placedId))
            {
                return;
            }

            try
            {
                DataRow order = OrderHelper.GetOrder(placedId);
                if (order != null)
                {
                    lblSuccess.Text = "Order " + OrderHelper.OrderNumber(placedId) + " was saved. Total: &#8369;" +
                        Convert.ToDecimal(order["GrandTotal"]).ToString("N2") +
                        " &middot; <a href=\"Orders.aspx?order=" + placedId + "\">View order</a>";
                }
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
            }
        }

        protected string StockBadgeClass(object status)
        {
            string text = status == null ? "" : status.ToString();
            if (text == "Out of stock") return "stock-badge stock-badge--out";
            if (text == "Low stock") return "stock-badge stock-badge--low";
            return "stock-badge stock-badge--in";
        }

        private void BindAll()
        {
            try
            {
                DataTable products = InventoryHelper.GetProductsForSale(txtSearch.Text);
                rptProducts.DataSource = products;
                rptProducts.DataBind();
                lblNoProducts.Visible = products.Rows.Count == 0;

                BindCart();
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't load the beverages right now. Please refresh the page.";
            }
        }

        // Shows the cart with the CURRENT prices from the database
        private void BindCart()
        {
            List<OrderLine> cart = Cart;

            DataTable view = new DataTable();
            view.Columns.Add("ItemId", typeof(int));
            view.Columns.Add("ItemName", typeof(string));
            view.Columns.Add("UnitPrice", typeof(decimal));
            view.Columns.Add("Qty", typeof(int));
            view.Columns.Add("LineTotal", typeof(decimal));

            decimal total = 0;

            if (cart.Count > 0)
            {
                string ids = string.Join(",", cart.Select(l => l.ItemId.ToString(CultureInfo.InvariantCulture)).ToArray());
                DataTable items = Db.Query("SELECT ItemId, ItemName, UnitPrice FROM StockItems WHERE ItemId IN (" + ids + ")");

                foreach (OrderLine line in cart)
                {
                    DataRow[] match = items.Select("ItemId = " + line.ItemId);
                    if (match.Length == 0)
                    {
                        continue;
                    }

                    decimal price = Convert.ToDecimal(match[0]["UnitPrice"]);
                    decimal lineTotal = price * line.Quantity;
                    total += lineTotal;
                    view.Rows.Add(line.ItemId, match[0]["ItemName"].ToString(), price, line.Quantity, lineTotal);
                }
            }

            rptCart.DataSource = view;
            rptCart.DataBind();

            lblCartEmpty.Visible = view.Rows.Count == 0;
            rptCart.Visible = view.Rows.Count > 0;
            lblTotal.Text = "&#8369;" + total.ToString("N2");
            btnPlace.Enabled = view.Rows.Count > 0;
            btnClearCart.Enabled = view.Rows.Count > 0;
        }

        private void ClearMessages()
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ClearMessages();
            BindAll();
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            ClearMessages();
            txtSearch.Text = "";
            BindAll();
        }

        private int CartQuantity(int itemId)
        {
            OrderLine line = Cart.FirstOrDefault(l => l.ItemId == itemId);
            return line == null ? 0 : line.Quantity;
        }

        protected void rptProducts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            ClearMessages();

            if (e.CommandName != "Add")
            {
                return;
            }

            int itemId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out itemId))
            {
                return;
            }

            TextBox qtyBox = (TextBox)e.Item.FindControl("txtQty");
            int quantity;
            if (qtyBox == null || !int.TryParse(qtyBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out quantity) || quantity < 1 || quantity > 10000)
            {
                lblMessage.Text = "Enter a quantity of at least 1.";
                BindAll();
                return;
            }

            try
            {
                DataRow product = InventoryHelper.GetProduct(itemId);
                if (product == null || !Convert.ToBoolean(product["IsActive"]))
                {
                    lblMessage.Text = "That beverage is not available.";
                }
                else
                {
                    int stock = Convert.ToInt32(product["Quantity"]);
                    int already = CartQuantity(itemId);

                    if (already + quantity > stock)
                    {
                        lblMessage.Text = "Only " + stock + " of " + Server.HtmlEncode(product["ItemName"].ToString()) + " available" +
                            (already > 0 ? " (" + already + " already in this order)." : ".");
                    }
                    else
                    {
                        OrderLine line = Cart.FirstOrDefault(l => l.ItemId == itemId);
                        if (line == null)
                        {
                            Cart.Add(new OrderLine { ItemId = itemId, Quantity = quantity });
                        }
                        else
                        {
                            line.Quantity += quantity;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't add that beverage right now. Please try again.";
            }

            BindAll();
        }

        protected void rptCart_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            ClearMessages();

            int itemId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out itemId))
            {
                return;
            }

            OrderLine line = Cart.FirstOrDefault(l => l.ItemId == itemId);
            if (line == null)
            {
                BindAll();
                return;
            }

            try
            {
                if (e.CommandName == "Remove")
                {
                    Cart.Remove(line);
                }
                else if (e.CommandName == "Dec")
                {
                    line.Quantity--;
                    if (line.Quantity <= 0)
                    {
                        Cart.Remove(line);
                    }
                }
                else if (e.CommandName == "Inc")
                {
                    DataRow product = InventoryHelper.GetProduct(itemId);
                    int stock = product == null ? 0 : Convert.ToInt32(product["Quantity"]);

                    if (line.Quantity + 1 > stock)
                    {
                        lblMessage.Text = "No more stock available for this beverage.";
                    }
                    else
                    {
                        line.Quantity++;
                    }
                }
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't update the order right now. Please try again.";
            }

            BindAll();
        }

        protected void btnClearCart_Click(object sender, EventArgs e)
        {
            ClearMessages();
            Cart.Clear();
            BindAll();
        }

        protected void btnPlace_Click(object sender, EventArgs e)
        {
            ClearMessages();

            // One-time token: a refresh or double click cannot save the same order twice
            string sessionToken = Session["OrderToken"] as string;
            if (string.IsNullOrEmpty(sessionToken) || sessionToken != hfToken.Value)
            {
                lblMessage.Text = "This order was already submitted or the page expired. Please check Orders, then reload this page to start a new order.";
                BindAll();
                return;
            }

            List<OrderLine> lines = Cart.Select(l => new OrderLine { ItemId = l.ItemId, Quantity = l.Quantity }).ToList();
            if (lines.Count == 0)
            {
                lblMessage.Text = "Add at least one beverage to the order.";
                BindAll();
                return;
            }

            string customer = txtCustomer.Text.Trim();
            if (customer.Length > 150)
            {
                lblMessage.Text = "The customer name can be up to 150 characters.";
                BindAll();
                return;
            }
            if (customer == "")
            {
                customer = "Walk-in customer";
            }

            try
            {
                int orderId;
                string error = OrderHelper.CreateOrder(customer, ddlStatus.SelectedValue, lines, AccountHelper.CurrentEmail(Session), out orderId);

                if (error != null)
                {
                    lblMessage.Text = Server.HtmlEncode(error);
                    BindAll();
                    return;
                }

                Cart.Clear();
                Session.Remove("OrderToken");
                Response.Redirect("NewOrder.aspx?placed=" + orderId);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw; // normal behaviour of Response.Redirect
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't save the order. Nothing was changed. Please try again.";
                BindAll();
            }
        }
    }
}
