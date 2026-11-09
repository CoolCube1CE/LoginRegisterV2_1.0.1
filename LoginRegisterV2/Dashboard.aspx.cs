using System;
using System.Web.UI;

namespace LoginRegisterV2
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AccountHelper.Require(this))
            {
                return;
            }

            string fullName = Session["FullName"] == null ? "" : Session["FullName"].ToString().Trim();
            if (fullName.Length > 0)
            {
                string firstName = fullName.Split(' ')[0];
                lblWelcome.Text = "Welcome back, " + Server.HtmlEncode(firstName) + "!";
            }

            lblUserEmail.Text = Server.HtmlEncode(AccountHelper.CurrentEmail(Session));
        }
    }
}
