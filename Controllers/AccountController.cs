using Member_App.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Member_App.Controllers
{
    public class AccountController : Controller
    {
        private readonly string connectionString = "Data Source=localhost\\SQLEXPRESS01;Initial Catalog=member;Trusted_Connection=True;TrustServerCertificate=True";

        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(Login model)
        {


            if (ModelState.IsValid)
            {
                await using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();
                    string query = "SELECT Id, Phone, PasswordHash, RoleId, IsApproved FROM Users WHERE Phone = @Phone AND IsApproved = 1";
                    await using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Phone", model.Phone);
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                string storedPasswordHash = reader["PasswordHash"].ToString();
                                int roleId = (int)reader["RoleId"];
                                string role = roleId == 1 ? "Admin" : "User";

                                if (VerifyPasswordHash(model.PasswordHash, storedPasswordHash))
                                {
                                    string phone = reader["Phone"].ToString();

                                    var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, phone),
                            new Claim("phone", phone),
                            new Claim(ClaimTypes.Role, role)
                        };

                                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                    var principal = new ClaimsPrincipal(identity);

                                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


                                    return RedirectToAction("Index", "Home");
                                }
                            }
                        }

                        ViewBag.Error = "Invalid credentials or not approved yet.";
                    }
                }
            }

            return View(model);
        }

        private bool VerifyPasswordHash(string inputPassword, string storedPasswordHash)
        {
            string hashedInput = HashPassword(inputPassword);
            return hashedInput == storedPasswordHash;
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User model, int roleId = 2)
        {
            if (ModelState.IsValid)
            {
                model.PasswordHash = HashPassword(model.PasswordHash);

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "INSERT INTO Users (Phone, PasswordHash, IsApproved, RoleId) VALUES (@Phone, @PasswordHash, 0, @RoleId)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Phone", model.Phone);
                        cmd.Parameters.AddWithValue("@PasswordHash", model.PasswordHash);
                        cmd.Parameters.AddWithValue("@RoleId", roleId);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            ViewBag.Success = "Registration successful. Please wait for admin approval.";
                        }
                        else
                        {
                            ViewBag.Error = "Error occurred during registration.";
                        }
                    }
                }
            }

            return View(model);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ApproveUsers()
        {
            List<User> users = new List<User>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT Id, Phone, IsApproved FROM Users WHERE IsApproved = 0 AND IsDenied = 0";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetInt32(0),
                            Phone = reader.GetString(1),
                            IsApproved = reader.GetBoolean(2)
                        });
                    }
                }
            }
            return View(users);
        }

        [HttpPost]
        public IActionResult UpdateUserApproval(int id, bool isApproved)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = isApproved
                    ? "UPDATE Users SET IsApproved = 1 WHERE Id = @Id"
                    : "UPDATE Users SET IsApproved = 0, IsDenied = 1 WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int result = cmd.ExecuteNonQuery();

                    TempData["Success"] = isApproved ? "User approved successfully." : "User denied successfully.";
                }
            }
            return RedirectToAction("ApproveUsers");
        }

        // GET: ForgotPassword Page
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
    
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string checkUser = "SELECT Id FROM Users WHERE Phone=@Phone";
                object result;
                using (SqlCommand cmd = new SqlCommand(checkUser, con))
                {
                    cmd.Parameters.AddWithValue("@Phone", model.Phone);
                    result = cmd.ExecuteScalar();
                }

                if (result == null)
                {
                    ViewBag.Error = "This phone number is not registered.";
                    return View(model);
                }

                int userId = Convert.ToInt32(result);

                // Generate Reset Token
                string token = Guid.NewGuid().ToString();
                DateTime expiry = DateTime.Now.AddMinutes(15);

                // Save token in DB
                string updateToken = @"UPDATE Users 
                               SET ResetToken=@Token, ResetTokenExpiry=@Expiry 
                               WHERE Id=@Id";

                using (SqlCommand cmd2 = new SqlCommand(updateToken, con))
                {
                    cmd2.Parameters.AddWithValue("@Token", token);
                    cmd2.Parameters.AddWithValue("@Expiry", expiry);
                    cmd2.Parameters.AddWithValue("@Id", userId);
                    cmd2.ExecuteNonQuery();
                }

                // ✅ Direct ResetPassword Page এ Redirect (token querystring ছাড়াই)
                return RedirectToAction("ResetPassword", new { token = token });
            }
        }




        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            // Hidden field এ token পাঠাবো
            return View(new ResetPasswordViewModel { Token = token });
        }


        // RESET PASSWORD POST
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string checkToken = "SELECT Id FROM Users WHERE ResetToken=@Token AND ResetTokenExpiry > GETDATE()";
                int userId = 0;
                using (SqlCommand cmd = new SqlCommand(checkToken, con))
                {
                    cmd.Parameters.AddWithValue("@Token", model.Token);
                    var result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        ViewBag.Error = "Invalid or expired token.";
                        return View(model);
                    }
                    userId = Convert.ToInt32(result);
                }

                // Hash new password
                string newHashedPassword = HashPassword(model.NewPassword);

                // Reset password and require admin approval again
                string resetQuery = @"UPDATE Users 
                              SET PasswordHash=@Password,
                                  IsApproved=0,
                                  IsDenied=0,
                                  ResetToken=NULL,
                                  ResetTokenExpiry=NULL
                              WHERE Id=@Id";

                using (SqlCommand cmd2 = new SqlCommand(resetQuery, con))
                {
                    cmd2.Parameters.AddWithValue("@Password", newHashedPassword);
                    cmd2.Parameters.AddWithValue("@Id", userId);
                    cmd2.ExecuteNonQuery();
                }

                ViewBag.Success = "Password reset successfully. Please wait for admin approval again.";
            }

            return View(model);
        }
    }
}



    





