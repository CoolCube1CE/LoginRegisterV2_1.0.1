<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Inventory.aspx.cs" Inherits="LoginRegisterV2.Inventory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-shell">

        <div class="dashboard-header">
            <div class="page-title-row">
                <span class="page-title-icon" aria-hidden="true">
                    <svg class="ico"><use href="#i-box" /></svg>
                </span>
                <div>
                    <h2>Beverages &amp; Inventory</h2>
                    <p class="dashboard-user-line">
                        View beverage details, prices, and available stock.
                        Update inventory through stock transactions.
                    </p>
                </div>
            </div>
        </div>

        <asp:Label ID="lblSuccess" runat="server" CssClass="alert alert-success" />
        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-error" />
    
<%-- DELETE / ACTIVATE / DEACTIVATE CONFIRMATION MODAL --%>

<asp:HiddenField ID="hfConfirmItemId" runat="server" />
<asp:HiddenField ID="hfConfirmAction" runat="server" />

<asp:Panel ID="pnlConfirm" runat="server"
    Visible="false"
    CssClass="confirm-overlay"
    role="dialog"
    aria-modal="true">

    <div class="confirm-modal">

        <div class="confirm-modal-header">
            <asp:Label ID="lblConfirmTitle" runat="server"
                Text="Confirm Action" />

            <button type="button"
                class="confirm-close"
                onclick="document.getElementById('<%= btnCancelConfirm.ClientID %>').click();"
                aria-label="Close">&times;</button>
        </div>

        <div class="confirm-modal-body">

            <div class="confirm-icon">
                <asp:Label ID="lblConfirmIcon" runat="server" />
            </div>

            <div class="confirm-details">
                <strong>
                    <asp:Label ID="lblConfirmItemName" runat="server" />
                </strong>

                <p>
                    <asp:Label ID="lblConfirmMessage" runat="server" />
                </p>
            </div>

        </div>

        <div class="confirm-modal-footer">

            <asp:Button ID="btnCancelConfirm" runat="server"
                Text="Cancel"
                CssClass="btn btn-outline"
                CausesValidation="false"
                OnClick="btnCancelConfirm_Click" />

            <asp:Button ID="btnConfirmAction" runat="server"
                Text="Confirm"
                CssClass="btn btn-danger"
                CausesValidation="false"
                OnClick="btnConfirmAction_Click" />

        </div>

    </div>
