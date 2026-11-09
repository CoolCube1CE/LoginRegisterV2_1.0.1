using System;
using System.Web.UI;

namespace LoginRegisterV2
{
    public partial class Profile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AccountHelper.Require(this))
            {
                return;
            }

            if (IsPostBack)
            {
                return;
            }

            try
            {
                StaffAccount account = AccountHelper.GetByEmail(AccountHelper.CurrentEmail(Session));

                if (account == null)
                {
                    lblMessage.Text = "We couldn't find your account details.";
                    return;
                }

                lblFullName.Text = Server.HtmlEncode(account.FullName);
                lblInitials.Text = Server.HtmlEncode(Initials(account.FullName));
                lblRole.Text = Server.HtmlEncode(account.Role);
                lblRoleDetail.Text = Server.HtmlEncode(account.Role);
                lblUsername.Text = Server.HtmlEncode(string.IsNullOrEmpty(account.Username) ? "-" : account.Username);
                lblEmail.Text = Server.HtmlEncode(account.Email);
                lblPhone.Text = Server.HtmlEncode(string.IsNullOrEmpty(account.PhoneNumber) ? "-" : account.PhoneNumber);
                lblBirthDate.Text = account.BirthDate.HasValue ? account.BirthDate.Value.ToString("MMMM d, yyyy") : "-";
                lblAddress.Text = Server.HtmlEncode(string.IsNullOrEmpty(account.Address) ? "-" : account.Address);
            }
            catch (Exception ex)
            {
                Db.LogError(ex);
                lblMessage.Text = "We couldn't load your profile right now. Please try again.";
            }
        }

        private static string Initials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "?";
            }

            string[] parts = fullName.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string first = parts[0].Substring(0, 1);
            string last = parts.Length > 1 ? parts[parts.Length - 1].Substring(0, 1) : "";
            return (first + last).ToUpperInvariant();
        }
    }
}
