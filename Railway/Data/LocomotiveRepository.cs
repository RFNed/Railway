using MySqlConnector;
using RailwayApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RailwayApp.Data;

public class LocomotiveRepository
{
    public static async Task<List<Locomotive>> GetAll(MySqlConnection conn)
    {
        List<Locomotive> result = [];

        await conn.OpenAsync();

        string query = "SELECT * FROM locomotives;";

        await using var cmd = new MySqlCommand(query, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new Locomotive
            {
                LocomotiveId = reader.GetInt32("locomotive_id"),
                LocomotiveNumber = reader.GetString("locomotive_number"),
            });
        }

        return result;
    }

    public static async Task<Locomotive?> GetById(MySqlConnection conn, int locomotiveId)
    {
        await conn.OpenAsync();

        const string query = "SELECT * FROM locomotives WHERE locomotive_id = @locomotiveId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@locomotiveId", locomotiveId);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Locomotive
            {
                LocomotiveId = reader.GetInt32("locomotive_id"),
                LocomotiveNumber = reader.GetString("locomotive_number"),
            };
        }

        return null;
    }

    public static async Task<bool> Create(MySqlConnection conn, string locomotiveNumber)
    {
        await conn.OpenAsync();

        const string query = "INSERT INTO locomotives (locomotive_number) VALUES (@locomotiveNumber);";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@locomotiveNumber", locomotiveNumber);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Update(MySqlConnection conn, int locomotiveId, string locomotiveNumber)
    {
        await conn.OpenAsync();

        const string query = "UPDATE locomotives SET locomotive_number = @locomotiveNumber WHERE locomotive_id = @locomotiveId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@locomotiveId", locomotiveId);
        cmd.Parameters.AddWithValue("@locomotiveNumber", locomotiveNumber);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Delete(MySqlConnection conn, int locomotiveId)
    {
        await conn.OpenAsync();

        const string query = "DELETE FROM locomotives WHERE locomotive_id = @locomotiveId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@locomotiveId", locomotiveId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
