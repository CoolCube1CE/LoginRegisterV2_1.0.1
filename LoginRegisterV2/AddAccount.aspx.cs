using System;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace LoginRegisterV2
{
    public partial class AddAccount : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AccountHelper.RequireAdmin(this))
            {
                return;
            }
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

        private void ClearErrors()
        {
            lblFullNameError.Text = "";
            lblUsernameError.Text = "";
            lblEmailError.Text = "";
            lblNumberError.Text = "";
            lblBirthDateError.Text = "";
            lblRoleError.Text = "";
            lblAddressError.Text = "";
            lblPasswordError.Text = "";
            lblConfirmPasswordError.Text = "";
            lblMessage.Text = "";
            lblSuccess.Text = "";
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            ClearErrors();
            bool valid = true;

            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string number = txtNumber.Text.Trim();
            string address = txtAddress.Text.Trim();
            string role = ddlRole.SelectedValue;

            if (fullName == "")
            { lblFullNameError.Text = "Full name is required."; valid = false; }
            else if (fullName.Length < 3 || !Regex.IsMatch(fullName, @"^[a-zA-Z .,'-]+$"))
            { lblFullNameError.Text = "Use letters, spaces, periods and commas only (at least 3 characters)."; valid = false; }

            if (username == "")
            { lblUsernameError.Text = "Username is required."; valid = false; }
            else if (!Regex.IsMatch(username, @"^[a-zA-Z0-9._-]{3,30}$"))
            { lblUsernameError.Text = "Use 3-30 letters, numbers, dot, dash or underscore."; valid = false; }

            if (email == "")
            { lblEmailError.Text = "Email is required."; valid = false; }
            else if (!Regex.IsMatch(email, @"^[a-zA-Z0-9.!#$%&'*+\-/=?^_`{|}~]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            { lblEmailError.Text = "That is not a valid email address."; valid = false; }

            if (number == "")
            { lblNumberError.Text = "Phone number is required."; valid = false; }
            else if (!Regex.IsMatch(number, @"^09[0-9]{9}$") && !Regex.IsMatch(number, @"^\+639[0-9]{9}$"))
            { lblNumberError.Text = "Use 09XXXXXXXXX or +639XXXXXXXXX."; valid = false; }

            DateTime birthDate;
            if (!DateTime.TryParse(txtBirthDate.Text, out birthDate))
            { lblBirthDateError.Text = "Birth date is required."; valid = false; }
            else if (!IsAtLeast18(birthDate))
            { lblBirthDateError.Text = "Staff must be at least 18 years old."; valid = false; }

            if (role != AccountHelper.RoleAdmin && role != AccountHelper.RoleUser)
            { lblRoleError.Text = "Choose Admin or User."; valid = false; }

            if (address == "")
            { lblAddressError.Text = "Address is required."; valid = false; }
            else if (address.Length < 5 || address.IndexOfAny(new char[] { '<', '>' }) >= 0)
            { lblAddressError.Text = "Enter a valid address (at least 5 characters)."; valid = false; }

            if (txtPassword.Text == "")
            { lblPasswordError.Text = "Password is required."; valid = false; }
            else if (!AccountHelper.IsPasswordStrong(txtPassword.Text))
            { lblPasswordError.Text = AccountHelper.PasswordRulesText; valid = false; }

            if (txtConfirmPassword.Text != txtPassword.Text)
            { lblConfirmPasswordError.Text = "The two passwords do not match."; valid = false; }

            if (!valid)
            {
                return;
            }

            try
            {
                string error = AccountHelper.CreateAccount(fullName, username, email, number, birthDate, address, role, txtPassword.Text);

                if (error != null)
                {
                    lblMessage.Text = Server.HtmlEncode(error);
                    return;
                }

                lblSuccess.Text = "Account created for " + Server.HtmlEncode(fullName) + " (" + role + "). They can log in with their email or username.";

                txtFullName.Text = "";
                txtUsername.Text = "";
                txtEmail.Text = "";
                txtNumber.Text = "";
                txtBirthDate.Text = "";
                txtAddress.Text = "";
                ddlRole.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't create the account right now. Please check the details and try again.";
            }
        }
    }
}
