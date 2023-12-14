using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;


namespace webapi_csharp.Models {
    public class Requests
    {
        public int Id {get; set;}
        public int SubscriptionId {get; set;}
        public int Latency {get; set;}
        public bool IsOverage {get; set;}
        public DateTime CreatedAt {get; set;}

        public static async Task<dynamic> getRequestCountWithinTimeRangeByServiceId(IDbConnection db, int service_id, DateTime start_date, DateTime end_date)
        {
            var query = @"SELECT COUNT(R.request_id) AS request_count
                          FROM requests R, subscriptions S
                          WHERE S.subscription_id = R.subscription_id AND
                                S.service_id = @ServiceId AND
                                R.created_at BETWEEN @StartDate AND @EndDate";

            var rows = await db.QueryAsync<dynamic>(query, new { ServiceId = service_id, StartDate = start_date, EndDate = end_date });

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        }
        public static async Task<dynamic> getAvgRequestLatencyWithinTimeRangeByServiceId(IDbConnection db, int service_id, DateTime start_date, DateTime end_date)
        {
            var query = @"SELECT AVG(R.latency) AS avg_latency
                          FROM requests R, subscriptions S
                          WHERE S.subscription_id = R.subscription_id AND
                                S.service_id = @ServiceId AND
                                R.created_at BETWEEN @StartDate AND @EndDate";

            var rows = await db.QueryAsync<dynamic>(query, new { ServiceId = service_id, StartDate = start_date, EndDate = end_date });

            return rows.AsList().Count > 0 ? rows.AsList() : new List<object>();
        }
        public static async Task<dynamic> findById(IDbConnection db, int id)
        {
            var rows = await db.QueryAsync("SELECT * FROM requests WHERE id = @Id", new { Id = id });

            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        }
        public static async Task<dynamic> create(IDbConnection db, int subscription_id, int latency, bool is_overage)
        {
            var query = @"INSERT INTO requests(subscription_id, latency, is_overage)
                          VALUES (@SubscriptionId, @Latency, @IsOverage)
                          RETURNING *";

            var rows = await db.QueryAsync<dynamic>(query, new { SubscriptionId = subscription_id, Latency = latency, IsOverage = is_overage });

            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        }
    }
}
