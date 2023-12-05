using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace webapi_csharp.models
{
    public class User
    {
        public static async Task<dynamic> findByEmail(IDbConnection db, string email) {
            var rows = await db.QueryAsync("SELECT * FROM users WHERE email = @email", new { email });

            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        }

        public static async Task<dynamic> findByUsername(IDbConnection db, string username) {
            var rows = await db.QueryAsync("SELECT * FROM users WHERE username = @username", username);

            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        }

        public static async Task<dynamic> findById(IDbConnection db, int id) {
            var rows = await db.QueryAsync("SELECT * FROM users WHERE id = @id", id);

            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        }

        public static async Task<dynamic> createUser(IDbConnection db, string first_name, string last_name, string email) {
            var parameters = new {
                first_name = first_name,
                last_name = last_name,
                email = email,
                active = true
            };

            var rows = await db.QueryAsync("INSERT INTO users (first_name, last_name, email, active) VALUES (@first_name, @last_name, @email, @active) RETURNING *", parameters);
            
            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        } 
    }
}