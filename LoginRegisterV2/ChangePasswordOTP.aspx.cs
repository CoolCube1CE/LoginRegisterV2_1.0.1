using System;
using System.Web.UI;

namespace LoginRegisterV2
{
    public partial class ChangePasswordOTP : System.Web.UI.Page
    {
        private const int CodeMinutes = 10;
        private const int MaxCodeAttempts = 5;
        private const int ResendSeconds = 30;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserEmail"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                Session.Remove("ChPW_Verified");

                // send a code only if there isn't a still-valid one (a page refresh won't spam emails)
                bool hasFreshCode = Session["ChPW_OTP"] != null && Session["ChPW_Expires"] is DateTime
                                    && DateTime.Now <= (DateTime)Session["ChPW_Expires"];
                if (!hasFreshCode)
                {
                    SendOtp();
                }
            }
        }

        private void ClearOtpSession()
        {
            Session.Remove("ChPW_OTP");
            Session.Remove("ChPW_Expires");
            Session.Remove("ChPW_Attempts");
            Session.Remove("ChPW_LastSent");
        }

        // a new code replaces the old one, so the previous OTP stops working
        private bool SendOtp()
        {
            int otp = AccountHelper.GenerateOtp();

            bool sent = EmailHelper.SendOtpEmail(
                Session["UserEmail"].ToString(), otp,
                "Password Change Verification",
                "Please enter this code to continue changing your password.");

            if (!sent)
            {
                lblMessage.Text = "We couldn't send the verification email right now. Please try again later.";
                return false;
            }

            Session["ChPW_OTP"] = otp;
            Session["ChPW_Expires"] = DateTime.Now.AddMinutes(CodeMinutes);
            Session["ChPW_Attempts"] = 0;
            Session["ChPW_LastSent"] = DateTime.Now;
            return true;
        }

        protected void btnResend_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";

            DateTime? lastSent = Session["ChPW_LastSent"] as DateTime?;
            if (lastSent.HasValue && (DateTime.Now - lastSent.Value).TotalSeconds < ResendSeconds)
            {
                lblMessage.Text = "Please wait a few seconds before asking for another code.";
                return;
            }

            if (SendOtp())
            {
                lblSuccess.Text = "A new code was sent to your email.";
            }
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";

            if (Session["ChPW_OTP"] == null || !(Session["ChPW_Expires"] is DateTime))
            {
                lblMessage.Text = "Your code is no longer valid. Please request a new one.";
                return;
            }

            if (DateTime.Now > (DateTime)Session["ChPW_Expires"])
            {
                Session.Remove("ChPW_OTP");
                lblMessage.Text = "That code has expired. Please request a new one.";
                return;
            }

            int attempts = Session["ChPW_Attempts"] == null ? 0 : (int)Session["ChPW_Attempts"];
            if (attempts >= MaxCodeAttempts)
            {
                Session.Remove("ChPW_OTP");
                lblMessage.Text = "Too many wrong codes. Please request a new one.";
                return;
            }

            if (txtOtp.Text.Trim() != ((int)Session["ChPW_OTP"]).ToString())
            {
                Session["ChPW_Attempts"] = attempts + 1;
                lblMessage.Text = "That code is not correct. Please check the email and try again.";
                return;
            }

            // ChangePassword.aspx requires this flag
            Session["ChPW_Verified"] = true;
            ClearOtpSession();
            Response.Redirect("ChangePassword.aspx");
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            ClearOtpSession();
            Response.Redirect("Profile.aspx");
        }
    }
}