using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;


namespace webapi_csharp.Models {
    public class SubscriptionsModel
    {
        public int Id {get; set;}
        public int UserId {get; set;}
        public int ServiceId {get; set;}
        public int PlanId {get; set;}
        public string Status {get; set;}
        public string PaymentStatus {get; set;}
        public DateTime StartDate {get; set;}
        public DateOnly EndDate {get; set;}

        
        public static async Task<dynamic> cancelByDetails(IDbConnection db, int user_id, int service_id)
        {   
            var query = @"UPDATE subscriptions
                          SET status = 'cancelled'
                          WHERE user_id = @UserId AND service_id = @ServiceId AND status = 'active'
                          OUTPUT INSERTED.*";

            var rows = await db.QueryAsync<dynamic>(query, new { UserId = user_id, ServiceId = service_id });

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        }
        public static async Task<dynamic> getActiveByDetails(IDbConnection db, int user_id, int service_id) {
            var query = @"
                SELECT *
                FROM subscriptions
                WHERE user_id = @UserId AND service_id = @ServiceId AND status = 'active'
            ";

            var rows = await db.QueryAsync<dynamic>(query, new { UserId = user_id, ServiceId = service_id });

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        } 
        public static async Task<dynamic> getActiveSubscriptions(IDbConnection db) {
            var query = @"
                SELECT *
                FROM subscriptions
                WHERE status = 'active'
            ";

            var rows = await db.QueryAsync<dynamic>(query);

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        } 
        public static async Task<dynamic> getByAccessToken(IDbConnection db, string token) {
            var query = @"
                SELECT S.*
                FROM subscriptions S, subscription_authentication SA
                WHERE S.id = SA.subscription_id AND access_token = @Token
            ";

            var rows = await db.QueryAsync<dynamic>(query, new { Token = token });

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        } 
        public static async Task<dynamic> updateStatusById(IDbConnection db, int id, string status)
        {   
            var query = @"UPDATE subscriptions
                          SET status = @Status
                          WHERE id = @Id
                          OUTPUT INSERTED.*";

            var rows = await db.QueryAsync<dynamic>(query, new { Id = id, Status = status });

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        }
        public static async Task<dynamic> expired(IDbConnection db)
        {   
            var utcDate = DateTime.UtcNow;
            var query = @"UPDATE subscriptions
                          SET status = 'expired'
                          WHERE status = 'active' AND end_date <= @UtcDate
                          OUTPUT INSERTED.*";

            var rows = await db.QueryAsync<dynamic>(query, new { UtcDate = utcDate });

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        }
        public static async Task<dynamic> expiredById(IDbConnection db, int id)
        {   
            var utcDate = DateTime.UtcNow;
            var query = @"UPDATE subscriptions
                          SET status = 'expired'
                          WHERE id = @Id AND status = 'active' AND end_date <= @UtcDate
                          OUTPUT INSERTED.*";

            var rows = await db.QueryAsync<dynamic>(query, new { Id = id, UtcDate = utcDate });

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        }
    }
}
