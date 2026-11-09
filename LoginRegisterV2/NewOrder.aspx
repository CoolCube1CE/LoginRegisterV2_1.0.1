<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="NewOrder.aspx.cs" Inherits="LoginRegisterV2.NewOrder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-shell">
        <div class="dashboard-header">
            <div class="page-title-row">
                <span class="page-title-icon" aria-hidden="true"><svg class="ico"><use href="#i-cart" /></svg></span>
                <div>
                    <h2>New Order</h2>
                    <p class="dashboard-user-line">Create orders by selecting beverages and setting their quantities. Stock is updated once the order is saved.</p>
                </div>
            </div>
        </div>

        <asp:Label ID="lblSuccess" runat="server" CssClass="alert alert-success" />
        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-error" />

        <div class="pos-layout">
            <div class="pos-products">
                <div class="filter-bar">
                    <div class="filter-bar__field filter-bar__field--grow">
                        <label class="form-label" for="<%= txtSearch.ClientID %>">Find a beverage</label>
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" placeholder="Name, category or SKU" MaxLength="100" />
                    </div>
                    <div class="filter-bar__buttons">
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" CausesValidation="false" />
                        <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CssClass="btn btn-outline" OnClick="btnClearSearch_Click" CausesValidation="false" />
                    </div>
                </div>

                <asp:Repeater ID="rptProducts" runat="server" OnItemCommand="rptProducts_ItemCommand">
                    <HeaderTemplate><div class="product-grid product-grid--compact"></HeaderTemplate>
                    <ItemTemplate>
                        <div class='<%# Convert.ToInt32(Eval("Quantity")) > 0 ? "product-card" : "product-card is-inactive" %>'>
                            <div class="product-card__img">
                                <img src='<%# ResolveUrl(LoginRegisterV2.InventoryHelper.ImageUrl(Eval("ImagePath"))) %>' alt='<%#: Eval("ItemName") %>' loading="lazy" />
                            </div>
                            <div class="product-card__body">
                                <div class="product-card__name"><%#: Eval("ItemName") %></div>
                                <div class="product-card__price">&#8369;<%# Eval("UnitPrice", "{0:N2}") %></div>
                                <div class="product-card__stock">
                                    <span class='<%# StockBadgeClass(Eval("StockStatus")) %>'><%#: Eval("StockStatus") %></span>
                                    <span><strong><%# Eval("Quantity") %></strong> available</span>
                                </div>
                            </div>
                            <div class="product-card__actions product-card__actions--add">
                                <asp:TextBox ID="txtQty" runat="server" Text="1" TextMode="Number" min="1" CssClass="form-control qty-input" onkeydown="if(event.key==='Enter'){event.preventDefault();}" Enabled='<%# Convert.ToInt32(Eval("Quantity")) > 0 %>' />
                                <asp:LinkButton ID="btnAddToCart" runat="server" CommandName="Add" CommandArgument='<%# Eval("ItemId") %>' CssClass="btn btn-primary btn-sm" CausesValidation="false" Enabled='<%# Convert.ToInt32(Eval("Quantity")) > 0 %>'><svg class="ico"><use href="#i-plus" /></svg>Add</asp:LinkButton>
                            </div>
                        </div>
                    </ItemTemplate>
                    <FooterTemplate></div></FooterTemplate>
                </asp:Repeater>

                <asp:Label ID="lblNoProducts" runat="server" CssClass="empty-state" Visible="false" Text="No beverages match your search." />
            </div>

            <aside class="pos-cart card">
                <h3 class="mt-0">Current Order</h3>

                <asp:Repeater ID="rptCart" runat="server" OnItemCommand="rptCart_ItemCommand">
                    <HeaderTemplate><ul class="cart-lines"></HeaderTemplate>
                    <ItemTemplate>
                        <li class="cart-line">
                            <div class="cart-line__info">
                                <div class="cart-line__name"><%#: Eval("ItemName") %></div>
                                <div class="cart-line__price">&#8369;<%# Eval("UnitPrice", "{0:N2}") %> each</div>
                            </div>
                            <div class="cart-line__qty">
                                <asp:LinkButton runat="server" CommandName="Dec" CommandArgument='<%# Eval("ItemId") %>' CssClass="qty-btn" CausesValidation="false" ToolTip="Less"><svg class="ico"><use href="#i-minus" /></svg></asp:LinkButton>
                                <span class="qty-value"><%# Eval("Qty") %></span>
                                <asp:LinkButton runat="server" CommandName="Inc" CommandArgument='<%# Eval("ItemId") %>' CssClass="qty-btn" CausesValidation="false" ToolTip="More"><svg class="ico"><use href="#i-plus" /></svg></asp:LinkButton>
                            </div>
                            <div class="cart-line__total">&#8369;<%# Eval("LineTotal", "{0:N2}") %></div>
                            <asp:LinkButton runat="server" CommandName="Remove" CommandArgument='<%# Eval("ItemId") %>' CssClass="qty-btn qty-btn--remove" CausesValidation="false" ToolTip="Remove"><svg class="ico"><use href="#i-trash" /></svg></asp:LinkButton>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate></ul></FooterTemplate>
                </asp:Repeater>

                <asp:Label ID="lblCartEmpty" runat="server" CssClass="empty-state" Text="No beverages added yet." />

                <div class="cart-total">
                    <span>Total</span>
                    <strong><asp:Label ID="lblTotal" runat="server" Text="&#8369;0.00" /></strong>
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtCustomer.ClientID %>">Customer name (optional)</label>
                    <asp:TextBox ID="txtCustomer" runat="server" CssClass="form-control" MaxLength="150" placeholder="Walk-in customer" onkeydown="if(event.key==='Enter'){event.preventDefault();}" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= ddlStatus.ClientID %>">Order status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Completed (paid / handed over)" Value="Completed" />
                        <asp:ListItem Text="Pending (reserved, not yet paid)" Value="Pending" />
                    </asp:DropDownList>
                </div>

                <asp:HiddenField ID="hfToken" runat="server" />

                <div class="btn-row btn-row--stack">
                    <asp:Button ID="btnPlace" runat="server" Text="Place order" CssClass="btn btn-primary btn-block" OnClick="btnPlace_Click" OnClientClick="if (this.getAttribute('data-busy')) { return false; } this.setAttribute('data-busy', '1'); this.value = 'Saving...';" UseSubmitBehavior="true" />
                    <asp:Button ID="btnClearCart" runat="server" Text="Clear order" CssClass="btn btn-outline btn-block" OnClick="btnClearCart_Click" CausesValidation="false" />
                </div>
            </aside>
        </div>
    </div>
</asp:Content>
