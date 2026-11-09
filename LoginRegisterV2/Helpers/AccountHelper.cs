using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace LoginRegisterV2
{
    /// <summary>The logged-in staff member's details.</summary>
    public class StaffAccount
    {
        public string FullName;
        public string Username;
        public string Email;
        public string Role;
        public DateTime? BirthDate;
        public string Address;
        public string PhoneNumber;
    }

    /// <summary>Staff accounts (table: admin). Roles: Admin and User.</summary>
    public static class AccountHelper
    {
        public const string RoleAdmin = "Admin";
        public const string RoleUser = "User";

        public static bool IsValidRole(string role)
        {
            return role == RoleAdmin || role == RoleUser;
        }

        // ------------------------------------------------------------------
        // Session / page protection
        // ------------------------------------------------------------------

        public static bool Require(Page page)
        {
            if (page.Session["UserEmail"] == null)
            {
                page.Response.Redirect("~/Login.aspx");
                return false;
            }
            return true;
        }

        public static bool IsAdmin(HttpSessionState session)
        {
            return session["AccessType"] != null && session["AccessType"].ToString() == RoleAdmin;
        }

        /// <summary>For admin-only pages (optional): not logged in -> Login, not admin -> Dashboard.</summary>
        public static bool RequireAdmin(Page page)
        {
            if (!Require(page))
            {
                return false;
            }
            if (!IsAdmin(page.Session))
            {
                page.Response.Redirect("~/Dashboard.aspx");
                return false;
            }
            return true;
        }

        public static void SignIn(HttpSessionState session, StaffAccount account)
        {
            session["UserEmail"] = account.Email;
            session["AccessType"] = account.Role;
            session["FullName"] = account.FullName;
            session["Username"] = account.Username;
        }

        public static string CurrentEmail(HttpSessionState session)
        {
            return session["UserEmail"] == null ? "" : session["UserEmail"].ToString();
        }

        // ------------------------------------------------------------------
        // Look-ups
        // ------------------------------------------------------------------

        private static string NormalizeRole(string role)
        {
            // only Admin and User exist: anything else stored is treated as Admin
            return string.Equals(role, RoleUser, StringComparison.OrdinalIgnoreCase) ? RoleUser : RoleAdmin;
        }

        private static StaffAccount FromRow(DataRow row)
        {
            StaffAccount account = new StaffAccount();
            account.FullName = row["fullname"].ToString();
            account.Email = row["email"].ToString();
            account.Username = row["username"] == DBNull.Value ? "" : row["username"].ToString();
            account.Role = NormalizeRole(row["accesstype"].ToString());
            account.Address = row["address"] == DBNull.Value ? "" : row["address"].ToString();
            account.PhoneNumber = row["number"] == DBNull.Value ? "" : row["number"].ToString();

            if (row["birthdate"] != DBNull.Value)
            {
                account.BirthDate = Convert.ToDateTime(row["birthdate"]);
            }

            return account;
        }

        public static StaffAccount GetByEmail(string email)
        {
            DataTable dt = Db.Query(
                "SELECT fullname, email, username, accesstype, address, number, birthdate FROM admin WHERE email = @email",
                Db.P("@email", email));
            return dt.Rows.Count > 0 ? FromRow(dt.Rows[0]) : null;
        }

        /// <summary>Checks an email-or-username plus password. Returns the account, or null if they don't match.</summary>
        public static StaffAccount Authenticate(string emailOrUsername, string password)
        {
            DataTable dt = Db.Query(
                @"SELECT TOP 1 fullname, email, username, accesstype, address, number, birthdate, password
                  FROM admin
                  WHERE email = @id OR username = @id
                  ORDER BY CASE WHEN email = @id THEN 0 ELSE 1 END",
                Db.P("@id", emailOrUsername));

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            string storedHash = dt.Rows[0]["password"].ToString();
            if (!PasswordHelper.VerifyPassword(password, storedHash))
            {
                return null;
            }

            return FromRow(dt.Rows[0]);
        }

        public static bool EmailExists(string email)
        {
            return Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM admin WHERE email = @v", Db.P("@v", email))) > 0;
        }

        public static bool UsernameExists(string username)
        {
            return Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM admin WHERE username = @v", Db.P("@v", username))) > 0;
        }

        // ------------------------------------------------------------------
        // Create / password
        // ------------------------------------------------------------------

        /// <summary>Creates a staff account. Returns null on success, or a user-facing message.</summary>
        public static string CreateAccount(string fullName, string username, string email, string phone, DateTime birthDate, string address, string role, string password)
        {
            if (!IsValidRole(role))
            {
                return "Please select a valid access type.";
            }
            if (EmailExists(email))
            {
                return "Email already registered. Please use a different email address or reset your password.";
            }
            if (UsernameExists(username))
            {
                return "That username is already taken.";
            }

            try
            {
                Db.Execute(
                    @"INSERT INTO admin (fullname, birthdate, address, email, number, accesstype, password, username)
                      VALUES (@fullname, @birthdate, @address, @email, @number, @role, @password, @username)",
                    Db.P("@fullname", fullName),
                    Db.P("@birthdate", birthDate),
                    Db.P("@address", address),
                    Db.P("@email", email),
                    Db.P("@number", phone),
                    Db.P("@role", role),
                    Db.P("@password", PasswordHelper.HashPassword(password)),
                    Db.P("@username", username));
                return null;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    return "Email already registered. Please use a different email address or reset your password.";
                }
                throw;
            }
        }

        public static bool UpdatePassword(string email, string newPassword)
        {
            int rows = Db.Execute("UPDATE admin SET password = @p WHERE email = @email",
                Db.P("@p", PasswordHelper.HashPassword(newPassword)), Db.P("@email", email));
            return rows > 0;
        }

        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            bool length = password.Length >= 8 && password.Length <= 16;
            bool upper = Regex.IsMatch(password, "[A-Z]");
            bool lower = Regex.IsMatch(password, "[a-z]");
            bool number = Regex.IsMatch(password, "[0-9]");
            bool special = Regex.IsMatch(password, "[^a-zA-Z0-9]");
            return length && upper && lower && number && special;
        }

        public const string PasswordRulesText = "Password must be 8-16 characters with an uppercase letter, a lowercase letter, a number and a special character.";

        /// <summary>A 6-digit code from a cryptographically secure generator.</summary>
        public static int GenerateOtp()
        {
            byte[] bytes = new byte[4];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            uint value = BitConverter.ToUInt32(bytes, 0);
            return (int)(value % 900000) + 100000;
        }


            // ---------------- Login lockout (3 wrong tries = 5 minutes) ----------------
