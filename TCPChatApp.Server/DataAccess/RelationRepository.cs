using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TCPChatApp.Common.Models;

namespace TCPChatApp.Server.DataAccess
{
    public class RelationRepository
    {
        private readonly string _connectionString;

        public RelationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Relation> GetRelationsByUserName(string username)
        {
            var relations = new List<Relation>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT UserName, TargetName, IsBlocked, IsFriend FROM Relations WHERE UserName = @UserName";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", username);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                relations.Add(new Relation
                {
                    UserName = reader["UserName"].ToString(),
                    TargetName = reader["TargetName"].ToString(),
                    IsBlocked = Convert.ToBoolean(reader["IsBlocked"]),
                    IsFriend = Convert.ToBoolean(reader["IsFriend"])
                });
            }
            return relations;
        }

        public bool AddRelation(Relation relation)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "INSERT INTO Relations (UserName, TargetName, IsBlocked, IsFriend) VALUES (@UserName, @TargetName, @IsBlocked, @IsFriend)";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", relation.UserName);
            command.Parameters.AddWithValue("@TargetName", relation.TargetName);
            command.Parameters.AddWithValue("@IsBlocked", relation.IsBlocked);
            command.Parameters.AddWithValue("@IsFriend", relation.IsFriend);

            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public bool UpdateRelation(Relation relation)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "UPDATE Relations SET IsBlocked = @IsBlocked, IsFriend = @IsFriend WHERE UserName = @UserName AND TargetName = @TargetName";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", relation.UserName);
            command.Parameters.AddWithValue("@TargetName", relation.TargetName);
            command.Parameters.AddWithValue("@IsBlocked", relation.IsBlocked);
            command.Parameters.AddWithValue("@IsFriend", relation.IsFriend);

            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public bool DeleteBlock(string username, string targetName)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "DELETE FROM Relations WHERE UserName = @UserName AND TargetName = @TargetName";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", username);
            command.Parameters.AddWithValue("@TargetName", targetName);

            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public bool DeleteFriend(string username, string targetName)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "DELETE FROM Relations WHERE UserName = @UserName AND TargetName = @TargetName AND IsFriend = @IsFriend";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", username);
            command.Parameters.AddWithValue("@TargetName", targetName);
            command.Parameters.AddWithValue("@IsFriend", true);

            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }
}