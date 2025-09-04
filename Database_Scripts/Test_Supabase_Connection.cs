using Npgsql;
using System;

namespace TestSupabaseConnection
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Host=hzweniqfssqorruiujwc.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=Y@Z105213eed;SSL Mode=Require;Trust Server Certificate=true;Timeout=60;CommandTimeout=30;";
            
            Console.WriteLine("🔍 Testing Supabase connection...");
            Console.WriteLine($"🔍 Connection string: {connectionString.Replace("Password=Y@Z105213eed", "Password=***")}");
            
            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                Console.WriteLine("🔍 Connection created, attempting to open...");
                connection.Open();
                Console.WriteLine("✅ Supabase connection opened successfully");
                
                // Test basic query
                using var cmd = new NpgsqlCommand("SELECT 1", connection);
                var result = cmd.ExecuteScalar();
                Console.WriteLine($"✅ Test query result: {result}");
                
                // Test Users table
                using var cmd2 = new NpgsqlCommand("SELECT COUNT(*) FROM \"Users\"", connection);
                var count = cmd2.ExecuteScalar();
                Console.WriteLine($"✅ Users table exists with {count} records");
                
                // Test specific user
                using var cmd3 = new NpgsqlCommand("SELECT \"Username\", \"RoleName\" FROM \"Users\" WHERE \"Username\" = @username", connection);
                cmd3.Parameters.AddWithValue("@username", "admin");
                using var reader = cmd3.ExecuteReader();
                
                if (reader.Read())
                {
                    Console.WriteLine($"✅ Admin user found: {reader["Username"]} - {reader["RoleName"]}");
                }
                else
                {
                    Console.WriteLine("❌ Admin user not found");
                }
                
                connection.Close();
                Console.WriteLine("✅ Supabase connection closed successfully");
                Console.WriteLine("🎉 All tests passed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection failed: {ex.Message}");
                Console.WriteLine($"❌ Exception type: {ex.GetType().Name}");
                
                if (ex is NpgsqlException npgsqlEx)
                {
                    Console.WriteLine($"❌ PostgreSQL Error Code: {npgsqlEx.SqlState}");
                    Console.WriteLine($"❌ PostgreSQL Error Detail: {npgsqlEx.Data}");
                }
            }
        }
    }
}