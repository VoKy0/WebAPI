using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;


namespace webapi_csharp.Models {
    public class Auth
    {
        public static async Task<bool> create(IDbConnection db, int userId, string verifyToken, string password, string refreshToken)
        {
            var parameters = new
            {
                user_id = userId,
                verify_token = verifyToken,
                password = password,
                refresh_token = refreshToken
            };

            var rows = await db.QueryAsync("INSERT INTO authentication (user_id, verify_token, password, refresh_token) VALUES (@user_id, @verify_token, @password, @refresh_token) RETURNING *", parameters);

            return rows.AsList().Count > 0;
        }

        public static async Task<bool> update(IDbConnection db, int userId, string verifyToken, string password, string refreshToken)
        {
            var parameters = new
            {
                verify_token = verifyToken,
                password = password,
                refresh_token = refreshToken,
                user_id = userId
            };

            var rows = await db.QueryAsync("UPDATE authentication SET verify_token = @verify_token, password = @password, refresh_token = @refresh_token WHERE user_id = @user_id RETURNING *", parameters);

            return rows.AsList().Count > 0;
        }

        public static async Task<dynamic> findByUserId(IDbConnection db, int userId)
        {
            var rows = await db.QueryAsync("SELECT * FROM authentication WHERE user_id = @userId", new { userId });

            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        }

        public static async Task<string> findTokenByUserId(IDbConnection db, int userId)
        {
            var rows = await db.QueryAsync("SELECT refresh_token FROM authentication WHERE user_id = @userId", new { userId });

            return rows.AsList().Count > 0 ? rows.AsList()[0].refresh_token : new List<object>();
        }

        public static async Task<dynamic> findUserByVerifyToken(IDbConnection db, string verifyToken)
        {
            var rows = await db.QueryAsync("SELECT * FROM authentication WHERE verify_token = @verifyToken", new { verifyToken });

            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        }

        public static async Task<string> findPasswordByUserId(IDbConnection db, int userId)
        {
            var rows = await db.QueryAsync("SELECT password FROM authentication WHERE user_id = @userId", new { userId });

            return rows.AsList().Count > 0 ? rows.AsList()[0].password : new List<object>();
        }

        public static async Task<bool> updateRefreshToken(IDbConnection db, string newRefreshToken, int userId)
        {
            var parameters = new
            {
                refresh_token = newRefreshToken,
                user_id = userId
            };

            var rows = await db.QueryAsync("UPDATE authentication SET refresh_token = @refresh_token WHERE user_id = @user_id RETURNING *", parameters);

            return rows.AsList().Count > 0;
        }

        public static async Task<bool> updateVerifyToken(IDbConnection db, string newVerifyToken, int userId)
        {
            var parameters = new
            {
                verify_token = newVerifyToken,
                user_id = userId
            };

            var rows = await db.QueryAsync("UPDATE authentication SET verify_token = @verify_token WHERE user_id = @user_id RETURNING *", parameters);

            return rows.AsList().Count > 0;
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Role { get; set; }       
    }
}
