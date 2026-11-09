using System;
using System.Web.UI;

namespace LoginRegisterV2
{
    /// <summary>Held in Session between Register.aspx and RegisterOTP.aspx (server-side only).</summary>
    [Serializable]
    public class PendingRegistration
    {
        public string FullName;
        public string Username;
        public string Email;
        public string Phone;
        public DateTime BirthDate;
        public string Address;
        public string Role;
        public string Password;
    }

    public partial class RegisterOTP : System.Web.UI.Page
    {
        private const int CodeMinutes = 10;
        private const int MaxCodeAttempts = 5;
        private const int ResendSeconds = 30;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserEmail"] != null)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            PendingRegistration pending = Session["Reg_Pending"] as PendingRegistration;
            if (pending == null)
            {
                Response.Redirect("~/Register.aspx");
                return;
            }

            lblSentTo.Text = "We sent a 6-digit code to " + Server.HtmlEncode(MaskEmail(pending.Email)) + ".";

            if (!IsPostBack && Session["Reg_Otp"] == null)
            {
                if (!SendCode(pending.Email))
                {
                    lblMessage.Text = "We couldn't send the verification email right now. Please try again later.";
                }
            }
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

        private void ClearRegistrationSession()
        {
            Session.Remove("Reg_Pending");
            Session.Remove("Reg_Otp");
            Session.Remove("Reg_Expires");
            Session.Remove("Reg_Attempts");
            Session.Remove("Reg_LastSent");
        }

        // a new code replaces the old one, so the previous OTP stops working
        private bool SendCode(string email)
        {
            int otp = AccountHelper.GenerateOtp();

            bool sent = EmailHelper.SendOtpEmail(email, otp,
                "Beverage Store Sign Up Code",
                "Thank you for signing up for the Beverage Store staff system. Enter this code to finish creating your account.");

            if (!sent)
            {
                return false;
            }

            Session["Reg_Otp"] = otp;
            Session["Reg_Expires"] = DateTime.Now.AddMinutes(CodeMinutes);
            Session["Reg_Attempts"] = 0;
            Session["Reg_LastSent"] = DateTime.Now;
            return true;
        }

        protected void btnResend_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";

            PendingRegistration pending = Session["Reg_Pending"] as PendingRegistration;
            if (pending == null)
            {
                Response.Redirect("~/Register.aspx");
                return;
            }

            DateTime? lastSent = Session["Reg_LastSent"] as DateTime?;
            if (lastSent.HasValue && (DateTime.Now - lastSent.Value).TotalSeconds < ResendSeconds)
            {
                lblMessage.Text = "Please wait a few seconds before asking for another code.";
                return;
            }

            if (!SendCode(pending.Email))
            {
                lblMessage.Text = "We couldn't send the verification email right now. Please try again later.";
                return;
            }

            lblSuccess.Text = "A new code was sent to " + Server.HtmlEncode(MaskEmail(pending.Email)) + ".";
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblSuccess.Text = "";

            PendingRegistration pending = Session["Reg_Pending"] as PendingRegistration;
            if (pending == null)
            {
                Response.Redirect("~/Register.aspx");
                return;
            }

            if (Session["Reg_Otp"] == null || Session["Reg_Expires"] == null)
            {
                lblMessage.Text = "Your code is no longer valid. Please request a new one.";
                return;
            }

            if (DateTime.Now > (DateTime)Session["Reg_Expires"])
            {
                Session.Remove("Reg_Otp");
                lblMessage.Text = "That code has expired. Please request a new one.";
                return;
            }

            int attempts = Session["Reg_Attempts"] == null ? 0 : (int)Session["Reg_Attempts"];
            if (attempts >= MaxCodeAttempts)
            {
                Session.Remove("Reg_Otp");
                lblMessage.Text = "Too many wrong codes. Please request a new one.";
                return;
            }

            string entered = txtOtp.Text.Trim();
            string expected = ((int)Session["Reg_Otp"]).ToString();

            if (entered != expected)
            {
                Session["Reg_Attempts"] = attempts + 1;
                lblMessage.Text = "That code is not correct. Please check the email and try again.";
                return;
            }

            try
            {
                string error = AccountHelper.CreateAccount(pending.FullName, pending.Username, pending.Email,
                    pending.Phone, pending.BirthDate, pending.Address, pending.Role, pending.Password);

                if (error != null)
                {
                    lblMessage.Text = Server.HtmlEncode(error);
                    return;
                }

                ClearRegistrationSession();
                Session["LoginNotice"] = "Account Created Successfully!";
                Response.Redirect("~/Login.aspx");
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't create your account right now. Please try again.";
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            ClearRegistrationSession();
            Response.Redirect("~/Register.aspx");
        }
    }
}