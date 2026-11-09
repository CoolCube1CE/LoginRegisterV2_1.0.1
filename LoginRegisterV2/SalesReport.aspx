<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="SalesReport.aspx.cs" Inherits="LoginRegisterV2.SalesReport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-shell">
        <div class="dashboard-header">
            <div class="page-title-row">
                <span class="page-title-icon" aria-hidden="true"><svg class="ico"><use href="#i-chart" /></svg></span>
                <div>
                    <h2>Sales Reports</h2>
                    <p class="dashboard-user-line">View sales reports based on completed orders. Pending and cancelled orders are excluded.</p>
                </div>
            </div>
            <div class="dashboard-header__side no-print">
                <button type="button" class="btn btn-outline" onclick="window.print();"><svg class="ico"><use href="#i-print" /></svg>Print</button>
            </div>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-error" />

        <div class="filter-bar no-print">
            <div class="filter-bar__field">
                <label class="form-label" for="<%= ddlPeriod.ClientID %>">Report</label>
                <asp:DropDownList ID="ddlPeriod" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlPeriod_SelectedIndexChanged">
                    <asp:ListItem Text="Daily" Value="Daily" />
                    <asp:ListItem Text="Weekly (Mon - Sun)" Value="Weekly" />
                    <asp:ListItem Text="Monthly" Value="Monthly" />
                    <asp:ListItem Text="Quarterly" Value="Quarterly" />
                    <asp:ListItem Text="Yearly" Value="Yearly" />
                    <asp:ListItem Text="Custom Sales Range" Value="Custom" />
                </asp:DropDownList>
            </div>

            <asp:Panel ID="pnlRefDate" runat="server" CssClass="filter-bar__field">
                <label class="form-label" for="<%= txtRefDate.ClientID %>">Pick any date in the period</label>
                <asp:TextBox ID="txtRefDate" runat="server" CssClass="form-control" TextMode="Date" />
            </asp:Panel>

            <asp:Panel ID="pnlCustom" runat="server" Visible="false" CssClass="filter-bar__field filter-bar__field--pair">
                <div>
                    <label class="form-label" for="<%= txtStart.ClientID %>">Start date</label>
                    <asp:TextBox ID="txtStart" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div>
                    <label class="form-label" for="<%= txtEnd.ClientID %>">End date</label>
                    <asp:TextBox ID="txtEnd" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
            </asp:Panel>

            <div class="filter-bar__buttons">
                <asp:Button ID="btnShow" runat="server" Text="Show report" CssClass="btn btn-primary" OnClick="btnShow_Click" CausesValidation="false" />
                <asp:Button ID="btnToday" runat="server" Text="Today" CssClass="btn btn-outline" OnClick="btnToday_Click" CausesValidation="false" />
            </div>
        </div>

        <p class="report-range"><svg class="ico" aria-hidden="true"><use href="#i-cal" /></svg><asp:Label ID="lblRange" runat="server" /></p>

        <div class="dashboard-grid dashboard-grid--four">
            <div class="stat-tile stat-tile--violet">
                <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-peso" /></svg></span>
                <div class="stat-tile__label">Total Sales</div>
                <div class="stat-tile__value"><asp:Label ID="lblTotalSales" runat="server" Text="&#8369;0.00" /></div>
                <div class="stat-tile__hint">Completed orders</div>
            </div>
            <div class="stat-tile stat-tile--info">
                <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-clipboard" /></svg></span>
                <div class="stat-tile__label">Total Orders</div>
                <div class="stat-tile__value"><asp:Label ID="lblTotalOrders" runat="server" Text="0" /></div>
                <div class="stat-tile__hint">In this period</div>
            </div>
            <div class="stat-tile stat-tile--teal">
                <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-box-solid" /></svg></span>
                <div class="stat-tile__label">Total Items Sold</div>
                <div class="stat-tile__value"><asp:Label ID="lblItemsSold" runat="server" Text="0" /></div>
                <div class="stat-tile__hint">Units across all beverages</div>
            </div>
            <div class="stat-tile stat-tile--warning">
                <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-chart" /></svg></span>
                <div class="stat-tile__label">Best-Selling Beverage</div>
                <div class="stat-tile__value stat-tile__value--text"><asp:Label ID="lblBestSeller" runat="server" Text="-" /></div>
                <div class="stat-tile__hint"><asp:Label ID="lblBestSellerHint" runat="server" Text="No sales in this period" /></div>
            </div>
        </div>

        <div class="dashboard-columns">
            <div class="dashboard-section">
                <div class="dashboard-section__header">
                    <h3><svg class="ico ico-blue"><use href="#i-box" /></svg>Sales per Beverage</h3>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="gvByBeverage" runat="server" CssClass="app-table" AutoGenerateColumns="false"
                        GridLines="None" EmptyDataText="No completed sales in this period.">
                        <Columns>
                            <asp:BoundField DataField="ItemName" HeaderText="Beverage" />
                            <asp:BoundField DataField="QuantitySold" HeaderText="Quantity Sold" />
                            <asp:BoundField DataField="Revenue" HeaderText="Revenue" DataFormatString="&#8369;{0:N2}" HtmlEncode="false" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

            <asp:Panel ID="pnlDaily" runat="server" CssClass="dashboard-section">
                <div class="dashboard-section__header">
                    <h3><svg class="ico ico-blue"><use href="#i-cal" /></svg>Sales per Day</h3>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="gvByDay" runat="server" CssClass="app-table" AutoGenerateColumns="false"
                        GridLines="None" EmptyDataText="No completed sales in this period.">
                        <Columns>
                            <asp:BoundField DataField="SaleDate" HeaderText="Date" DataFormatString="{0:ddd, MMM d, yyyy}" />
                            <asp:BoundField DataField="OrderCount" HeaderText="Orders" />
                            <asp:BoundField DataField="Sales" HeaderText="Sales" DataFormatString="&#8369;{0:N2}" HtmlEncode="false" />
                        </Columns>
                    </asp:GridView>
                </div>
            </asp:Panel>
        </div>

        <div class="dashboard-section">
            <div class="dashboard-section__header">
                <h3><svg class="ico ico-blue"><use href="#i-doc" /></svg>Completed Orders</h3>
            </div>
            <div class="table-responsive">
                <asp:GridView ID="gvOrders" runat="server" CssClass="app-table" AutoGenerateColumns="false"
                    GridLines="None" EmptyDataText="No completed orders in this period.">
                    <Columns>
                        <asp:TemplateField HeaderText="Order #">
                            <ItemTemplate><strong><%# LoginRegisterV2.OrderHelper.OrderNumber(Eval("OrderId")) %></strong></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="CompletedAt" HeaderText="Date &amp; Time" DataFormatString="{0:MMM d, yyyy h:mm tt}" />
                        <asp:BoundField DataField="CustomerName" HeaderText="Customer" />
                        <asp:BoundField DataField="ItemsSummary" HeaderText="Beverages" />
                        <asp:BoundField DataField="TotalQty" HeaderText="Items" />
                        <asp:BoundField DataField="GrandTotal" HeaderText="Total" DataFormatString="&#8369;{0:N2}" HtmlEncode="false" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
