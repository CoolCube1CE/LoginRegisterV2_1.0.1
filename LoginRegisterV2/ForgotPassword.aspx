<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="LoginRegisterV2.ForgotPassword" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="auth-shell">
        <div class="card">
            <div class="card-icon-badge" aria-hidden="true"><svg class="ico"><use href="#i-key" /></svg></div>
            <h2 class="card-title">Forgot Password</h2>
            <p class="card-subtitle">Reset your staff account password in three quick steps.</p>

            <asp:Label ID="lblSuccess" runat="server" CssClass="alert alert-success" />
            <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-error" />

            <%-- Step 1: email --%>
            <asp:Panel ID="pnlEmail" runat="server">
                <div class="form-group">
                    <label class="form-label" for="<%= txtEmail.ClientID %>">Registered email address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" MaxLength="100" TextMode="Email" />
                    <span class="form-hint">We will send a 6-digit code to this address.</span>
                </div>
                <asp:Button ID="btnSendCode" runat="server" Text="Send verification code" OnClick="btnSendCode_Click" CssClass="btn btn-primary btn-block" />
            </asp:Panel>

            <%-- Step 2: code --%>
            <asp:Panel ID="pnlCode" runat="server" Visible="false">
                <div class="form-group">
                    <label class="form-label" for="<%= txtOtp.ClientID %>">6-digit verification code</label>
                    <asp:TextBox ID="txtOtp" runat="server" CssClass="form-control" MaxLength="6" autocomplete="one-time-code" />
                    <span class="form-hint">The code expires in 10 minutes.</span>
                </div>
                <asp:Button ID="btnVerify" runat="server" Text="Verify code" OnClick="btnVerify_Click" CssClass="btn btn-primary btn-block" />
                <p class="text-center mt-sm mb-0">
                    <asp:LinkButton ID="btnResend" runat="server" OnClick="btnResend_Click" CausesValidation="false">Send a new code</asp:LinkButton>
                </p>
            </asp:Panel>

            <%-- Step 3: new password --%>
            <asp:Panel ID="pnlReset" runat="server" Visible="false">
                <div class="form-group">
                    <label class="form-label" for="<%= txtNewPassword.ClientID %>">New password</label>
                    <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" CssClass="form-control" MaxLength="16" autocomplete="new-password" />
                    <span class="form-hint">8-16 characters with an uppercase letter, lowercase letter, number and special character.</span>
                </div>
                <div class="form-group">
                    <label class="form-label" for="<%= txtConfirmPassword.ClientID %>">Confirm new password</label>
                    <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="form-control" MaxLength="16" autocomplete="new-password" />
                </div>
                <asp:Button ID="btnReset" runat="server" Text="Reset password" OnClick="btnReset_Click" CssClass="btn btn-primary btn-block" />
            </asp:Panel>

            <div class="divider"></div>
            <p class="text-center mb-0">
                <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="~/Login.aspx">Back to login</asp:HyperLink>
            </p>
        </div>
    </div>

</asp:Content>
