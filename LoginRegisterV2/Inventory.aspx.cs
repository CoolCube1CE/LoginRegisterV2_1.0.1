
using System;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LoginRegisterV2
{
    public partial class Inventory : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AccountHelper.Require(this))
            {
                return;
            }

            if (!IsPostBack)
            {
                string filter = Request.QueryString["filter"];
                if (filter == "low" || filter == "out" || filter == "inactive")
                {
                    ddlFilter.SelectedValue = filter;
                }

                BindProducts();
            }
        }

        protected string StockBadgeClass(object status)
        {
            string text = status == null ? "" : status.ToString();

            if (text == "Out of stock")
                return "stock-badge stock-badge--out";

            if (text == "Low stock")
                return "stock-badge stock-badge--low";

            return "stock-badge stock-badge--in";
        }

        private void BindProducts()
        {
            try
            {
                DataTable products = InventoryHelper.GetProducts(
                    txtSearch.Text,
                    ddlFilter.SelectedValue
                );

                rptProducts.DataSource = products;
                rptProducts.DataBind();

                lblEmpty.Visible = products.Rows.Count == 0;

                lblCount.Text = products.Rows.Count == 1
                    ? "1 beverage"
                    : products.Rows.Count + " beverages";
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't load the beverages right now. Please refresh the page.";
            }
        }

        private void ClearMessages()
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";
        }

        // Reads and checks the shared fields; returns an error message or null
        private string ReadDetails(
            string nameText,
            string categoryText,
            string skuText,
            string priceText,
            string reorderText,
            out string name,
            out string category,
            out string sku,
            out decimal price,
            out int reorder)
        {
            name = nameText.Trim();
            category = categoryText.Trim();
            sku = skuText.Trim();
            price = 0;
            reorder = 0;

            if (name == "")
                return "Beverage name is required.";

            if (name.Length > 150)
                return "The name can be up to 150 characters.";

            if (category.Length > 100)
                return "The category can be up to 100 characters.";

            if (sku.Length > 50)
                return "The SKU can be up to 50 characters.";

            if (!decimal.TryParse(
                priceText,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out price) || price <= 0 || price > 1000000)
            {
                return "Enter a valid price greater than zero.";
            }

            if (!int.TryParse(
                reorderText,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out reorder) || reorder < 0 || reorder > 100000)
            {
                return "Reorder level must be a whole number from 0 to 100,000.";
            }

            return null;
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            ClearMessages();

            string name, category, sku;
            decimal price;
            int reorder;

            string error = ReadDetails(
                txtName.Text,
                txtCategory.Text,
                txtSku.Text,
                txtPrice.Text,
                txtReorder.Text,
                out name,
                out category,
                out sku,
                out price,
                out reorder
            );

            if (error != null)
            {
                lblMessage.Text = Server.HtmlEncode(error);
                BindProducts();
                return;
            }

            int opening;

            if (!int.TryParse(
                txtOpeningQty.Text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out opening) || opening < 0 || opening > 1000000)
            {
                lblMessage.Text = "Opening stock must be a whole number from 0 to 1,000,000.";
                BindProducts();
                return;
            }

            string savedPhoto = null;

            try
            {
                if (fuPhoto.HasFile)
                {
                    string photoError = InventoryHelper.SavePhoto(
                        fuPhoto.PostedFile,
                        out savedPhoto
                    );

                    if (photoError != null)
                    {
                        lblMessage.Text = Server.HtmlEncode(photoError);
                        BindProducts();
                        return;
                    }
                }

                error = InventoryHelper.AddProduct(
                    name,
                    category,
                    sku,
                    price,
                    reorder,
                    opening,
                    savedPhoto,
                    AccountHelper.CurrentEmail(Session)
                );

                if (error != null)
                {
                    InventoryHelper.DeletePhoto(savedPhoto);
                    lblMessage.Text = Server.HtmlEncode(error);
                    BindProducts();
                    return;
                }

                lblSuccess.Text = "\"" + Server.HtmlEncode(name) + "\" was added.";

                txtName.Text = "";
                txtCategory.Text = "";
                txtSku.Text = "";
                txtPrice.Text = "";
                txtReorder.Text = "10";
                txtOpeningQty.Text = "0";
            }
            catch (Exception ex)
            {
                InventoryHelper.DeletePhoto(savedPhoto);
                Db.LogError(ex);
                lblMessage.Text = "We couldn't save the beverage right now. Please try again.";
            }

            BindProducts();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ClearMessages();
            BindProducts();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearMessages();

            txtSearch.Text = "";
            ddlFilter.SelectedIndex = 0;

            BindProducts();
        }

        protected void rptProducts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            ClearMessages();

            int itemId;

            if (!int.TryParse(Convert.ToString(e.CommandArgument), out itemId))
            {
                return;
            }

            try
            {
                if (e.CommandName == "EditItem")
                {
                    LoadEditPanel(itemId);
                }
                else if (e.CommandName == "Toggle" ||
                         e.CommandName == "DeleteItem")
                {
                    DataRow product = InventoryHelper.GetProduct(itemId);

                    if (product == null)
                    {
                        lblMessage.Text = "That beverage no longer exists.";
                    }
                    else
                    {
                        string itemName = product["ItemName"].ToString();

                        hfConfirmItemId.Value = itemId.ToString();
                        hfConfirmAction.Value = e.CommandName;

                        lblConfirmItemName.Text = Server.HtmlEncode(itemName);

                        if (e.CommandName == "DeleteItem")
                        {
                            lblConfirmTitle.Text = "Delete Beverage";
                            lblConfirmIcon.Text = "&#128465;";
                            lblConfirmMessage.Text =
                                "Are you sure you want to permanently delete this beverage? This action cannot be undone.";

                            btnConfirmAction.Text = "Delete";
                            btnConfirmAction.CssClass = "btn btn-danger";
                        }
                        else
                        {
                            bool active = Convert.ToBoolean(product["IsActive"]);

                            lblConfirmTitle.Text = active
                                ? "Deactivate Beverage"
                                : "Activate Beverage";

                            lblConfirmIcon.Text = active
                                ? "&#128065;"
                                : "&#10003;";

                            lblConfirmMessage.Text = active
                                ? "This beverage will no longer be available for ordering. Do you want to continue?"
                                : "This beverage will become available for ordering again. Do you want to continue?";

                            btnConfirmAction.Text = active
                                ? "Deactivate"
                                : "Activate";

                            btnConfirmAction.CssClass = "btn btn-primary";
                        }

                        pnlConfirm.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "That action could not be completed. Please try again.";
            }

            BindProducts();
        }

        private void LoadEditPanel(int itemId)
        {
            DataRow product = InventoryHelper.GetProduct(itemId);

            if (product == null)
            {
                lblMessage.Text = "That beverage no longer exists.";
                return;
            }

            hfEditId.Value = itemId.ToString();

            txtEditName.Text = product["ItemName"].ToString();

            txtEditCategory.Text = product["Category"] == DBNull.Value
                ? ""
                : product["Category"].ToString();

            txtEditSku.Text = product["SKU"] == DBNull.Value
                ? ""
                : product["SKU"].ToString();

            txtEditPrice.Text = Convert.ToDecimal(product["UnitPrice"])
                .ToString("0.00", CultureInfo.InvariantCulture);

            txtEditReorder.Text = product["ReorderLevel"].ToString();

            chkRemovePhoto.Checked = false;

            imgEdit.ImageUrl = InventoryHelper.ImageUrl(
                product["ImagePath"] == DBNull.Value
                    ? null
                    : product["ImagePath"].ToString()
            );

            pnlEdit.Visible = true;
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ClearMessages();
            pnlEdit.Visible = false;
            BindProducts();
        }

        protected void btnSaveEdit_Click(object sender, EventArgs e)
        {
            ClearMessages();

            int itemId;

            if (!int.TryParse(hfEditId.Value, out itemId))
            {
                pnlEdit.Visible = false;
                return;
            }

            string name, category, sku;
            decimal price;
            int reorder;

            string error = ReadDetails(
                txtEditName.Text,
                txtEditCategory.Text,
                txtEditSku.Text,
                txtEditPrice.Text,
                txtEditReorder.Text,
                out name,
                out category,
                out sku,
                out price,
                out reorder
            );

            if (error != null)
            {
                lblMessage.Text = Server.HtmlEncode(error);
                BindProducts();
                return;
            }

            string newPhoto = null;

            try
            {
                DataRow current = InventoryHelper.GetProduct(itemId);

                if (current == null)
                {
                    lblMessage.Text = "That beverage no longer exists.";
                    pnlEdit.Visible = false;
                    BindProducts();
                    return;
                }

                string oldPhoto = current["ImagePath"] == DBNull.Value
                    ? null
                    : current["ImagePath"].ToString();

                string imageArgument = null; // null = keep current photo

                if (fuEditPhoto.HasFile)
                {
                    string photoError = InventoryHelper.SavePhoto(
                        fuEditPhoto.PostedFile,
                        out newPhoto
                    );

                    if (photoError != null)
                    {
                        lblMessage.Text = Server.HtmlEncode(photoError);
                        BindProducts();
                        return;
                    }

                    imageArgument = newPhoto;
                }
                else if (chkRemovePhoto.Checked)
                {
                    imageArgument = "";
                }

                error = InventoryHelper.UpdateProduct(
                    itemId,
                    name,
                    category,
                    sku,
                    price,
                    reorder,
                    imageArgument
                );

                if (error != null)
                {
                    InventoryHelper.DeletePhoto(newPhoto);
                    lblMessage.Text = Server.HtmlEncode(error);
                    BindProducts();
                    return;
                }

                if (imageArgument != null)
                {
                    InventoryHelper.DeletePhoto(oldPhoto);
                }

                lblSuccess.Text = "Changes saved.";
                pnlEdit.Visible = false;
            }
            catch (Exception ex)
            {
                InventoryHelper.DeletePhoto(newPhoto);
                Db.LogError(ex);
                lblMessage.Text = "We couldn't save the changes right now. Please try again.";
            }

            BindProducts();
        }

        protected void btnCancelConfirm_Click(object sender, EventArgs e)
        {
            pnlConfirm.Visible = false;
            ClearMessages();
            BindProducts();
        }

        protected void btnConfirmAction_Click(object sender, EventArgs e)
        {
            ClearMessages();

            int itemId;

            if (!int.TryParse(hfConfirmItemId.Value, out itemId))
            {
                pnlConfirm.Visible = false;
                lblMessage.Text = "Invalid beverage selection.";
                BindProducts();
                return;
            }

            string action = hfConfirmAction.Value;

            try
            {
                if (action == "DeleteItem")
                {
                    string photo;

                    string error = InventoryHelper.DeleteProduct(itemId, out photo);

                    if (error != null)
                    {
                        lblMessage.Text = Server.HtmlEncode(error);
                    }
                    else
                    {
                        InventoryHelper.DeletePhoto(photo);
                        lblSuccess.Text = "The beverage was deleted.";
                        pnlEdit.Visible = false;
                    }
                }
                else if (action == "Toggle")
                {
                    DataRow product = InventoryHelper.GetProduct(itemId);

                    if (product == null)
                    {
                        lblMessage.Text = "That beverage no longer exists.";
                    }
                    else
                    {
                        bool active = Convert.ToBoolean(product["IsActive"]);

                        InventoryHelper.SetProductActive(itemId, !active);

                        lblSuccess.Text = active
                            ? "The beverage is now hidden from ordering."
                            : "The beverage is available for ordering again.";
                    }
                }
                else
                {
                    lblMessage.Text = "Invalid confirmation action.";
                }
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text =
                    "That action could not be completed. Please try again.";
            }

            pnlConfirm.Visible = false;
            BindProducts();
        }
    }
}
