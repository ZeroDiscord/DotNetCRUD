using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using MySqlX.XDevAPI.Common;  // <-- add this

namespace COMMON_API.Service
{
    public class apiController
    {
        private readonly IConfiguration _config;
        private readonly dbServices ds;

        public apiController(IConfiguration config)  // inject IConfiguration here
        {
            _config = config;
            ds = new dbServices();
        }

        public async Task<responseData> getUserDetails(requestData req)
        {
            responseData resData = new responseData();
            try
            {
                MySqlParameter[] myParam = new MySqlParameter[]
                {
                    new MySqlParameter("@id",req.addInfo["id"])
                };
                var sql = @"SELECT * FROM dummy_db.users where id=@id;";
                var dbData = ds.ExecuteSQLName(sql, myParam);
                if (dbData[0].Count() > 0)
                {
                    resData.rData["rData"] = dbData[0];
                    resData.rData["rCode"] = 0;
                    resData.rStatus = 200;
                    resData.rData["rMessage"] = "Data Retrieve Successfully";
                }
                else
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "No Data Available";
                }
            }
            catch (Exception ex)
            {
                resData.rData["rCode"] = 1;
                resData.rData["rMessage"] = ex.Message;
            }
            return resData;
        }

      public async Task<responseData> Register(requestData req)
{
    responseData res = new responseData
    {
        addInfo = req.addInfo
    };

    try
    {
        string name = req.addInfo["name"].ToString();
        string email = req.addInfo["email"].ToString();
        string password = cf.CalculateSHA256Hash(req.addInfo["password"].ToString());
        string createdAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        // Check if email exists
        string chksql = "SELECT * FROM dummy_db.users WHERE email=@email;";
        MySqlParameter[] checkParams = new MySqlParameter[]
        {
            new MySqlParameter("@email", email)
        };
        var reschk = ds.ExecuteSQLName(chksql, checkParams);
        if (reschk != null && reschk.Count > 0 && reschk[0].Length > 0)
        {
            res.rStatus = 200;
            res.rData["rCode"] = 1;
            res.rData["rMessage"] = "User with same email already exists";
            res.rData["user"] = reschk[0][0];
            return res; // Early return if user exists
        }

        // Insert new user
        string insertSql = @"INSERT INTO dummy_db.users (name, email, password, createdAt)
                             VALUES (@name, @email, @password, @createdAt);
                             SELECT * FROM dummy_db.users WHERE id = LAST_INSERT_ID();";

        MySqlParameter[] insertParams = new MySqlParameter[]
        {
            new MySqlParameter("@name", name),
            new MySqlParameter("@email", email),
            new MySqlParameter("@password", password),
            new MySqlParameter("@createdAt", createdAt)
        };

        var result = ds.ExecuteSQLName(insertSql, insertParams);

        if (result != null && result.Count > 0 && result[0].Length > 0)
        {
            res.rStatus = 200;
            res.rData["rCode"] = 0;
            res.rData["rMessage"] = "Registration successful";
            res.rData["user"] = result[0][0];
        }
        else
        {
            res.rStatus = 400;
            res.rData["rCode"] = 1;
            res.rData["rMessage"] = "Registration failed";
        }
    }
    catch (Exception ex)
    {
        res.rStatus = 500;
        res.rData["rCode"] = 1;
        res.rData["rMessage"] = "Error: " + ex.Message;
    }

    return res;
}

public async Task<responseData> Login(requestData req)
{
    responseData res = new responseData
    {
        addInfo = req.addInfo
    };

    try
    {
        string username = req.addInfo["username"].ToString();
        string password = cf.CalculateSHA256Hash(req.addInfo["password"].ToString());

        MySqlParameter[] parameters = new MySqlParameter[]
        {
            new MySqlParameter("@username", username),//mobile_no,email-----unique
            new MySqlParameter("@password", password)
        };

        string sql = @"SELECT id FROM dummy_db.users WHERE name=@username AND password=@password LIMIT 1;";//user details
        var result = ds.ExecuteSQLName(sql, parameters);

        if (result != null && result.Count > 0 && result[0].Length > 0)
        {
            string rawGuid = result[0][0]["id"].ToString();
            string hashedGuid = cf.CalculateSHA256Hash(rawGuid);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["jwt_config:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim("guid", hashedGuid)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["jwt_config:Issuer"],
                Audience = _config["jwt_config:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(token);

            res.rStatus = 200;
            res.rData["rCode"] = 0;
            res.rData["rMessage"] = "Login successful";
            res.rData["token"] = tokenString;
        }
        else
        {
            res.rStatus = 401;
            res.rData["rCode"] = 1;
            res.rData["rMessage"] = "Invalid credentials";
        }
    }
    catch (Exception ex)
    {
        res.rStatus = 500;
        res.rData["rCode"] = 1;
        res.rData["rMessage"] = "Login error: " + ex.Message;
    }

    return res;
}


    }
}
