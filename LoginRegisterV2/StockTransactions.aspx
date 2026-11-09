<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="StockTransactions.aspx.cs" Inherits="LoginRegisterV2.StockTransactions" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-shell">
        <div class="dashboard-header">
            <div class="page-title-row">
                <span class="page-title-icon" aria-hidden="true"><svg class="ico"><use href="#i-swap" /></svg></span>
                <div>
                    <h2>Stock Transactions</h2>
                    <p class="dashboard-user-line">Record incoming and outgoing stock and monitor inventory movements.</p>
                </div>
            </div>
        </div>

        <asp:Label ID="lblSuccess" runat="server" CssClass="alert alert-success" />
        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-error" />

        <div class="card card--compact mb-sm">
            <h3 class="mt-0">Record a Stock Movement</h3>
            <div class="form-grid">
                <div class="form-group">
                    <label class="form-label" for="<%= ddlItem.ClientID %>">Beverage</label>
                    <asp:DropDownList ID="ddlItem" runat="server" CssClass="form-control" DataTextField="Label" DataValueField="ItemId" />
                </div>
                <div class="form-group">
                    <label class="form-label" for="<%= ddlType.ClientID %>">Movement</label>
                    <asp:DropDownList ID="ddlType" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Stock In (received - new batch)" Value="IN" />
                        <asp:ListItem Text="Stock Out (damaged, expired, correction)" Value="OUT" />
                    </asp:DropDownList>
                </div>
                <div class="form-group">
                    <label class="form-label" for="<%= txtQty.ClientID %>">Quantity</label>
                    <asp:TextBox ID="txtQty" runat="server" CssClass="form-control" TextMode="Number" min="1" />
                </div>
                <div class="form-group">
                    <label class="form-label" for="<%= txtNotes.ClientID %>">Notes (optional)</label>
                    <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" MaxLength="250" placeholder="Supplier, delivery no., reason..." />
                </div>
            </div>
            <asp:Button ID="btnRecord" runat="server" Text="Record movement" CssClass="btn btn-primary" OnClick="btnRecord_Click" />
        </div>

        <div class="dashboard-section">
            <div class="dashboard-section__header">
                <h3><svg class="ico ico-blue"><use href="#i-layers" /></svg>Stock Batches (oldest sold first)</h3>
            </div>
            <div class="table-responsive">
                <asp:GridView ID="gvBatches" runat="server" CssClass="app-table" AutoGenerateColumns="false"
                    GridLines="None" EmptyDataText="No stock on hand.">
                    <Columns>
                        <asp:BoundField DataField="ItemName" HeaderText="Beverage" />
                        <asp:TemplateField HeaderText="Batch">
                            <ItemTemplate>#<%# Eval("BatchId") %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ReceivedDate" HeaderText="Received" DataFormatString="{0:MMM d, yyyy h:mm tt}" />
                        <asp:BoundField DataField="QuantityReceived" HeaderText="Received Qty" />
                        <asp:BoundField DataField="QuantityRemaining" HeaderText="Remaining" />
                        <asp:BoundField DataField="Notes" HeaderText="Notes" NullDisplayText="-" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <div class="dashboard-section">
            <div class="dashboard-section__header">
                <h3><svg class="ico ico-blue"><use href="#i-doc" /></svg>Movement History</h3>
            </div>
            <div class="table-responsive">
                <asp:GridView ID="gvHistory" runat="server" CssClass="app-table" AutoGenerateColumns="false"
                    GridLines="None" EmptyDataText="No stock movements recorded yet.">
                    <Columns>
                        <asp:BoundField DataField="TransactionDate" HeaderText="Date" DataFormatString="{0:MMM d, yyyy h:mm tt}" />
                        <asp:BoundField DataField="ItemName" HeaderText="Beverage" />
                        <asp:TemplateField HeaderText="Type">
                            <ItemTemplate>
                                <span class='<%# Eval("TransactionType").ToString() == "IN" ? "stock-badge stock-badge--in" : "stock-badge stock-badge--out" %>'><%# Eval("TransactionType").ToString() == "IN" ? "In" : "Out" %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Quantity" HeaderText="Qty" />
                        <asp:BoundField DataField="Reference" HeaderText="Reference" NullDisplayText="-" />
                        <asp:BoundField DataField="Notes" HeaderText="Notes" NullDisplayText="-" />
                        <asp:BoundField DataField="PerformedBy" HeaderText="By" NullDisplayText="-" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
