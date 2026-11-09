
<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Orders.aspx.cs" Inherits="LoginRegisterV2.Orders" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        /* Order Details Modal */
        .order-modal-overlay {
            position: fixed;
            inset: 0;
            z-index: 9999;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
            background: rgba(15, 23, 42, 0.60);
            overflow-y: auto;
        }

        .order-modal {
            width: 100%;
            max-width: 850px;
            max-height: 90vh;
            overflow-y: auto;
            background: #fff;
            border-radius: 14px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.25);
            padding: 26px;
            animation: orderModalAppear 0.2s ease-out;
        }

        .order-modal-header {
            display: flex;
            align-items: flex-start;
            justify-content: space-between;
            gap: 16px;
            padding-bottom: 18px;
            margin-bottom: 18px;
            border-bottom: 1px solid #e5e7eb;
        }

        .order-modal-header h3 {
            margin: 0;
            font-size: 22px;
        }

        .order-modal-close {
            flex-shrink: 0;
        }

        .order-modal-info {
            margin-bottom: 20px;
            color: #64748b;
            font-size: 14px;
        }

        .order-modal-total {
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 12px;
            padding: 18px 0;
            margin-top: 12px;
            border-top: 1px solid #e5e7eb;
            font-size: 16px;
        }

        .order-modal-total strong {
            font-size: 22px;
        }

        .order-modal-status {
            margin-top: 18px;
            padding-top: 18px;
            border-top: 1px solid #e5e7eb;
        }

        .order-modal-status .filter-bar {
            margin-bottom: 10px;
        }

        .order-modal-footer {
            display: flex;
            justify-content: flex-end;
            gap: 10px;
            margin-top: 22px;
            padding-top: 18px;
            border-top: 1px solid #e5e7eb;
        }

        @keyframes orderModalAppear {
            from {
                opacity: 0;
                transform: translateY(10px) scale(0.98);
            }
            to {
                opacity: 1;
                transform: translateY(0) scale(1);
            }
        }

        @media (max-width: 600px) {
            .order-modal {
                padding: 18px;
                max-height: 94vh;
            }

            .order-modal-header h3 {
                font-size: 18px;
            }

            .order-modal-total strong {
                font-size: 19px;
            }

            .order-modal-footer {
                flex-direction: column-reverse;
            }

            .order-modal-footer .btn {
                width: 100%;
            }
        }
    </style>

    <div class="page-shell">

        <div class="dashboard-header">
            <div class="page-title-row">
                <span class="page-title-icon" aria-hidden="true">
                    <svg class="ico"><use href="#i-clipboard" /></svg>
                </span>
                <div>
                    <h2>Order History</h2>
                    <p class="dashboard-user-line">
                        View recorded orders, including order numbers, items, totals, dates, and statuses.
                    </p>
                </div>
            </div>

            <div class="dashboard-header__side">
                <asp:HyperLink ID="lnkNewOrder" runat="server"
                    NavigateUrl="~/NewOrder.aspx"
                    CssClass="btn btn-primary">
                    <svg class="ico"><use href="#i-cart" /></svg>
                    New Order
                </asp:HyperLink>
            </div>
        </div>

        <asp:Label ID="lblSuccess" runat="server" CssClass="alert alert-success" />
        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-error" />

        <%-- ORDER DETAILS MODAL --%>
        <asp:Panel ID="pnlDetails" runat="server"
            Visible="false"
            CssClass="order-modal-overlay">

            <div class="order-modal">

                <div class="order-modal-header">
                    <div>
                        <h3>
                            <asp:Label ID="lblDetailTitle" runat="server" />
                        </h3>
                        <p class="order-modal-info">
                            <asp:Label ID="lblDetailInfo" runat="server" />
                        </p>
                    </div>

                    <asp:Button ID="btnCloseDetails" runat="server"
                        Text="&#10005;"
                        CssClass="btn btn-outline btn-sm order-modal-close"
                        OnClick="btnCloseDetails_Click"
                        CausesValidation="false" />
                </div>

                <asp:HiddenField ID="hfOrderId" runat="server" />

                <h4>Order Items</h4>

                <div class="table-responsive">
                    <asp:GridView ID="gvLines" runat="server"
                        CssClass="app-table"
                        AutoGenerateColumns="false"
                        GridLines="None"
                        EmptyDataText="This order has no items.">

                        <Columns>
                            <asp:BoundField DataField="ItemName" HeaderText="Beverage" />
                            <asp:BoundField DataField="Quantity" HeaderText="Qty" />
                            <asp:BoundField DataField="UnitPrice"
                                HeaderText="Unit Price"
                                DataFormatString="&#8369;{0:N2}"
                                HtmlEncode="false" />
                            <asp:BoundField DataField="LineTotal"
                                HeaderText="Line Total"
                                DataFormatString="&#8369;{0:N2}"
                                HtmlEncode="false" />
                        </Columns>
                    </asp:GridView>
                </div>

                <div class="order-modal-total">
                    <span>Order total</span>
                    <strong>
                        <asp:Label ID="lblDetailTotal" runat="server" />
                    </strong>
                </div>

                <div class="order-modal-status">

                    <h4>Update Order Status</h4>

                    <div class="filter-bar">
                        <div class="filter-bar__field">
                            <label class="form-label"
                                for="<%= ddlDetailStatus.ClientID %>">
                                Status
                            </label>

                            <asp:DropDownList ID="ddlDetailStatus" runat="server"
                                CssClass="form-control">
                                <asp:ListItem Text="Pending" Value="Pending" />
                                <asp:ListItem Text="Completed" Value="Completed" />
                                <asp:ListItem Text="Cancelled" Value="Cancelled" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <span class="form-hint">
                        Cancelling an order returns its stock to the batches it came from.
                        Only Completed orders count in sales reports.
                    </span>

                </div>

                <div class="order-modal-footer">

                    <asp:Button ID="btnCloseDetailsFooter" runat="server"
                        Text="Close"
                        CssClass="btn btn-outline"
                        OnClick="btnCloseDetails_Click"
                        CausesValidation="false" />

                    <asp:Button ID="btnUpdateStatus" runat="server"
                        Text="Update status"
                        CssClass="btn btn-primary"
                        OnClick="btnUpdateStatus_Click"
                        CausesValidation="false"
                        OnClientClick="return confirmStatusChange();" />

                </div>

            </div>
        </asp:Panel>

        <%-- FILTERS --%>
        <div class="filter-bar">

            <div class="filter-bar__field filter-bar__field--grow">
                <label class="form-label" for="<%= txtSearch.ClientID %>">
                    Order number or customer
                </label>

                <asp:TextBox ID="txtSearch" runat="server"
                    CssClass="form-control"
                    placeholder="e.g. ORD-000012 or a name"
                    MaxLength="100" />
            </div>

            <div class="filter-bar__field">
                <label class="form-label" for="<%= ddlStatus.ClientID %>">
                    Status
                </label>

                <asp:DropDownList ID="ddlStatus" runat="server"
                    CssClass="form-control">
                    <asp:ListItem Text="All statuses" Value="" />
                    <asp:ListItem Text="Pending" Value="Pending" />
                    <asp:ListItem Text="Completed" Value="Completed" />
                    <asp:ListItem Text="Cancelled" Value="Cancelled" />
                </asp:DropDownList>
            </div>

            <div class="filter-bar__field">
                <label class="form-label" for="<%= txtFrom.ClientID %>">
                    From
                </label>

                <asp:TextBox ID="txtFrom" runat="server"
                    CssClass="form-control"
                    TextMode="Date" />
            </div>

            <div class="filter-bar__field">
                <label class="form-label" for="<%= txtTo.ClientID %>">
                    To
                </label>

                <asp:TextBox ID="txtTo" runat="server"
                    CssClass="form-control"
                    TextMode="Date" />
            </div>

            <div class="filter-bar__buttons">
                <asp:Button ID="btnFilter" runat="server"
                    Text="Search"
                    CssClass="btn btn-primary"
                    OnClick="btnFilter_Click"
                    CausesValidation="false" />

                <asp:Button ID="btnReset" runat="server"
                    Text="Reset"
                    CssClass="btn btn-outline"
                    OnClick="btnReset_Click"
                    CausesValidation="false" />
            </div>

        </div>

        <asp:Label ID="lblCount" runat="server" CssClass="results-count" />

        <%-- ORDERS TABLE --%>
        <div class="list-panel">
            <div class="table-responsive">

                <asp:GridView ID="gvOrders" runat="server"
                    CssClass="app-table"
                    AutoGenerateColumns="false"
                    GridLines="None"
                    DataKeyNames="OrderId"
                    OnRowCommand="gvOrders_RowCommand"
                    EmptyDataText="No orders found.">

                    <Columns>

                        <asp:TemplateField HeaderText="Order #">
                            <ItemTemplate>
                                <strong>
                                    <%# LoginRegisterV2.OrderHelper.OrderNumber(Eval("OrderId")) %>
                                </strong>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="OrderDate"
                            HeaderText="Date &amp; Time"
                            DataFormatString="{0:MMM d, yyyy h:mm tt}" />

                        <asp:BoundField DataField="CustomerName"
                            HeaderText="Customer" />

                        <asp:BoundField DataField="ItemsSummary"
                            HeaderText="Beverages" />

                        <asp:BoundField DataField="TotalQty"
                            HeaderText="Items" />

                        <asp:BoundField DataField="GrandTotal"
                            HeaderText="Total"
                            DataFormatString="&#8369;{0:N2}"
                            HtmlEncode="false" />

                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# "status-pill status-pill--" + Eval("Status").ToString().ToLower() %>'>
                                    <%# Eval("Status") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton runat="server"
                                    CommandName="ViewOrder"
                                    CommandArgument='<%# Eval("OrderId") %>'
                                    CssClass="btn btn-outline btn-sm"
                                    CausesValidation="false">
                                    <svg class="ico"><use href="#i-eye" /></svg>
                                    View
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>

                    </Columns>
                </asp:GridView>

            </div>
        </div>

    </div>

    <script>
        function confirmStatusChange() {
            var select = document.getElementById('<%= ddlDetailStatus.ClientID %>');

            if (select && select.value === 'Cancelled') {
                return confirm('Cancel this order? Its stock will be returned to inventory.');
            }

            return true;
        }
    </script>

</asp:Content>
