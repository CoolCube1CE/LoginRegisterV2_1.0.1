using System;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace LoginRegisterV2
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserEmail"] != null)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            // computed from today's date, never hard-coded
            DateTime today = DateTime.Today;
            txtBirthDate.Attributes["max"] = today.AddYears(-18).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            txtBirthDate.Attributes["min"] = today.AddYears(-120).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static bool IsAtLeast18(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }
            return age >= 18;
        }

        // username = email, and the username column is NVARCHAR(50), so email max is 50
        private static bool IsValidEmail(string email)
        {
            if (email.Length > 50) return false;
            if (!Regex.IsMatch(email, @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9\-]+(\.[A-Za-z0-9\-]+)*\.[A-Za-z]{2,}$")) return false;
            try { return new MailAddress(email).Address == email; }
            catch { return false; }
        }

        private void ClearErrors()
        {
            lblFullNameError.Text = "";
            lblEmailError.Text = "";
            lblNumberError.Text = "";
            lblBirthDateError.Text = "";
            lblAddressError.Text = "";
            lblRoleError.Text = "";
            lblPasswordError.Text = "";
            lblConfirmPasswordError.Text = "";
            lblAgreeError.Text = "";
            lblMessage.Text = "";
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            ClearErrors();
            bool valid = true;

            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string number = txtNumber.Text.Trim();
            string address = txtAddress.Text.Trim();
            string role = ddlRole.SelectedValue;

            if (fullName == "")
            { lblFullNameError.Text = "Full name is required."; valid = false; }
            else if (fullName.Length < 2 || !Regex.IsMatch(fullName, @"^\p{L}+(?:[ '’\-]\p{L}+)*$"))
            { lblFullNameError.Text = "Full name must contain letters only. Numbers and special characters are not allowed."; valid = false; }

            DateTime birthDate;
            if (!DateTime.TryParseExact(txtBirthDate.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate))
            { lblBirthDateError.Text = "Please select your birthday."; valid = false; }
            else if (birthDate.Date > DateTime.Today)
            { lblBirthDateError.Text = "Birthday cannot be in the future."; valid = false; }
            else if (!IsAtLeast18(birthDate))
            { lblBirthDateError.Text = "You must be at least 18 years old."; valid = false; }

            if (address == "")
            { lblAddressError.Text = "Address is required."; valid = false; }
            else if (address.Length < 5 || address.IndexOfAny(new char[] { '<', '>' }) >= 0)
            { lblAddressError.Text = "Enter a valid address (at least 5 characters)."; valid = false; }

            if (email == "")
            { lblEmailError.Text = "Email is required."; valid = false; }
            else if (!IsValidEmail(email))
            { lblEmailError.Text = "That is not a valid email address (max 50 characters)."; valid = false; }

            if (number == "")
            { lblNumberError.Text = "Mobile number is required."; valid = false; }
            else if (!Regex.IsMatch(number, @"^09[0-9]{9}$") && !Regex.IsMatch(number, @"^\+639[0-9]{9}$"))
            { lblNumberError.Text = "Use 09XXXXXXXXX (11 digits) or +639XXXXXXXXX (13 characters)."; valid = false; }

            if (!AccountHelper.IsValidRole(role))
            { lblRoleError.Text = "Please select an access type."; valid = false; }

            if (txtPassword.Text == "")
            { lblPasswordError.Text = "Password is required."; valid = false; }
            else if (!AccountHelper.IsPasswordStrong(txtPassword.Text))
            { lblPasswordError.Text = AccountHelper.PasswordRulesText; valid = false; }

            if (txtConfirmPassword.Text != txtPassword.Text)
            { lblConfirmPasswordError.Text = "The two passwords do not match."; valid = false; }

            if (!chkAgree.Checked)
            { lblAgreeError.Text = "You must agree to the Data Privacy Terms and Agreement."; valid = false; }

            if (!valid)
            {
                return;
            }

            if (!SchemaMigrator.EnsureSchema())
            {
                lblMessage.Text = "The system could not finish setting up its database. Please tell your administrator.";
                return;
            }

            try
            {
                if (AccountHelper.EmailExists(email) || AccountHelper.UsernameExists(email))
                {
                    lblEmailError.Text = "Email already registered. Please use a different email address or reset your password.";
                    return;
                }

                Session["Reg_Pending"] = new PendingRegistration
                {
                    FullName = fullName,
                    Username = email,
                    Email = email,
                    Phone = number,
                    BirthDate = birthDate,
                    Address = address,
                    Role = role,
                    Password = txtPassword.Text
                };
                Session.Remove("Reg_Otp");
                Session.Remove("Reg_Expires");
                Session.Remove("Reg_Attempts");
                Session.Remove("Reg_LastSent");

                Response.Redirect("~/RegisterOTP.aspx");
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't continue your sign up right now. Please check the details and try again.";
            }
        }
    }
}