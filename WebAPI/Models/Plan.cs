using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace webapi_csharp.Models
{
    public class Plan
    {
        public static async Task<dynamic> findPlanLimiteDetailBySubscriptionKey(IDbConnection db, int subscriptionKey) {
            var query = @"
                SELECT
                    rate_limit.interval AS rl_interval,
                    rate_limit.limit_value AS rl_value,
                    quota_limit.limit_value AS ql_value,
                    quota_limit.interval AS ql_interval,
                    quota_limit.allow_overage AS ql_allow_overage,
                    quota_limit.overage_fee AS ql_overage_fee
                FROM
                    subscriptions
                    INNER JOIN quota_limit ON subscriptions.plan_id = quota_limit.plan_id
                    INNER JOIN rate_limit ON subscriptions.plan_id = rate_limit.plan_id
                    INNER JOIN request_authentication ON subscriptions.id = request_authentication.subscription_id
                WHERE
                    request_authentication.public_key = @subscriptionKey
            ";

            var rows = await db.QueryAsync(query, new { subscriptionKey });
            
            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        } 

        public static async Task<dynamic> findPlanLimitDetailById(IDbConnection db, int id) {
            var query = @"
                SELECT
                    rate_limit.interval AS rl_interval,
                    rate_limit.limit_value AS rl_value,
                    quota_limit.limit_value AS ql_value,
                    quota_limit.interval AS ql_interval,
                    quota_limit.allow_overage AS ql_allow_overage,
                    quota_limit.overage_fee AS ql_overage_fee
                FROM
                    plans
                    INNER JOIN quota_limit ON plans.id = quota_limit.plan_id
                    INNER JOIN rate_limit ON plans.id = rate_limit.plan_id
                WHERE
                    plans.id = @Id
            ";

            var rows = await db.QueryAsync(query, new { id });
            
            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        }

        public static async Task<dynamic> findById(IDbConnection db, int id) {
            var rows = await db.QueryAsync("SELECT * FROM plans WHERE id = @id", new { id });

            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        }
    }
}