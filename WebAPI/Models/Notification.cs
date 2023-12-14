using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;


namespace webapi_csharp.Models {
    public class Notification
    {
        public int Id {get; set;}
        public int UserId {get; set;}
        public string Title {get; set;}
        public string Description {get; set;}
        public bool Seen {get; set;}

        public static async Task<dynamic> getByUserId(IDbConnection db, int user_id)
        {   
            var query = @"SELECT *
                          FROM notifications
                          WHERE user_id = @UserId
                          ORDER BY created_at DESC";

            var rows = await db.QueryAsync<dynamic>(query, new { UserId = user_id });

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        }
        public static async Task<dynamic> seenById(IDbConnection db, int id, bool seen)
        {   
            var query = @"UPDATE notifications
                          SET seen = @Seen
                          WHERE id = @Id;
                          
                          SELECT *
                          FROM notifications
                          WHERE id = @Id";

            var rows = await db.QueryAsync<dynamic>(query, new { Seen = seen, Id = id });

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        }
        public static async Task<dynamic> delete(IDbConnection db, int id)
        {   
            var query = @"DELETE FROM notifications
                          WHERE id = @Id";

            var rows = await db.QueryAsync<dynamic>(query, new { Id = id });

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        }
        public static async Task<dynamic> create(IDbConnection db, int user_id, string title, string description)
        {   
            var query = @"INSERT INTO notifications(user_id, title, description)
                          VALUES (@UserId, @Title, @Description);
                          
                          SELECT * FROM notifications WHERE id = SCOPE_IDENTITY()";

            var rows = await db.QueryAsync<dynamic>(query, new { UserId = user_id, Title = title, Description = description });

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        }
    }
}
