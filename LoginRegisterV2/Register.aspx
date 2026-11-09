<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="LoginRegisterV2.Register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .pw-field { position: relative; }
        .pw-field .form-control { padding-right: 48px; }
        .pw-eye { position: absolute; right: 8px; top: 50%; transform: translateY(-50%); width: 36px; height: 36px;
                  display: flex; align-items: center; justify-content: center; background: transparent; border: none;
                  border-radius: 8px; color: var(--text-soft); cursor: pointer; padding: 0; }
        .pw-eye .ico { width: 20px; height: 20px; }
        .pw-eye::after { content: ""; position: absolute; width: 22px; height: 2px; background: currentColor;
                         transform: rotate(-45deg); pointer-events: none; }
        .pw-eye.is-visible::after { display: none; }
        .req-list { list-style: none; margin: 8px 0 0; padding: 0; font-size: 13px; }
        .req-list li.bad { color: #dc2626; }
        .req-list li.bad::before { content: "\2716  "; }
        .req-list li.ok { color: #16a34a; }
        .req-list li.ok::before { content: "\2714  "; }
        .strength { height: 8px; border-radius: 99px; background: #d1d5db; margin-top: 10px; overflow: hidden; }
        .strength span { display: block; height: 100%; width: 0; transition: width .2s, background-color .2s; }
        .strength-text { font-size: 13px; font-weight: 700; margin-top: 4px; }
        .match-text { font-size: 13px; margin-top: 4px; }
    </style>

    <div class="auth-shell auth-shell--wide">
        <div class="card">
            <div class="card-icon-badge" aria-hidden="true"><svg class="ico"><use href="#i-user-plus" /></svg></div>
            <h2 class="card-title">Create an Account</h2>
            <p class="card-subtitle">Staff sign up &middot; Beverage Store</p>

            <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-error" />

            <div class="form-grid">
                <div class="form-group">
                    <label class="form-label" for="<%= txtFullName.ClientID %>">Full name</label>
                    <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" MaxLength="100" autocomplete="name" />
                    <span class="form-hint">Letters only. Hyphen and apostrophe allowed.</span>
                    <asp:Label ID="lblFullNameError" runat="server" CssClass="field-error" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtBirthDate.ClientID %>">Birthday</label>
                    <asp:TextBox ID="txtBirthDate" runat="server" CssClass="form-control" TextMode="Date" />
                    <span class="form-hint">You must be at least 18 years old.</span>
                    <asp:Label ID="lblBirthDateError" runat="server" CssClass="field-error" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtAddress.ClientID %>">Address</label>
                    <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" MaxLength="200" autocomplete="street-address" />
                    <asp:Label ID="lblAddressError" runat="server" CssClass="field-error" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtEmail.ClientID %>">Email (this is your username)</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" MaxLength="50" TextMode="Email" autocomplete="username" />
                    <asp:Label ID="lblEmailError" runat="server" CssClass="field-error" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtNumber.ClientID %>">Mobile number</label>
                    <asp:TextBox ID="txtNumber" runat="server" CssClass="form-control" MaxLength="13" inputmode="tel" autocomplete="tel" />
                    <span class="form-hint">09XXXXXXXXX or +639XXXXXXXXX</span>
                    <asp:Label ID="lblNumberError" runat="server" CssClass="field-error" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= ddlRole.ClientID %>">Access type</label>
                    <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-control">
                        <asp:ListItem Value="" Text="-- Select --" />
                        <asp:ListItem Value="Admin" Text="Admin" />
                        <asp:ListItem Value="User" Text="User" />
                    </asp:DropDownList>
                    <asp:Label ID="lblRoleError" runat="server" CssClass="field-error" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtPassword.ClientID %>">Password</label>
                    <div class="pw-field">
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" MaxLength="16" autocomplete="new-password" />
                        <button type="button" class="pw-eye" data-target="<%= txtPassword.ClientID %>" aria-label="Show password"><svg class="ico"><use href="#i-eye" /></svg></button>
                    </div>
                    <ul class="req-list">
                        <li id="rule-len" class="bad">8-16 characters</li>
                        <li id="rule-up" class="bad">Uppercase letter</li>
                        <li id="rule-low" class="bad">Lowercase letter</li>
                        <li id="rule-num" class="bad">Number</li>
                        <li id="rule-sp" class="bad">Special character</li>
                    </ul>
                    <div class="strength"><span id="strengthBar"></span></div>
                    <div class="strength-text" id="strengthText">Weak</div>
                    <asp:Label ID="lblPasswordError" runat="server" CssClass="field-error" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtConfirmPassword.ClientID %>">Confirm password</label>
                    <div class="pw-field">
                        <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="form-control" MaxLength="16" autocomplete="off" />
                        <button type="button" class="pw-eye" data-target="<%= txtConfirmPassword.ClientID %>" aria-label="Show password"><svg class="ico"><use href="#i-eye" /></svg></button>
                    </div>
                    <div class="match-text" id="matchText"></div>
                    <asp:Label ID="lblConfirmPasswordError" runat="server" CssClass="field-error" />
                </div>
            </div>

            <div class="form-group">
                <asp:CheckBox ID="chkAgree" runat="server" Text=" I have read and agree to the Data Privacy Terms and Agreement" />
                <asp:Label ID="lblAgreeError" runat="server" CssClass="field-error" />
            </div>

            <asp:Button ID="btnRegister" runat="server" Text="Sign up" OnClick="btnRegister_Click" CssClass="btn btn-primary btn-block" />

            <div class="divider"></div>
            <p class="text-center mb-0">
                Already have an account?
                <asp:HyperLink ID="lnkLogin" runat="server" NavigateUrl="~/Login.aspx">Log in</asp:HyperLink>
            </p>
        </div>
    </div>

    <script>
        (function () {
            function $(id) { return document.getElementById(id); }
            var fullName = $('<%= txtFullName.ClientID %>');
            var email    = $('<%= txtEmail.ClientID %>');
            var phone    = $('<%= txtNumber.ClientID %>');
            var address  = $('<%= txtAddress.ClientID %>');
            var pw       = $('<%= txtPassword.ClientID %>');
            var cpw      = $('<%= txtConfirmPassword.ClientID %>');

            // block unwanted characters while typing / pasting
            fullName.addEventListener('input', function () {
                this.value = this.value.replace(/[^\p{L} '’\-]/gu, '');
            });
            email.addEventListener('input', function () {
                this.value = this.value.replace(/\s/g, '');
            });
            address.addEventListener('input', function () {
                this.value = this.value.replace(/[<>]/g, '');
            });
            phone.addEventListener('input', function () {
                var v = this.value.replace(/[^\d+]/g, '');
                var plus = v.charAt(0) === '+';
                v = v.replace(/\+/g, '');
                this.value = (plus ? '+' : '') + v.substring(0, plus ? 12 : 11);
            });

            // password rules + strength
            var rules = {
                'rule-len': /^.{8,16}$/,
                'rule-up':  /[A-Z]/,
                'rule-low': /[a-z]/,
                'rule-num': /[0-9]/,
                'rule-sp':  /[^A-Za-z0-9]/
            };
            function checkPassword() {
                var passed = 0;
                for (var id in rules) {
                    var ok = rules[id].test(pw.value);
                    $(id).className = ok ? 'ok' : 'bad';
                    if (ok) { passed++; }
                }
                var bar = $('strengthBar'), txt = $('strengthText');
                bar.style.width = (passed * 20) + '%';
                if (passed === 5)     { bar.style.backgroundColor = '#16a34a'; txt.textContent = 'Strong';  txt.style.color = '#16a34a'; }
                else if (passed >= 3) { bar.style.backgroundColor = '#eab308'; txt.textContent = 'Average'; txt.style.color = '#ca8a04'; }
                else                  { bar.style.backgroundColor = '#dc2626'; txt.textContent = 'Weak';    txt.style.color = '#dc2626'; }
                checkMatch();
            }
            function checkMatch() {
                var m = $('matchText');
                if (!cpw.value) { m.textContent = ''; return; }
                var same = cpw.value === pw.value;
                m.textContent = same ? 'Passwords match' : 'Passwords do not match';
                m.style.color = same ? '#16a34a' : '#dc2626';
            }
            pw.addEventListener('input', checkPassword);
            cpw.addEventListener('input', checkMatch);

            // Confirm Password: no copy / paste / cut / drop / right-click
            ['paste', 'copy', 'cut', 'drop', 'contextmenu'].forEach(function (ev) {
                cpw.addEventListener(ev, function (e) { e.preventDefault(); });
            });

            // eye icon
            var eyes = document.querySelectorAll('.pw-eye');
            for (var i = 0; i < eyes.length; i++) {
                eyes[i].addEventListener('click', function () {
                    var input = $(this.getAttribute('data-target'));
                    var show = input.type === 'password';
                    input.type = show ? 'text' : 'password';
                    this.classList.toggle('is-visible', show);
                    this.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
                });
            }
            checkPassword();
        })();
    </script>
</asp:Content>