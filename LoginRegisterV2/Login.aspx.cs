using System;
using System.Web;
using System.Web.UI;

namespace LoginRegisterV2
{
    public partial class Login : System.Web.UI.Page
    {
        private const string RememberCookie = "BSMS_RememberEmail";

        protected void Page_Load(object sender, EventArgs e)
        {
            // arrived from the inactivity timer: end the old session first
            bool timedOut = Request.QueryString["timeout"] == "1";
            if (timedOut)
            {
                Session.Clear();
            }

            if (!timedOut && Session["UserEmail"] != null)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (timedOut)
                {
                    lblMessage.Text = "You were logged out because of inactivity. Please log in again.";
                }

                string notice = Session["LoginNotice"] as string;
                if (!string.IsNullOrEmpty(notice))
                {
                    lblSuccess.Text = Server.HtmlEncode(notice);
                    Session.Remove("LoginNotice");
                }

                // Remember Me: only the email is remembered (never the password or the session)
                HttpCookie remembered = Request.Cookies[RememberCookie];
                if (remembered != null && !string.IsNullOrEmpty(remembered.Value) && remembered.Value.Length <= 100)
                {
                    txtEmail.Text = remembered.Value;
                    chkRemember.Checked = true;
                }

                if (!SchemaMigrator.EnsureSchema())
                {
                    lblMessage.Text = "The system could not finish setting up its database. Please tell your administrator (details are saved in App_Data/error.log).";
                }
            }
        }

        private static string LockedMessage(int secondsLeft)
        {
            int minutes = (int)Math.Ceiling(secondsLeft / 60.0);
            return "Too many unsuccessful login attempts. Your login has been temporarily blocked. Please try again in "
                   + minutes + (minutes == 1 ? " minute." : " minutes.");
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";

            string identifier = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (identifier == "" || password == "")
            {
                lblMessage.Text = "Please enter your email (or username) and password.";
                return;
            }

            // blocked? stop here, even if the password would have been correct
            int wait = AccountHelper.LockSecondsLeft(identifier);
            if (wait > 0)
            {
                lblMessage.Text = LockedMessage(wait);
                return;
            }

            if (!SchemaMigrator.EnsureSchema())
            {
                lblMessage.Text = "The system could not finish setting up its database. Please tell your administrator.";
                return;
            }

            StaffAccount account;
            try
            {
                account = AccountHelper.Authenticate(identifier, password);
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't sign you in right now. Please try again in a moment.";
                return;
            }

            if (account == null)
            {
                int left = AccountHelper.RegisterFailedLogin(identifier);
                if (left == 0)
                {
                    lblMessage.Text = LockedMessage(AccountHelper.LockMinutes * 60);
                }
                else
                {
                    lblMessage.Text = "Incorrect email/username or password. " + left
                        + (left == 1 ? " attempt left." : " attempts left.");
                }
                return;
            }

            AccountHelper.ClearFailedLogins(identifier);

            if (chkRemember.Checked)
            {
                HttpCookie cookie = new HttpCookie(RememberCookie, account.Email);
                cookie.HttpOnly = true;
                cookie.Secure = Request.IsSecureConnection;
                cookie.SameSite = SameSiteMode.Lax;
                cookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(cookie);
            }
            else if (Request.Cookies[RememberCookie] != null)
            {
                HttpCookie old = new HttpCookie(RememberCookie, "");
                old.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(old);
            }

            Session.Clear();
            AccountHelper.SignIn(Session, account);
            Response.Redirect("~/Dashboard.aspx");
        }
    }
}