using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace webapi_csharp.Models
{
    public class Service
    {
        public static async Task<dynamic> getServicesMetadata(IDbConnection db) {
            var query = @"
                SELECT 
                    services.*, 
                    service_authentication.private_key
                FROM
                    services
                    INNER JOIN service_authentication ON services.id = service_authentication.service_id
            ";

            var rows = await db.QueryAsync(query);

            return rows.AsList();
        } 

        public static async Task<dynamic> getEndpointByServiceId(IDbConnection db, int serviceId) {
            var rows = await db.QueryAsync("SELECT * FROM endpoints WHERE service_id = @service_id", new { serviceId });

            return rows.AsList();
        }

        public static async Task<dynamic> findById(IDbConnection db, int id) {
            var rows = await db.QueryAsync("SELECT * FROM services WHERE id = @id", new { id });

            return rows.AsList().Count > 0 ? rows.AsList()[0] : new List<object>();
        }
    }
}