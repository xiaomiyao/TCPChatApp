using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Server.DataAccess
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public User GetUserByUsername(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            string query = "SELECT Username, PasswordHash FROM Users WHERE Username = @Username";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Username = reader["Username"].ToString(),
                    PasswordHash = reader["PasswordHash"].ToString()
                };
            }
            return null;
        }

        public bool AddUser(User user)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            string query = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }
}
