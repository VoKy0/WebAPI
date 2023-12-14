using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace webapi_csharp.Models
{
    public class ServiceAuth
    {
        public static async Task<dynamic> findByServiceId(IDbConnection db, int service_id) {
            var rows = await db.QueryAsync("SELECT * FROM service_authentication WHERE service_id = @ServiceId", new { service_id });

            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        }
    }
}