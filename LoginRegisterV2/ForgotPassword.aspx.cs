using System;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace LoginRegisterV2
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        private const int CodeMinutes = 10;
        private const int MaxCodeAttempts = 5;
        private const int ResendSeconds = 30;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ClearResetSession();
                ShowStep(1);
            }
        }

        private void ClearResetSession()
        {
            Session.Remove("FP_Email");
            Session.Remove("FP_Otp");
            Session.Remove("FP_Expires");
            Session.Remove("FP_Attempts");
            Session.Remove("FP_Verified");
            Session.Remove("FP_LastSent");
        }

        private void ShowStep(int step)
        {
            pnlEmail.Visible = step == 1;
            pnlCode.Visible = step == 2;
            pnlReset.Visible = step == 3;
        }

        private static string MaskEmail(string email)
        {
            int at = email.IndexOf('@');
            if (at <= 1)
            {
                return email;
            }
            return email.Substring(0, 1) + new string('*', Math.Min(at - 1, 6)) + email.Substring(at);
        }

        // Creates a new code, e-mails it, and remembers it on the server (never sent to the browser)
        private bool SendCode(string email)
        {
            int otp = AccountHelper.GenerateOtp();

            bool sent = EmailHelper.SendOtpEmail(email, otp,
                "Password Reset Code",
                "We received a request to reset the password of your Beverage Store staff account.");

            if (!sent)
            {
                return false;
            }

            Session["FP_Email"] = email;
            Session["FP_Otp"] = otp;
            Session["FP_Expires"] = DateTime.Now.AddMinutes(CodeMinutes);
            Session["FP_Attempts"] = 0;
            Session["FP_LastSent"] = DateTime.Now;
            Session.Remove("FP_Verified");
            return true;
        }

        protected void btnSendCode_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";

            string email = txtEmail.Text.Trim();

            if (email == "")
            {
                lblMessage.Text = "Please enter your registered email address.";
                return;
            }
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$"))
            {
                lblMessage.Text = "That doesn't look like a valid email address.";
                return;
            }

            StaffAccount account;
            try
            {
                account = AccountHelper.GetByEmail(email);
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't check that email right now. Please try again in a moment.";
                return;
            }

            if (account == null)
            {
                lblMessage.Text = "We couldn't find a staff account with that email address.";
                return;
            }

            if (!SendCode(account.Email))
            {
                lblMessage.Text = "We couldn't send the verification email right now. Please try again later, or ask an Admin to check the email settings.";
                return;
            }

            lblSuccess.Text = "We sent a 6-digit code to " + Server.HtmlEncode(MaskEmail(account.Email)) + ".";
            ShowStep(2);
        }

        protected void btnResend_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";

            string email = Session["FP_Email"] as string;
            if (string.IsNullOrEmpty(email))
            {
                lblMessage.Text = "Please start again by entering your email address.";
                ShowStep(1);
                return;
            }

            DateTime? lastSent = Session["FP_LastSent"] as DateTime?;
            if (lastSent.HasValue && (DateTime.Now - lastSent.Value).TotalSeconds < ResendSeconds)
            {
                lblMessage.Text = "Please wait a few seconds before asking for another code.";
                ShowStep(2);
                return;
            }

            if (!SendCode(email))
            {
                lblMessage.Text = "We couldn't send the verification email right now. Please try again later.";
                ShowStep(2);
                return;
            }

            lblSuccess.Text = "A new code was sent to " + Server.HtmlEncode(MaskEmail(email)) + ".";
            ShowStep(2);
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";

            if (Session["FP_Otp"] == null || Session["FP_Expires"] == null)
            {
                lblMessage.Text = "Your code is no longer valid. Please request a new one.";
                ShowStep(1);
                return;
            }

            DateTime expires = (DateTime)Session["FP_Expires"];
            int attempts = Session["FP_Attempts"] == null ? 0 : (int)Session["FP_Attempts"];

            if (DateTime.Now > expires)
            {
                ClearResetSession();
                lblMessage.Text = "That code has expired. Please request a new one.";
                ShowStep(1);
                return;
            }

            if (attempts >= MaxCodeAttempts)
            {
                ClearResetSession();
                lblMessage.Text = "Too many wrong codes. Please request a new one.";
                ShowStep(1);
                return;
            }

            string entered = txtOtp.Text.Trim();
            string expected = ((int)Session["FP_Otp"]).ToString();

            if (entered != expected)
            {
                Session["FP_Attempts"] = attempts + 1;
                lblMessage.Text = "That code is not correct. Please check the email and try again.";
                ShowStep(2);
                return;
            }

            Session["FP_Verified"] = true;
            Session.Remove("FP_Otp");
            lblSuccess.Text = "Code verified. Choose your new password.";
            ShowStep(3);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";

            string email = Session["FP_Email"] as string;
            bool verified = Session["FP_Verified"] is bool && (bool)Session["FP_Verified"];

            if (!verified || string.IsNullOrEmpty(email))
            {
                ClearResetSession();
                lblMessage.Text = "Please verify your email first.";
                ShowStep(1);
                return;
            }

            string newPassword = txtNewPassword.Text;

            if (!AccountHelper.IsPasswordStrong(newPassword))
            {
                lblMessage.Text = AccountHelper.PasswordRulesText;
                ShowStep(3);
                return;
            }

            if (newPassword != txtConfirmPassword.Text)
            {
                lblMessage.Text = "The two passwords do not match.";
                ShowStep(3);
                return;
            }

            try
            {
                if (!AccountHelper.UpdatePassword(email, newPassword))
                {
                    lblMessage.Text = "We couldn't find that account any more.";
                    ShowStep(1);
                    return;
                }
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't save the new password right now. Please try again.";
                ShowStep(3);
                return;
            }

            ClearResetSession();
            Session["LoginNotice"] = "Your password was reset. You can now log in with your new password.";
            Response.Redirect("~/Login.aspx");
        }
    }
}
