<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DashboardOverview.ascx.cs" Inherits="LoginRegisterV2.DashboardOverview" %>

<asp:Label ID="lblError" runat="server" CssClass="alert alert-error" />

<div class="dashboard-grid">
    <div class="stat-tile stat-tile--violet">
        <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-peso" /></svg></span>
        <div class="stat-tile__label">Today's Sales</div>
        <div class="stat-tile__value"><asp:Label ID="lblTodaySales" runat="server" Text="₱0.00" /></div>
        <div class="stat-tile__hint">From completed orders</div>
    </div>
    <div class="stat-tile stat-tile--info">
        <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-clipboard" /></svg></span>
        <div class="stat-tile__label">Today's Orders</div>
        <div class="stat-tile__value"><asp:Label ID="lblTodayOrders" runat="server" Text="0" /></div>
        <div class="stat-tile__hint"><asp:Label ID="lblTodayItems" runat="server" Text="0 items sold" /></div>
    </div>
    <div class="stat-tile stat-tile--teal">
        <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-box-solid" /></svg></span>
        <div class="stat-tile__label">Inventory</div>
        <div class="stat-tile__value"><asp:Label ID="lblUnits" runat="server" Text="0" /></div>
        <div class="stat-tile__hint"><asp:Label ID="lblInventoryHint" runat="server" Text="units in stock" /></div>
    </div>
    <div class="stat-tile stat-tile--warning">
        <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-warn" /></svg></span>
        <div class="stat-tile__label">Low Stock</div>
        <div class="stat-tile__value"><asp:Label ID="lblLowStockCount" runat="server" Text="0" /></div>
        <div class="stat-tile__hint">Beverages needing a restock</div>
    </div>
    <div class="stat-tile stat-tile--info">
        <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-chart" /></svg></span>
        <div class="stat-tile__label">Best Seller (this month)</div>
        <div class="stat-tile__value stat-tile__value--text"><asp:Label ID="lblBestSeller" runat="server" Text="-" /></div>
        <div class="stat-tile__hint"><asp:Label ID="lblBestSellerHint" runat="server" Text="No sales yet" /></div>
    </div>
    <div class="stat-tile stat-tile--violet">
        <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-sheet" /></svg></span>
        <div class="stat-tile__label">Stock Value</div>
        <div class="stat-tile__value"><asp:Label ID="lblStockValue" runat="server" Text="₱0.00" /></div>
        <div class="stat-tile__hint">At selling price</div>
    </div>
</div>

<div class="dashboard-actions">
    <asp:HyperLink ID="lnkActionNewOrder" runat="server" NavigateUrl="~/NewOrder.aspx" CssClass="btn btn-primary"><svg class="ico"><use href="#i-cart" /></svg>New Order</asp:HyperLink>
    <asp:HyperLink ID="lnkActionReceive" runat="server" NavigateUrl="~/StockTransactions.aspx" CssClass="btn btn-outline"><svg class="ico"><use href="#i-swap" /></svg>Record Stock Movement</asp:HyperLink>
    <asp:HyperLink ID="lnkActionInventory" runat="server" NavigateUrl="~/Inventory.aspx" CssClass="btn btn-outline"><svg class="ico"><use href="#i-box" /></svg>Manage Beverages</asp:HyperLink>
    <asp:HyperLink ID="lnkActionReports" runat="server" NavigateUrl="~/SalesReport.aspx" CssClass="btn btn-outline"><svg class="ico"><use href="#i-chart" /></svg>Sales Reports</asp:HyperLink>
</div>

<div class="dashboard-columns">
    <div class="dashboard-section">
        <div class="dashboard-section__header">
            <h3><svg class="ico ico-orange"><use href="#i-warn" /></svg>Low Stock Items</h3>
            <asp:HyperLink ID="lnkViewLow" runat="server" NavigateUrl="~/Inventory.aspx?filter=low" CssClass="text-action-link">View all &rarr;</asp:HyperLink>
        </div>
        <div class="table-responsive">
            <asp:GridView ID="gvLowStock" runat="server" CssClass="app-table app-table--low" AutoGenerateColumns="false"
                GridLines="None" EmptyDataText="All stock levels look healthy.">
                <Columns>
                    <asp:BoundField DataField="ItemName" HeaderText="Beverage" />
                    <asp:BoundField DataField="SKU" HeaderText="SKU" />
                    <asp:BoundField DataField="Quantity" HeaderText="In Stock" />
                    <asp:BoundField DataField="ReorderLevel" HeaderText="Reorder Level" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <span class='<%# Convert.ToInt32(Eval("Quantity")) <= 0 ? "stock-badge stock-badge--out" : "stock-badge stock-badge--low" %>'><%# Convert.ToInt32(Eval("Quantity")) <= 0 ? "Out" : "Low" %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <div class="dashboard-section">
        <div class="dashboard-section__header">
            <h3><svg class="ico ico-blue"><use href="#i-doc" /></svg>Recent Orders</h3>
            <asp:HyperLink ID="lnkViewOrders" runat="server" NavigateUrl="~/Orders.aspx" CssClass="text-action-link">View all &rarr;</asp:HyperLink>
        </div>
        <div class="table-responsive">
            <asp:GridView ID="gvRecentOrders" runat="server" CssClass="app-table" AutoGenerateColumns="false"
                GridLines="None" EmptyDataText="No orders placed yet.">
                <Columns>
                    <asp:TemplateField HeaderText="Order #">
                        <ItemTemplate><%# LoginRegisterV2.OrderHelper.OrderNumber(Eval("OrderId")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="CustomerName" HeaderText="Customer" />
                    <asp:BoundField DataField="ItemsSummary" HeaderText="Beverages" />
                    <asp:BoundField DataField="GrandTotal" HeaderText="Total" DataFormatString="₱{0:N2}" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class='<%# "status-pill status-pill--" + Eval("Status").ToString().ToLower() %>'><%# Eval("Status") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="OrderDate" HeaderText="Date" DataFormatString="{0:MMM d, h:mm tt}" />
                </Columns>
            </asp:GridView>
        </div>
    </div>
</div>

<div class="dashboard-section">
    <div class="dashboard-section__header">
        <h3><svg class="ico ico-blue"><use href="#i-swap" /></svg>Recent Stock Changes</h3>
        <asp:HyperLink ID="lnkViewStock" runat="server" NavigateUrl="~/StockTransactions.aspx" CssClass="text-action-link">View all &rarr;</asp:HyperLink>
    </div>
    <div class="table-responsive">
        <asp:GridView ID="gvRecentTransactions" runat="server" CssClass="app-table" AutoGenerateColumns="false"
            GridLines="None" EmptyDataText="No stock movements recorded yet.">
            <Columns>
                <asp:BoundField DataField="ItemName" HeaderText="Beverage" />
                <asp:TemplateField HeaderText="Type">
                    <ItemTemplate>
                        <span class='<%# Eval("TransactionType").ToString() == "IN" ? "stock-badge stock-badge--in" : "stock-badge stock-badge--out" %>'><%# Eval("TransactionType").ToString() == "IN" ? "In" : "Out" %></span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Quantity" HeaderText="Qty" />
                <asp:BoundField DataField="TransactionDate" HeaderText="Date" DataFormatString="{0:MMM d, h:mm tt}" />
                <asp:BoundField DataField="Reference" HeaderText="Remarks" NullDisplayText="-" />
            </Columns>
        </asp:GridView>
    </div>
</div>
