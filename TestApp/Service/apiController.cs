using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TestApp.Models;

namespace TestApp.Service
{
    [ApiController]
    public class apiController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly dbServices _dbServices;
        private readonly commonFunctions _commonFunctions;

        public apiController(IConfiguration config, dbServices dbServices, commonFunctions commonFunctions)
        {
            _config = config;
            _dbServices = dbServices;
            _commonFunctions = commonFunctions;
        }

        // Existing GetUserDetails, Register, and Login methods...

        [HttpPost("/getUserDetails")]
        [Authorize]
        public async Task<responseData> GetUserDetails([FromBody] requestData req)
        {
            responseData resData = new responseData();
            await Task.Run(() =>
            {
                try
                {
                    if (req.addInfo == null || !req.addInfo.ContainsKey("id") || req.addInfo["id"] == null)
                    {
                        resData.rData["rCode"] = 1;
                        resData.rData["rMessage"] = "ID is missing or null in the request.";
                        resData.rStatus = 400;
                        return;
                    }

                    MySqlParameter[] myParam = new MySqlParameter[]
                    {
                        new MySqlParameter("@id", req.addInfo["id"])
                    };

                    var sql = @"SELECT id, name, email, createdAt FROM dummy_db.users where id=@id;";
                    var dbData = _dbServices.ExecuteSQLName(sql, myParam);

                    if (dbData != null && dbData.Any() && dbData[0].Any())
                    {
                        resData.rData["rData"] = dbData[0];
                        resData.rData["rCode"] = 0;
                        resData.rStatus = 200;
                        resData.rData["rMessage"] = "Data Retrieved Successfully";
                    }
                    else
                    {
                        resData.rStatus = 404;
                        resData.rData["rCode"] = 1;
                        resData.rData["rMessage"] = "No Data Available";
                    }
                }
                catch (Exception ex)
                {
                    resData.rStatus = 500;
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = ex.Message;
                }
            });
            return resData;
        }

        [HttpPost("/register")]
        [AllowAnonymous]
        public async Task<responseData> Register([FromBody] requestData req)
        {
            var res = new responseData { addInfo = req.addInfo };
            await Task.Run(() =>
            {
                try
                {
                    if (req.addInfo == null || !req.addInfo.ContainsKey("name") || !req.addInfo.ContainsKey("email") || !req.addInfo.ContainsKey("password"))
                    {
                        res.rStatus = 400;
                        res.rData["rCode"] = 1;
                        res.rData["rMessage"] = "Missing required fields: name, email, password.";
                        return;
                    }

                    string? name = req.addInfo["name"]?.ToString();
                    string? email = req.addInfo["email"]?.ToString();
                    string? password = req.addInfo["password"]?.ToString();

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    {
                        res.rStatus = 400;
                        res.rData["rCode"] = 1;
                        res.rData["rMessage"] = "Name, email, and password cannot be empty.";
                        return;
                    }

                    string hashedPassword = _commonFunctions.CalculateSHA256Hash(password);
                    string createdAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                    string chkSql = "SELECT * FROM dummy_db.users WHERE email=@email;";
                    var resChk = _dbServices.ExecuteSQLName(chkSql, new[] { new MySqlParameter("@email", email) });

                    if (resChk != null && resChk.Any() && resChk[0].Any())
                    {
                        res.rStatus = 409;
                        res.rData["rCode"] = 1;
                        res.rData["rMessage"] = "User with the same email already exists.";
                        return;
                    }

                    string insertSql = @"INSERT INTO dummy_db.users (name, email, password, createdAt)
                                         VALUES (@name, @email, @password, @createdAt);
                                         SELECT id, name, email, createdAt FROM dummy_db.users WHERE id = LAST_INSERT_ID();";

                    var insertParams = new MySqlParameter[]
                    {
                        new MySqlParameter("@name", name),
                        new MySqlParameter("@email", email),
                        new MySqlParameter("@password", hashedPassword),
                        new MySqlParameter("@createdAt", createdAt)
                    };

                    var result = _dbServices.ExecuteSQLName(insertSql, insertParams);

                    if (result != null && result.Any() && result[0].Any())
                    {
                        res.rStatus = 201;
                        res.rData["rCode"] = 0;
                        res.rData["rMessage"] = "Registration successful.";
                        res.rData["user"] = result[0][0];
                    }
                    else
                    {
                        res.rStatus = 500;
                        res.rData["rCode"] = 1;
                        res.rData["rMessage"] = "Registration failed.";
                    }
                }
                catch (Exception ex)
                {
                    res.rStatus = 500;
                    res.rData["rCode"] = 1;
                    res.rData["rMessage"] = "Error: " + ex.Message;
                }
            });
            return res;
        }

