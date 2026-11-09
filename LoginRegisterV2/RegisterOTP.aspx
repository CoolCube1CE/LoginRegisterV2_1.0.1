<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="RegisterOTP.aspx.cs" Inherits="LoginRegisterV2.RegisterOTP" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="auth-shell">
        <div class="card text-center">
            <div class="card-icon-badge" aria-hidden="true"><svg class="ico"><use href="#i-shield" /></svg></div>
            <h2 class="card-title">Verify Your Email</h2>
            <p class="card-subtitle">
                <asp:Label ID="lblSentTo" runat="server" />
            </p>

            <asp:Label ID="lblSuccess" runat="server" CssClass="alert alert-success" />
            <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-error" />

            <div class="form-group">
                <label class="form-label" for="<%= txtOtp.ClientID %>">6-digit verification code</label>
                <asp:TextBox ID="txtOtp" runat="server" CssClass="form-control text-center" MaxLength="6" autocomplete="one-time-code" />
                <span class="form-hint">The code expires in 10 minutes.</span>
            </div>

            <div class="btn-row" style="justify-content:center;">
                <asp:Button ID="btnVerify" runat="server" Text="Verify &amp; create account" OnClick="btnVerify_Click" CssClass="btn btn-primary" />
                <asp:Button ID="btnBack" runat="server" Text="Back" OnClick="btnBack_Click" CausesValidation="false" CssClass="btn btn-secondary" />
            </div>

            <p class="text-center mt-sm mb-0">
                <asp:LinkButton ID="btnResend" runat="server" OnClick="btnResend_Click" CausesValidation="false">Send a new code</asp:LinkButton>
            </p>
        </div>
    </div>
</asp:Content>
