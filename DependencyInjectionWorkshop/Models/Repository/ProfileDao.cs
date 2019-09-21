using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models.Repository
{
    public interface IProfile
    {
        string GetPassword(string accountId);
    }

    public class ProfileDao : IProfile
    {
        public string GetPassword(string accountId)
        {
            using (var connection = new SqlConnection("my connection string"))
            {
                return SqlMapper.Query<string>(connection, "spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
    }
}