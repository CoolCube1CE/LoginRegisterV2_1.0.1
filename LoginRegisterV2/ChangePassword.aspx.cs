using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace LoginRegisterV2
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserEmail"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            // The OTP step must be completed first; opening this page directly is not enough.
            if (!(Session["ChPW_Verified"] is bool) || !(bool)Session["ChPW_Verified"])
            {
                Response.Redirect("ChangePasswordOTP.aspx");
            }
        }

        private bool IsPasswordStrong(string password)
        {
            bool length = password.Length >= 8 && password.Length <= 16;
            bool upper = Regex.IsMatch(password, @"[A-Z]");
            bool lower = Regex.IsMatch(password, @"[a-z]");
            bool number = Regex.IsMatch(password, @"[0-9]");
            bool special = Regex.IsMatch(password, @"[^a-zA-Z0-9]");
            return length && upper && lower && number && special;
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            string userEmail = Session["UserEmail"].ToString();

            if (newPassword == "")
            { lblMessage.Text = "New Password is required."; return; }

            if (!IsPasswordStrong(newPassword))
            {
                lblMessage.Text = "Password must be 8-16 characters and include an uppercase letter, lowercase letter, number, and special character.";
                return;
            }

            if (confirmPassword == "")
            { lblMessage.Text = "Confirm Password is required."; return; }

            if (newPassword != confirmPassword)
            { lblMessage.Text = "Password does not match."; return; }

            string connStr = ConfigurationManager.ConnectionStrings["LoginRegisterDB"].ConnectionString;

            using (SqlConnection sqlConnect = new SqlConnection(connStr))
            {
                try
                {
                    sqlConnect.Open();

                    // Check it's not the same as the current password
                    string checkSql = "SELECT password FROM admin WHERE email = @email";
                    using (SqlCommand checkCmd = new SqlCommand(checkSql, sqlConnect))
                    {
                        checkCmd.Parameters.AddWithValue("@email", userEmail);
                        object currentHash = checkCmd.ExecuteScalar();

                        if (currentHash != null && PasswordHelper.VerifyPassword(newPassword, currentHash.ToString()))
                        {
                            lblMessage.Text = "You cannot use your previous password. Please enter a new password.";
                            return;
                        }
                    }

                    string newHash = PasswordHelper.HashPassword(newPassword);
                    string updateSql = "UPDATE admin SET password = @password WHERE email = @email";

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, sqlConnect))
                    {
                        updateCmd.Parameters.AddWithValue("@password", newHash);
                        updateCmd.Parameters.AddWithValue("@email", userEmail);

                        int rowsAffected = updateCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Session.Clear();
                            Session["LoginNotice"] = "Your password was changed. Please log in again.";
                            Response.Redirect("Login.aspx");
                        }
                        else
                        {
                            lblMessage.Text = "Unable to change password. Account was not found.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Db.LogError(ex);
                    lblMessage.Text = "We couldn't change the password right now. Please try again.";
                }
            }
        }
    }
}