public const int MaxLoginAttempts = 3;
        public const int LockMinutes = 5;

        private class LoginState
        {
            public int Failed;
            public DateTime? LockedUntil;
        }

        private static readonly object LoginLock = new object();

        private static string LoginKey(string identifier)
        {
            return "login-fail:" + (identifier ?? "").Trim().ToLowerInvariant();
        }

        /// <summary>Seconds left in the lockout for this email/username (0 = not locked).</summary>
        public static int LockSecondsLeft(string identifier)
        {
            LoginState s = HttpRuntime.Cache[LoginKey(identifier)] as LoginState;
            if (s == null || !s.LockedUntil.HasValue)
            {
                return 0;
            }
            double left = (s.LockedUntil.Value - DateTime.Now).TotalSeconds;
            return left > 0 ? (int)Math.Ceiling(left) : 0;
        }

        /// <summary>Counts a wrong password. Returns attempts left; 0 means the login is now blocked.</summary>
        public static int RegisterFailedLogin(string identifier)
        {
            string key = LoginKey(identifier);
            lock (LoginLock)
            {
                LoginState s = HttpRuntime.Cache[key] as LoginState ?? new LoginState();

                if (s.LockedUntil.HasValue && s.LockedUntil.Value <= DateTime.Now)
                {
                    s.LockedUntil = null;   // old lockout is over, start fresh
                    s.Failed = 0;
                }

                s.Failed++;
                if (s.Failed >= MaxLoginAttempts)
                {
                    s.LockedUntil = DateTime.Now.AddMinutes(LockMinutes);
                    s.Failed = 0;
                }

                HttpRuntime.Cache.Insert(key, s, null, DateTime.Now.AddMinutes(LockMinutes + 10),
                    System.Web.Caching.Cache.NoSlidingExpiration);

                return s.LockedUntil.HasValue ? 0 : MaxLoginAttempts - s.Failed;
            }
        }

        public static void ClearFailedLogins(string identifier)
        {
            HttpRuntime.Cache.Remove(LoginKey(identifier));
        }
    }
}