        [HttpPost("/login")]
        [AllowAnonymous]
        public async Task<responseData> Login([FromBody] requestData req)
        {
            var res = new responseData { addInfo = req.addInfo };
            await Task.Run(() =>
            {
                try
                {
                    if (req.addInfo == null || !req.addInfo.ContainsKey("username") || !req.addInfo.ContainsKey("password"))
                    {
                        res.rStatus = 400;
                        res.rData["rCode"] = 1;
                        res.rData["rMessage"] = "Username and password are required.";
                        return;
                    }

                    string? username = req.addInfo["username"]?.ToString();
                    string? password = req.addInfo["password"]?.ToString();

                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        res.rStatus = 400;
                        res.rData["rCode"] = 1;
                        res.rData["rMessage"] = "Username and password cannot be empty.";
                        return;
                    }

                    string hashedPassword = _commonFunctions.CalculateSHA256Hash(password);

                    var parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@username", username),
                        new MySqlParameter("@password", hashedPassword)
                    };

                    string sql = @"SELECT id, name FROM dummy_db.users WHERE (name=@username OR email=@username) AND password=@password LIMIT 1;";
                    var result = _dbServices.ExecuteSQLName(sql, parameters);

                    if (result != null && result.Any() && result[0].Any())
                    {
                        var user = result[0][0];
                        string userId = user["id"]?.ToString() ?? "";
                        string userName = user["name"]?.ToString() ?? "";

                        var tokenHandler = new JwtSecurityTokenHandler();
                        var keyString = _config["jwt_config:Key"];
                        if (string.IsNullOrEmpty(keyString)) throw new InvalidOperationException("JWT Key not configured.");
                        var key = Encoding.UTF8.GetBytes(keyString);

                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, userId),
                                new Claim(ClaimTypes.Name, userName)
                            }),
                            Expires = DateTime.UtcNow.AddHours(8),
                            Issuer = _config["jwt_config:Issuer"],
                            Audience = _config["jwt_config:Audience"],
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                        };

                        var token = tokenHandler.CreateToken(tokenDescriptor);
                        string tokenString = tokenHandler.WriteToken(token);

                        res.rStatus = 200;
                        res.rData["rCode"] = 0;
                        res.rData["rMessage"] = "Login successful.";
                        res.rData["token"] = tokenString;
                    }
                    else
                    {
                        res.rStatus = 401;
                        res.rData["rCode"] = 1;
                        res.rData["rMessage"] = "Invalid credentials.";
                    }
                }
                catch (Exception ex)
                {
                    res.rStatus = 500;
                    res.rData["rCode"] = 1;
                    res.rData["rMessage"] = "Login error: " + ex.Message;
                }
            });
            return res;
        }

        [HttpPost("/updateUser")]
        [Authorize]
        public async Task<responseData> UpdateUser([FromBody] requestData req)
        {
            var res = new responseData { addInfo = req.addInfo };
            await Task.Run(() =>
            {
                try
                {
                    if (req.addInfo == null || !req.addInfo.ContainsKey("id") || !req.addInfo.ContainsKey("name"))
                    {
                        res.rStatus = 400;
                        res.rData["rCode"] = 1;
                        res.rData["rMessage"] = "User ID and name are required.";
                        return;
                    }

                    string? id = req.addInfo["id"]?.ToString();
                    string? name = req.addInfo["name"]?.ToString();

                    string sql = "UPDATE dummy_db.users SET name = @name WHERE id = @id;";
                    var parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@name", name),
                        new MySqlParameter("@id", id)
                    };

                    _dbServices.ExecuteSQLName(sql, parameters);

                    res.rStatus = 200;
                    res.rData["rCode"] = 0;
                    res.rData["rMessage"] = "User updated successfully.";
                }
                catch (Exception ex)
                {
                    res.rStatus = 500;
                    res.rData["rCode"] = 1;
                    res.rData["rMessage"] = "Update error: " + ex.Message;
                }
            });
            return res;
        }

        [HttpPost("/deleteUser")]
        [Authorize]
        public async Task<responseData> DeleteUser([FromBody] requestData req)
        {
            var res = new responseData { addInfo = req.addInfo };
            await Task.Run(() =>
            {
                try
                {
                    if (req.addInfo == null || !req.addInfo.ContainsKey("id"))
                    {
                        res.rStatus = 400;
                        res.rData["rCode"] = 1;
                        res.rData["rMessage"] = "User ID is required.";
                        return;
                    }

                    string? id = req.addInfo["id"]?.ToString();

                    string sql = "DELETE FROM dummy_db.users WHERE id = @id;";
                    var parameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@id", id)
                    };

                    _dbServices.ExecuteSQLName(sql, parameters);

                    res.rStatus = 200;
                    res.rData["rCode"] = 0;
                    res.rData["rMessage"] = "User deleted successfully.";
                }
                catch (Exception ex)
                {
                    res.rStatus = 500;
                    res.rData["rCode"] = 1;
                    res.rData["rMessage"] = "Delete error: " + ex.Message;
                }
            });
            return res;
        }
    }
}