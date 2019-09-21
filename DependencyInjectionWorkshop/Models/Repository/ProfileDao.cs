using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models.Repository
{
    public class ProfileDao
    {
        public string GetPasswordFromDb(string accountId)
        {
            using (var connection = new SqlConnection("my connection string"))
            {
                return SqlMapper.Query<string>(connection, "spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
    }
}