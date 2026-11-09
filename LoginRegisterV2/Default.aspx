<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="LoginRegisterV2._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="page-shell">
        <div class="card text-center mb-sm">
            <h1 id="aspnetTitle" class="mt-0">Beverage Store Management System</h1>
            <p class="lead text-muted">A web-based inventory and sales management system for beverage retail — track stock, record orders, and monitor sales in one place.</p>
        </div>

        <div class="feature-grid">
            <section class="feature-card" aria-labelledby="gettingStartedTitle">
                <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-box-solid" /></svg></span>
                <h2 id="gettingStartedTitle">Inventory Monitoring</h2>
                <p class="text-muted">Keep track of beverage stock levels, so nothing runs out or goes to waste.</p>
            </section>
            <section class="feature-card" aria-labelledby="librariesTitle">
                <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-clipboard" /></svg></span>
                <h2 id="librariesTitle">Order Recording</h2>
                <p class="text-muted">Log transactions quickly and accurately as they happen at the counter.</p>
            </section>
            <section class="feature-card" aria-labelledby="hostingTitle">
                <span class="stat-tile__icon" aria-hidden="true"><svg class="ico"><use href="#i-chart" /></svg></span>
                <h2 id="hostingTitle">Sales Calculation</h2>
                <p class="text-muted">Get a clear picture of daily and overall sales performance.</p>
            </section>
        </div>
    </div>

</asp:Content>
