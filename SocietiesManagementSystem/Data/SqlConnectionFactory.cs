using System.Configuration;
using Microsoft.Data.SqlClient;

namespace SocietiesManagementSystem.Data;

public static class SqlConnectionFactory
{
    public static SqlConnection CreateOpenConnection()
    {
        var cs = ConfigurationManager.ConnectionStrings["SocietiesDb"]?.ConnectionString;
        if (string.IsNullOrWhiteSpace(cs))
            cs = "Data Source=localhost;Initial Catalog=SocietiesManagement;Integrated Security=True;TrustServerCertificate=True;";

        var conn = new SqlConnection(cs);
        conn.Open();
        return conn;
    }
}
