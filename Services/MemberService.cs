
using Member_App.Models;
using System.Data;
using Microsoft.Data.SqlClient;



namespace Member_App.Services
{
    public class MemberService : IMemberService
    {
        
            private readonly string _connectionString;
            private readonly IWebHostEnvironment _webHostEnvironment;

            // The constructor now receives dependencies from the framework.
            public MemberService(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
            {
                // We get the connection string from appsettings.json, not hardcoded.
                _connectionString = configuration.GetConnectionString("DefaultConnection");
                _webHostEnvironment = webHostEnvironment;
            }

            public async Task<List<Member>> GetAllMembersAsync()
            {
                var members = new List<Member>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("dbo.GetAllRecords", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            members.Add(new Member
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = reader["Name"].ToString(),
                                Address = reader["Address"].ToString(),
                                Department = reader["Department"].ToString(),
                                Unique_ID = reader["Unique_ID"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                Image = reader["Image"].ToString()
                            });
                        }
                    }
                }
                return members;
            }

            public async Task<Member?> GetMemberByIdAsync(int id)
            {
                Member? member = null;
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("dbo.GetMemberById", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddWithValue("@ID", id);
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            member = new Member
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = reader["Name"].ToString(),
                                Address = reader["Address"].ToString(),
                                Department = reader["Department"].ToString(),
                                Unique_ID = reader["Unique_ID"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                Image = reader["Image"].ToString()
                            };
                        }
                    }
                }
                return member;
            }

            public async Task<string> SaveMemberAsync(Member member, IFormFile? file)
            {
                string? imagePath = await HandleFileUploadAsync(file);

                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("dbo.InsertMember", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    command.Parameters.AddWithValue("@Name", member.Name);
                    command.Parameters.AddWithValue("@Address", member.Address);
                    command.Parameters.AddWithValue("@Department", member.Department);
                    command.Parameters.AddWithValue("@Unique_ID", member.Unique_ID);
                    command.Parameters.AddWithValue("@Email", member.Email);
                    command.Parameters.AddWithValue("@Phone", member.Phone);
                    // Handle null image path correctly
                    command.Parameters.AddWithValue("@Image", (object)imagePath ?? DBNull.Value);

                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0 ? "Member saved successfully!" : "Failed to save member.";
                }
            }

        public async Task<string> UpdateMemberAsync(int id, Member member, IFormFile? file)
        {
            string imagePath = member.Image; // পুরানো ছবি

            if (file != null && file.Length > 0)
            {
                string? uploadedPath = await HandleFileUploadAsync(file);
                if (!string.IsNullOrEmpty(uploadedPath))
                {
                    imagePath = uploadedPath;
                }
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("UpdateMember", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@Name", member.Name);
                    cmd.Parameters.AddWithValue("@Address", member.Address);
                    cmd.Parameters.AddWithValue("@Department", member.Department);
                    cmd.Parameters.AddWithValue("@Unique_ID", member.Unique_ID);
                    cmd.Parameters.AddWithValue("@Email", member.Email);
                    cmd.Parameters.AddWithValue("@Phone", member.Phone);
                    cmd.Parameters.AddWithValue("@Image", imagePath);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            return "Member updated successfully.";
        }


        public async Task DeleteMemberAsync(int id)
        {
            // 1. Member এর তথ্য বের করো
            var member = await GetMemberByIdAsync(id);

            if (member != null && !string.IsNullOrEmpty(member.Image))
            {
                string relativePath = member.Image.TrimStart('/'); // "/Images/no-photo.png" → "Images/no-photo.png"
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

                // যদি এটি "no-photo.png" না হয়, তাহলে ডিলিট করো
                if (!relativePath.EndsWith("no-photo.png", StringComparison.OrdinalIgnoreCase))
                {
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath); // ✅ কাস্টম ইমেজ ডিলিট
                    }
                }
            }

            // 3. তারপর ডাটাবেজ থেকে মেম্বার রেকর্ড ডিলিট করো
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("DeleteMember", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ID", id);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }


        // Helper method to handle file uploads
        private async Task<string?> HandleFileUploadAsync(IFormFile? file)
            {
                if (file == null || file.Length == 0)
                {
                    return null;
                }

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/Images/{uniqueFileName}"; // Relative path for use in HTML
            }
        }
    }