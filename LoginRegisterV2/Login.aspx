<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="LoginRegisterV2.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .login-page { max-width: 500px; margin: 4vh auto 0; }

        .login-card {
            padding: 40px 36px 32px;
            border-radius: 22px;
            box-shadow: 0 20px 50px rgba(20, 50, 100, 0.14);
        }
        .login-card .card-title { font-size: 34px; font-weight: 800; text-align: center; margin: 0 0 8px; }
        .login-card .card-subtitle { font-size: 16px; text-align: center; margin: 0 0 28px; }
        .login-card .form-label { display: block; font-size: 15px; font-weight: 700; color: var(--text-color); margin-bottom: 8px; }

        .login-card .input-icon { position: relative; }
        .login-card .input-icon input.form-control {
            display: block;
            width: 100%;
            height: 49px;
            padding: 0 16px 0 52px;
            font-size: 16px;
            border: 1.5px solid var(--input-border);
            border-radius: 10px;
            box-sizing: border-box;
        }
        .login-card .input-icon--toggle input.form-control { padding-right: 54px; }

        .login-card .input-icon__lead {
            position: absolute;
            left: 16px;
            top: 50%;
            transform: translateY(-50%);
            width: 20px;
            height: 20px;
            color: var(--text-soft);
            pointer-events: none;
        }

        /* show / hide password button (slashed eye = hidden) */
        .login-card .pw-toggle {
            position: absolute;
            right: 8px;
            top: 50%;
            transform: translateY(-50%);
            width: 38px;
            height: 38px;
            padding: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            -webkit-appearance: none;
            appearance: none;
            background: transparent;
            border: none;
            border-radius: 8px;
            color: var(--text-soft);
            cursor: pointer;
        }
        .login-card .pw-toggle:hover { background-color: var(--surface-alt); }
        .login-card .pw-toggle .ico { width: 20px; height: 20px; }
        .login-card .pw-toggle::after {
            content: "";
            position: absolute;
            width: 22px;
            height: 2px;
            background-color: currentColor;
            transform: rotate(-45deg);
            pointer-events: none;
        }
        .login-card .pw-toggle.is-visible::after { display: none; }

        .login-card .btn-primary { height: 49px; font-size: 17px; font-weight: 700; border-radius: 10px; }
        html:not(.dark-theme) .login-card .btn-primary { background-color: #0A2652; }
        html:not(.dark-theme) .login-card .btn-primary:hover { background-color: #123470; }

        @media (max-width: 600px) {
            .login-card { padding: 28px 20px 24px; }
            .login-card .card-title { font-size: 28px; }
        }
    </style>

    <div class="auth-shell login-page">
        <div class="card login-card">
            <h2 class="card-title">Welcome Back</h2>
            <p class="card-subtitle">Staff sign in &middot; Beverage Store</p>

            <asp:Label ID="lblSuccess" runat="server" CssClass="alert alert-success" />
            <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-error" />

            <div class="form-group">
                <label class="form-label" for="<%= txtEmail.ClientID %>">Email or Username</label>
                <div class="input-icon">
                    <svg class="ico input-icon__lead" aria-hidden="true"><use href="#i-mail" /></svg>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" MaxLength="100"
                        placeholder="Enter your email or username" autocomplete="username" />
                </div>
            </div>

            <div class="form-group">
                <label class="form-label" for="<%= txtPassword.ClientID %>">Password</label>
                <div class="input-icon input-icon--toggle">
                    <svg class="ico input-icon__lead" aria-hidden="true"><use href="#i-lock" /></svg>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" MaxLength="64"
                        placeholder="Enter your password" autocomplete="current-password" />
                    <button type="button" class="pw-toggle" onclick="togglePassword(this)" aria-label="Show password" title="Show / hide password">
                        <svg class="ico" aria-hidden="true"><use href="#i-eye" /></svg>
                    </button>
                </div>
            </div>

             <div class="form-group">
             <asp:CheckBox ID="chkRemember" runat="server" Text=" Remember me" />
             </div>

            <asp:Button ID="btnLogin" runat="server" Text="Login" OnClick="btnLogin_Click" CssClass="btn btn-primary btn-block" />

            <div class="divider"></div>

            <p class="text-center mb-0">
                <asp:HyperLink ID="lnkForgot" runat="server" NavigateUrl="~/ForgotPassword.aspx">Forgot your password?</asp:HyperLink>
            </p>

            <p class="text-center mb-0" style="margin-top:14px;">
                Don&rsquo;t have an account?
                <asp:HyperLink ID="lnkSignUp" runat="server" NavigateUrl="~/Register.aspx">Sign up</asp:HyperLink>
            </p>
        </div>
    </div>

    <script>
        /** @param {HTMLElement} btn */
        function togglePassword(btn) {
            /** @type {HTMLInputElement | null} */
            var input = /** @type {HTMLInputElement | null} */ (document.getElementById('<%= txtPassword.ClientID %>'));
            if (!input) { return; }
            var show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            btn.classList.toggle('is-visible', show);
            btn.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
        }
    </script>

</asp:Content>