<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="LoginRegisterV2.ChangePassword" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="auth-shell">
        <div class="card">
            <div class="card-icon-badge" aria-hidden="true"><svg class="ico"><use href="#i-key" /></svg></div>
            <h2 class="card-title">Change Password</h2>
            <p class="card-subtitle">Choose a new password for your account</p>

            <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-error" />

            <div class="form-group">
                <label class="form-label" for="<%= txtNewPassword.ClientID %>">New Password</label>
                <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" CssClass="form-control" />
            </div>

            <div class="form-group">
                <label class="form-label" for="<%= txtConfirmPassword.ClientID %>">Confirm New Password</label>
                <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="form-control" />
            </div>

            <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" OnClick="btnChangePassword_Click" CssClass="btn btn-primary btn-block" />
        </div>
    </div>
</asp:Content>
