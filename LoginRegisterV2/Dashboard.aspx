<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="LoginRegisterV2.Dashboard" %>
<%@ Register TagPrefix="uc" TagName="DashboardOverview" Src="~/DashboardOverview.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-shell">
        <div class="dashboard-header">
            <div>
                <span class="dashboard-greeting">Good day,</span>
                <h1><asp:Label ID="lblWelcome" runat="server" Text="Welcome back!" /></h1>
                <p class="dashboard-user-line">Here's what's happening in your store today.</p>
            </div>
            <div class="login-chip">
                <svg class="ico" aria-hidden="true"><use href="#i-cal" /></svg>
                <span class="login-chip__text">
                    <span class="login-chip__label">Logged in as</span>
                    <asp:Label ID="lblUserEmail" runat="server" CssClass="login-chip__email" />
                </span>
            </div>
        </div>

        <uc:DashboardOverview ID="dashboardOverview" runat="server" />
    </div>
</asp:Content>