</asp:Panel>


        <%-- ADD BEVERAGE --%>
        <div class="card card--compact mb-sm">
            <h3 class="mt-0">Add a Beverage</h3>

            <div class="form-grid">
                <div class="form-group">
                    <label class="form-label" for="<%= txtName.ClientID %>">Name</label>
                    <asp:TextBox ID="txtName" runat="server"
                        CssClass="form-control" MaxLength="150" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtCategory.ClientID %>">Category</label>
                    <asp:TextBox ID="txtCategory" runat="server"
                        CssClass="form-control" MaxLength="100" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtSku.ClientID %>">SKU</label>
                    <asp:TextBox ID="txtSku" runat="server"
                        CssClass="form-control" MaxLength="50" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtPrice.ClientID %>">Price (&#8369;)</label>
                    <asp:TextBox ID="txtPrice" runat="server"
                        CssClass="form-control" TextMode="Number"
                        step="0.01" min="0" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtReorder.ClientID %>">Reorder level</label>
                    <asp:TextBox ID="txtReorder" runat="server"
                        CssClass="form-control" TextMode="Number"
                        min="0" Text="10" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtOpeningQty.ClientID %>">Opening stock</label>
                    <asp:TextBox ID="txtOpeningQty" runat="server"
                        CssClass="form-control" TextMode="Number"
                        min="0" Text="0" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= fuPhoto.ClientID %>">
                        Photo (JPG, PNG, GIF, WEBP up to 2 MB)
                    </label>
                    <asp:FileUpload ID="fuPhoto" runat="server"
                        CssClass="form-control" accept="image/*" />
                </div>
            </div>

            <asp:Button ID="btnAdd" runat="server"
                Text="Add beverage"
                CssClass="btn btn-primary"
                OnClick="btnAdd_Click" />
        </div>

        <%-- EDIT ITEM MODAL --%>
        <asp:Panel ID="pnlEdit" runat="server"
            Visible="false"
            CssClass="inventory-modal-overlay"
            role="dialog"
            aria-modal="true"
            aria-labelledby="editModalTitle">

            <div class="inventory-modal inventory-modal--edit">

                <%-- MODAL HEADER --%>
                <div class="inventory-modal__header">
                    <div class="inventory-modal__heading">
                        <span class="inventory-modal__heading-icon" aria-hidden="true">
                            <svg class="ico"><use href="#i-edit" /></svg>
                        </span>
                        <h3 id="editModalTitle">Edit Item</h3>
                    </div>

                    <button type="button"
                        class="inventory-modal__close"
                        onclick="document.getElementById('<%= btnCancelEdit.ClientID %>').click();"
                        aria-label="Close edit dialog">
                        &times;
                    </button>
                </div>

                <%-- MODAL BODY --%>
                <div class="inventory-modal__content">

                    <asp:HiddenField ID="hfEditId" runat="server" />

                    <div class="edit-modal-grid">

                        <%-- PRODUCT IMAGE --%>
                        <div class="edit-modal-photo-column">

                            <div class="edit-modal-photo-frame">
                                <asp:Image ID="imgEdit" runat="server"
                                    CssClass="edit-modal-photo"
                                    AlternateText="Current beverage photo" />
                            </div>

                            <label class="form-label"
                                for="<%= fuEditPhoto.ClientID %>">
                                Change product image
                            </label>

                            <asp:FileUpload ID="fuEditPhoto" runat="server"
                                CssClass="form-control"
                                accept=".jpg,.jpeg,.png,.gif,.webp,image/jpeg,image/png,image/gif,image/webp" />

                            <p class="edit-modal-hint">
                                JPG, PNG, GIF or WEBP. Maximum 2 MB.
                            </p>

                            <label class="checkbox-row edit-modal-remove-photo">
                                <asp:CheckBox ID="chkRemovePhoto" runat="server" />
                                Remove current photo
                            </label>

                        </div>

                        <%-- PRODUCT DETAILS --%>
                        <div class="edit-modal-fields">

                            <div class="form-group">
                                <label class="form-label"
                                    for="<%= txtEditName.ClientID %>">
                                    Product Name <span class="required-mark">*</span>
                                </label>

                                <asp:TextBox ID="txtEditName" runat="server"
                                    CssClass="form-control" MaxLength="150" />
                            </div>

                            <div class="form-group">
                                <label class="form-label"
                                    for="<%= txtEditCategory.ClientID %>">
                                    Category
                                </label>

                                <asp:TextBox ID="txtEditCategory" runat="server"
                                    CssClass="form-control" MaxLength="100" />
                            </div>

                            <div class="form-group">
                                <label class="form-label"
                                    for="<%= txtEditSku.ClientID %>">
                                    SKU
                                </label>

                                <asp:TextBox ID="txtEditSku" runat="server"
                                    CssClass="form-control" MaxLength="50" />
                            </div>

                            <div class="edit-modal-two-columns">

                                <div class="form-group">
                                    <label class="form-label"
                                        for="<%= txtEditPrice.ClientID %>">
                                        Price (&#8369;) <span class="required-mark">*</span>
                                    </label>

                                    <asp:TextBox ID="txtEditPrice" runat="server"
                                        CssClass="form-control"
                                        TextMode="Number" step="0.01" min="0" />
                                </div>

                                <div class="form-group">
                                    <label class="form-label"
                                        for="<%= txtEditReorder.ClientID %>">
                                        Reorder Level <span class="required-mark">*</span>
                                    </label>

                                    <asp:TextBox ID="txtEditReorder" runat="server"
                                        CssClass="form-control"
                                        TextMode="Number" min="0" />
                                </div>

                            </div>

                            <div class="edit-modal-note">
                                Current stock quantity is managed through
                                <strong>Stock Transactions</strong>
                                to preserve your inventory history.
                            </div>

                        </div>
                    </div>
                </div>

                <%-- MODAL FOOTER --%>
                <div class="inventory-modal__footer">

                    <asp:Button ID="btnCancelEdit" runat="server"
                        Text="Cancel"
                        CssClass="btn btn-outline"
                        OnClick="btnCancelEdit_Click"
                        CausesValidation="false" />

                    <asp:Button ID="btnSaveEdit" runat="server"
                        Text="Update Item"
                        CssClass="btn btn-primary edit-modal-save"
                        OnClick="btnSaveEdit_Click"
                        CausesValidation="false" />

                </div>

            </div>
        </asp:Panel>

        <%-- MODAL DESIGN --%>
        <style>
            .inventory-modal-overlay {
                position: fixed;
                inset: 0;
                z-index: 10000;
                display: flex;
                align-items: center;
                justify-content: center;
                padding: 24px;
                background: rgba(12,29,52,.42);
                backdrop-filter: blur(3px);
                -webkit-backdrop-filter: blur(3px);
                overflow-y: auto;
            }

            .inventory-modal {
                width: min(850px, 100%);
                max-height: calc(100vh - 48px);
                background: var(--surface,#fff);
                color: var(--text,#142b49);
                border-radius: 16px;
                box-shadow: 0 24px 70px rgba(10,28,50,.28);
                overflow: hidden;
                display: flex;
                flex-direction: column;
                animation: inventoryModalIn .18s ease-out;
            }

            .inventory-modal__header {
                min-height: 66px;
                padding: 16px 24px;
                background: #12365d;
                color: #fff;
                display: flex;
                align-items: center;
                justify-content: space-between;
            }

            .inventory-modal__heading {
                display: flex;
                align-items: center;
                gap: 13px;
            }

            .inventory-modal__heading h3 {
                margin: 0;
                color: #fff;
                font-size: 21px;
                font-weight: 700;
            }

            .inventory-modal__heading-icon {
                width: 34px;
                height: 34px;
                display: grid;
                place-items: center;
            }

            .inventory-modal__heading-icon .ico {
                width: 25px;
                height: 25px;
            }

            .inventory-modal__close {
                border: 0;
                background: transparent;
                color: #fff;
                text-decoration: none;
                font-size: 30px;
                line-height: 1;
                padding: 0 4px;
                opacity: .9;
                cursor: pointer;
            }

            .inventory-modal__close:hover {
                opacity: 1;
                color: #fff;
            }

            .inventory-modal__content {
                padding: 26px;
                overflow-y: auto;
            }

            .edit-modal-grid {
                display: grid;
                grid-template-columns: minmax(210px,.82fr) minmax(0,1.5fr);
                gap: 26px;
                align-items: start;
            }

            .edit-modal-photo-frame {
                height: 225px;
                border: 1px solid #dce6f2;
                border-radius: 11px;
                background: #edf4fc;
                display: flex;
                align-items: center;
                justify-content: center;
                overflow: hidden;
                margin-bottom: 14px;
            }

            .edit-modal-photo {
                width: 100%;
                height: 100%;
                object-fit: contain;
            }

            .edit-modal-hint {
                font-size: 12px;
                color: #71839a;
                margin: 7px 0 14px;
            }

            .edit-modal-remove-photo {
                margin-top: 12px;
            }

            .edit-modal-fields {
                display: grid;
                gap: 16px;
            }

            .edit-modal-fields .form-group {
                margin: 0;
                min-width: 0;
            }

            .edit-modal-two-columns {
                display: grid;
                grid-template-columns: 1fr 1fr;
                gap: 14px;
            }

            .required-mark {
                color: #dc3545;
            }

            .edit-modal-note {
                padding: 12px 14px;
                border-radius: 9px;
                background: #eef5fd;
                color: #49617d;
                font-size: 12px;
                line-height: 1.5;
            }

            .inventory-modal__footer {
                display: flex;
                justify-content: flex-end;
                gap: 12px;
                padding: 17px 26px;
                border-top: 1px solid #e5ebf2;
                background: var(--surface,#fff);
            }

            .edit-modal-save {
                min-width: 145px;
            }

            @keyframes inventoryModalIn {
                from {
                    opacity: 0;
                    transform: translateY(10px) scale(.99);
                }
                to {
                    opacity: 1;
                    transform: translateY(0) scale(1);
                }
            }

            @media(max-width:650px) {
                .inventory-modal-overlay {
                    padding: 12px;
                    align-items: flex-start;
                }

                .inventory-modal {
                    max-height: calc(100vh - 24px);
                    border-radius: 12px;
                }

                .inventory-modal__content {
                    padding: 18px;
                }

                .edit-modal-grid {
                    grid-template-columns: 1fr;
                    gap: 20px;
                }

                .edit-modal-photo-frame {
                    height: 175px;
                }

                .edit-modal-two-columns {
                    grid-template-columns: 1fr 1fr;
                }

                .inventory-modal__header {
                    padding: 14px 18px;
                }

                .inventory-modal__footer {
                    padding: 14px 18px;
                }
            }

            @media(max-width:380px) {
                .edit-modal-two-columns {
                    grid-template-columns: 1fr;
                }

                .inventory-modal__footer {
                    flex-direction: column-reverse;
                }

                .inventory-modal__footer .btn {
                    width: 100%;
                }
            }

            /* Confirmation Modal */
.confirm-overlay {
    position: fixed;
    inset: 0;
    z-index: 11000;
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 16px;
    background: rgba(15, 30, 50, 0.40);
    backdrop-filter: blur(2px);
}

.confirm-modal {
    width: 100%;
    max-width: 420px;
    background: #ffffff;
    border-radius: 12px;
    box-shadow: 0 12px 40px rgba(0,0,0,.18);
    overflow: hidden;
    animation: confirmAppear .15s ease-out;
}

.confirm-modal-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 17px 20px;
    color: #173557;
    font-size: 17px;
    font-weight: 700;
    border-bottom: 1px solid #edf0f4;
}

.confirm-close {
    border: none;
    background: transparent;
    font-size: 25px;
    color: #718096;
    cursor: pointer;
}

.confirm-modal-body {
    display: flex;
    gap: 14px;
    align-items: flex-start;
    padding: 22px 20px;
}

.confirm-icon {
    width: 42px;
    height: 42px;
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 50%;
    background: #fff0f0;
    color: #dc3545;
    font-size: 21px;
}

.confirm-details strong {
    display: block;
    color: #173557;
    font-size: 14px;
    margin-bottom: 5px;
}

.confirm-details p {
    margin: 0;
    color: #718096;
    font-size: 13px;
    line-height: 1.5;
}

.confirm-modal-footer {
    display: flex;
    justify-content: flex-end;
    gap: 10px;
    padding: 14px 20px;
    border-top: 1px solid #edf0f4;
}

@keyframes confirmAppear {
    from {
        opacity: 0;
        transform: translateY(5px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

@media(max-width:480px) {
    .confirm-modal-footer {
        flex-direction: column-reverse;
    }

    .confirm-modal-footer .btn {
        width: 100%;
    }
}
        </style>

        <%-- SEARCH / FILTER --%>
        <div class="filter-bar">

            <div class="filter-bar__field filter-bar__field--grow">
                <label class="form-label" for="<%= txtSearch.ClientID %>">Search</label>

                <asp:TextBox ID="txtSearch" runat="server"
                    CssClass="form-control"
                    placeholder="Name, category or SKU"
                    MaxLength="100" />
            </div>

            <div class="filter-bar__field">
                <label class="form-label" for="<%= ddlFilter.ClientID %>">Show</label>

                <asp:DropDownList ID="ddlFilter" runat="server"
                    CssClass="form-control">
                    <asp:ListItem Text="All beverages" Value="" />
                    <asp:ListItem Text="Low stock" Value="low" />
                    <asp:ListItem Text="Out of stock" Value="out" />
                    <asp:ListItem Text="Inactive (hidden)" Value="inactive" />
                </asp:DropDownList>
            </div>

            <div class="filter-bar__buttons">
                <asp:Button ID="btnSearch" runat="server"
                    Text="Search"
                    CssClass="btn btn-primary"
                    OnClick="btnSearch_Click" />

                <asp:Button ID="btnClear" runat="server"
                    Text="Clear"
                    CssClass="btn btn-outline"
                    OnClick="btnClear_Click"
                    CausesValidation="false" />
            </div>
        </div>

        <asp:Label ID="lblCount" runat="server" CssClass="results-count" />

        <%-- PRODUCT CARDS --%>
        <asp:Repeater ID="rptProducts" runat="server"
            OnItemCommand="rptProducts_ItemCommand">

            <HeaderTemplate>
                <div class="product-grid">
            </HeaderTemplate>

            <ItemTemplate>
                <div class='<%# Convert.ToBoolean(Eval("IsActive")) ? "product-card" : "product-card is-inactive" %>'>

                    <div class="product-card__img">
                        <img src='<%# ResolveUrl(LoginRegisterV2.InventoryHelper.ImageUrl(Eval("ImagePath"))) %>'
                            alt='<%#: Eval("ItemName") %>'
                            loading="lazy" />
                    </div>

                    <div class="product-card__body">
                        <div class="product-card__name">
                            <%#: Eval("ItemName") %>
                        </div>

                        <div class="product-card__meta">
                            <%#: Eval("Category") %> &middot; SKU <%#: Eval("SKU") %>
                        </div>

                        <div class="product-card__price">
                            &#8369;<%# Eval("UnitPrice", "{0:N2}") %>
                        </div>

                        <div class="product-card__stock">
                            <span class='<%# StockBadgeClass(Eval("StockStatus")) %>'>
                                <%#: Eval("StockStatus") %>
                            </span>

                            <span>
                                <strong><%# Eval("Quantity") %></strong> in stock
                            </span>
                        </div>

                        <div class="product-card__meta">
                            Reorder at <%# Eval("ReorderLevel") %>
                            <%# Convert.ToBoolean(Eval("IsActive")) ? "" : " &middot; Hidden from ordering" %>
                        </div>
                    </div>

                    <div class="product-card__actions">

                        <asp:LinkButton runat="server"
                            CommandName="EditItem"
                            CommandArgument='<%# Eval("ItemId") %>'
                            CssClass="btn btn-outline btn-sm"
                            CausesValidation="false">
                            <svg class="ico"><use href="#i-edit" /></svg>Edit
                        </asp:LinkButton>

                        <asp:LinkButton runat="server"
                            CommandName="Toggle"
                            CommandArgument='<%# Eval("ItemId") %>'
                            CssClass="btn btn-outline btn-sm"
                            CausesValidation="false">
                            <%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>
                        </asp:LinkButton>

                       <asp:LinkButton runat="server"
                            CommandName="DeleteItem"
                            CommandArgument='<%# Eval("ItemId") %>'
                            CssClass="btn btn-danger btn-sm"
                            CausesValidation="false">
                            <svg class="ico"><use href="#i-trash" /></svg>Delete
                        </asp:LinkButton>

                    </div>
                </div>
            </ItemTemplate>

            <FooterTemplate>
                </div>
            </FooterTemplate>

        </asp:Repeater>

        <asp:Label ID="lblEmpty" runat="server"
            CssClass="empty-state"
            Visible="false"
            Text="No beverages match your search." />

    </div>
</asp:Content>