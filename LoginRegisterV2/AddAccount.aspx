<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="AddAccount.aspx.cs" Inherits="LoginRegisterV2.AddAccount" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-shell">
        <div class="dashboard-header">
            <div class="page-title-row">
                <span class="page-title-icon" aria-hidden="true"><svg class="ico"><use href="#i-user-plus" /></svg></span>
                <div>
                    <h2>Add Account</h2>
                    <p class="dashboard-user-line">Create a login for a new staff member. Admin and User have the same access.</p>
                </div>
            </div>
        </div>

        <asp:Label ID="lblSuccess" runat="server" CssClass="alert alert-success" />
        <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-error" />

        <div class="card card--compact">
            <div class="form-grid">
                <div class="form-group">
                    <label class="form-label" for="<%= txtFullName.ClientID %>">Full name</label>
                    <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" MaxLength="100" />
                    <asp:Label ID="lblFullNameError" runat="server" CssClass="field-error" />
                </div>
                <div class="form-group">
                    <label class="form-label" for="<%= txtUsername.ClientID %>">Username</label>
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" MaxLength="30" />
                    <span class="form-hint">3-30 letters, numbers, dot, dash or underscore.</span>
                    <asp:Label ID="lblUsernameError" runat="server" CssClass="field-error" />
                </div>
                <div class="form-group">
                    <label class="form-label" for="<%= txtEmail.ClientID %>">Email</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" MaxLength="100" TextMode="Email" />
                    <asp:Label ID="lblEmailError" runat="server" CssClass="field-error" />
                </div>
                <div class="form-group">
                    <label class="form-label" for="<%= txtNumber.ClientID %>">Phone number</label>
                    <asp:TextBox ID="txtNumber" runat="server" CssClass="form-control" MaxLength="13" />
                    <span class="form-hint">09XXXXXXXXX or +639XXXXXXXXX</span>
                    <asp:Label ID="lblNumberError" runat="server" CssClass="field-error" />
                </div>
                <div class="form-group">
                    <label class="form-label" for="<%= txtBirthDate.ClientID %>">Birth date</label>
                    <asp:TextBox ID="txtBirthDate" runat="server" CssClass="form-control" TextMode="Date" />
                    <asp:Label ID="lblBirthDateError" runat="server" CssClass="field-error" />
                </div>
                <div class="form-group">
                    <label class="form-label" for="<%= ddlRole.ClientID %>">Role</label>
                    <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-control">
                        <asp:ListItem Text="User (staff)" Value="User" />
                        <asp:ListItem Text="Admin (staff)" Value="Admin" />
                    </asp:DropDownList>
                    <asp:Label ID="lblRoleError" runat="server" CssClass="field-error" />
                </div>
            </div>

            <div class="form-group">
                <label class="form-label" for="<%= txtAddress.ClientID %>">Address</label>
                <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" MaxLength="200" />
                <asp:Label ID="lblAddressError" runat="server" CssClass="field-error" />
            </div>

            <div class="form-grid">
                <div class="form-group">
                    <label class="form-label" for="<%= txtPassword.ClientID %>">Password</label>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" MaxLength="16" autocomplete="new-password" />
                    <span class="form-hint">8-16 characters: uppercase, lowercase, number and special character.</span>
                    <asp:Label ID="lblPasswordError" runat="server" CssClass="field-error" />
                </div>
                <div class="form-group">
                    <label class="form-label" for="<%= txtConfirmPassword.ClientID %>">Confirm password</label>
                    <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="form-control" MaxLength="16" autocomplete="new-password" />
                    <asp:Label ID="lblConfirmPasswordError" runat="server" CssClass="field-error" />
                </div>
            </div>

            <div class="btn-row">
                <asp:Button ID="btnCreate" runat="server" Text="Create account" OnClick="btnCreate_Click" CssClass="btn btn-primary" />
                <asp:HyperLink ID="lnkCancel" runat="server" NavigateUrl="~/Dashboard.aspx" CssClass="btn btn-outline">Cancel</asp:HyperLink>
            </div>
        </div>
    </div>
</asp:Content>
