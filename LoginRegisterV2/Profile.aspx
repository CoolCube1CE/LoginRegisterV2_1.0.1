<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="LoginRegisterV2.Profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-shell">
        <div class="dashboard-header">
            <div class="page-title-row">
                <span class="page-title-icon" aria-hidden="true"><svg class="ico"><use href="#i-user" /></svg></span>
                <div>
                    <h2>My Profile</h2>
                    <p class="dashboard-user-line">The details of the account you are logged in with.</p>
                </div>
            </div>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-error" />

        <div class="card card--compact profile-card">
            <div class="profile-card__head">
                <span class="profile-card__avatar"><asp:Label ID="lblInitials" runat="server" /></span>
                <div>
                    <div class="profile-card__name"><asp:Label ID="lblFullName" runat="server" /></div>
                    <span class="badge badge-primary"><asp:Label ID="lblRole" runat="server" /></span>
                </div>
            </div>

            <dl class="detail-list">
                <div><dt>Username</dt><dd><asp:Label ID="lblUsername" runat="server" /></dd></div>
                <div><dt>Email</dt><dd><asp:Label ID="lblEmail" runat="server" /></dd></div>
                <div><dt>Role</dt><dd><asp:Label ID="lblRoleDetail" runat="server" /></dd></div>
                <div><dt>Phone number</dt><dd><asp:Label ID="lblPhone" runat="server" /></dd></div>
                <div><dt>Birth date</dt><dd><asp:Label ID="lblBirthDate" runat="server" /></dd></div>
                <div><dt>Address</dt><dd><asp:Label ID="lblAddress" runat="server" /></dd></div>
            </dl>

            <div class="btn-row">
                <asp:HyperLink ID="lnkChangePassword" runat="server" NavigateUrl="~/ChangePasswordOTP.aspx" CssClass="btn btn-primary"><svg class="ico"><use href="#i-key" /></svg>Change Password</asp:HyperLink>
                <asp:HyperLink ID="lnkAddAccount" runat="server" NavigateUrl="~/AddAccount.aspx" CssClass="btn btn-outline"><svg class="ico"><use href="#i-user-plus" /></svg>Add Account</asp:HyperLink>
            </div>
        </div>
    </div>
</asp:Content>
