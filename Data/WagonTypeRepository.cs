using MySqlConnector;
using RailwayApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RailwayApp.Data;

public class WagonTypeRepository
{
    public static async Task<List<WagonType>> GetAll(MySqlConnection conn)
    {
        List<WagonType> result = [];

        await conn.OpenAsync();

        string query = "SELECT * FROM wagon_types;";

        await using var cmd = new MySqlCommand(query, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new WagonType
            {
                WagonTypeId = reader.GetInt32("wagon_type_id"),
                WagonCode = reader.GetString("wagon_code"),
                Description = reader.GetString("description"),
            });
        }

        return result;
    }

    public static async Task<WagonType?> GetById(MySqlConnection conn, int wagonTypeId)
    {
        await conn.OpenAsync();

        const string query = "SELECT * FROM wagon_types WHERE wagon_type_id = @wagonTypeId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@wagonTypeId", wagonTypeId);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new WagonType
            {
                WagonTypeId = reader.GetInt32("wagon_type_id"),
                WagonCode = reader.GetString("wagon_code"),
                Description = reader.GetString("description"),
            };
        }

        return null;
    }

    public static async Task<bool> Create(MySqlConnection conn, NewWagonType newWagonType)
    {
        await conn.OpenAsync();

        const string query = "INSERT INTO wagon_types (wagon_code, description) VALUES (@wagonCode, @description);";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@wagonCode", newWagonType.WagonCode);
        cmd.Parameters.AddWithValue("@description", newWagonType.Description);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Update(MySqlConnection conn, int wagonTypeId, UpdateWagonType updateWagonType)
    {
        await conn.OpenAsync();

        const string query = @"
            UPDATE wagon_types SET
                wagon_code = @wagonCode,
                description = @description
            WHERE wagon_type_id = @wagonTypeId;
        ";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@wagonTypeId", wagonTypeId);
        cmd.Parameters.AddWithValue("@wagonCode", updateWagonType.WagonCode);
        cmd.Parameters.AddWithValue("@description", updateWagonType.Description);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Delete(MySqlConnection conn, int wagonTypeId)
    {
        await conn.OpenAsync();

        const string query = "DELETE FROM wagon_types WHERE wagon_type_id = @wagonTypeId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@wagonTypeId", wagonTypeId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
