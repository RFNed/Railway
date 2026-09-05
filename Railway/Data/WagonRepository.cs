using MySqlConnector;
using RailwayApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RailwayApp.Data;

public class WagonRepository
{
    private static WagonWithType MapWagonWithType(MySqlDataReader reader)
    {
        return new WagonWithType
        {
            WagonId = reader.GetInt32("wagon_id"),
            WagonNumber = reader.GetString("wagon_number"),
            WagonTypeId = reader.GetInt32("wagon_type_id"),
            WagonTypeCode = reader.GetString("wagon_code"),
            TypeDescription = reader.GetString("description")
        };
    }

    public static async Task<List<WagonWithType>> GetAll(MySqlConnection conn)
    {
        List<WagonWithType> result = [];

        await conn.OpenAsync();

        string query = @"
            SELECT
                w.wagon_id
                , w.wagon_number
                , w.wagon_type_id
                , wt.wagon_code
                , wt.description
            FROM wagons w
            JOIN wagon_types wt ON wt.wagon_type_id = w.wagon_type_id;
        ";

        await using var cmd = new MySqlCommand(query, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(MapWagonWithType(reader));
        }

        return result;
    }

    public static async Task<WagonWithType?> GetById(MySqlConnection conn, int wagonId)
    {
        await conn.OpenAsync();

        const string query = @"
            SELECT
                w.wagon_id
                , w.wagon_number
                , w.wagon_type_id
                , wt.wagon_code
                , wt.description
            FROM wagons w
            JOIN wagon_types wt
            WHERE wagon_id = @wagonId;
        ";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@wagonId", wagonId);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new WagonWithType
            {
                WagonId = reader.GetInt32("wagon_id"),
                WagonNumber = reader.GetString("wagon_number"),
                WagonTypeId = reader.GetInt32("wagon_type_id"),
                WagonTypeCode = reader.GetString("wagon_code"),
                TypeDescription = reader.GetString("description"),
            };
        }

        return null;
    }

    public static async Task<bool> Create(MySqlConnection conn, NewWagon newWagon)
    {
        await conn.OpenAsync();

        const string query = "INSERT INTO wagons (wagon_number, wagon_type_id) VALUES (@wagonNumber, @wagonTypeId);";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@wagonNumber", newWagon.WagonNumber);
        cmd.Parameters.AddWithValue("@wagonTypeId", newWagon.WagonTypeId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Update(MySqlConnection conn, int wagonId, UpdateWagon updateWagon)
    {
        await conn.OpenAsync();

        const string query = @"
            UPDATE wagons SET
                wagon_number = @wagonNumber,
                wagon_type_id = @wagonTypeId
            WHERE wagon_id = @wagonId;
        ";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@wagonId", wagonId);
        cmd.Parameters.AddWithValue("@wagonNumber", updateWagon.WagonNumber);
        cmd.Parameters.AddWithValue("@wagonTypeId", updateWagon.WagonTypeId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Delete(MySqlConnection conn, int wagonId)
    {
        await conn.OpenAsync();

        const string query = "DELETE FROM wagons WHERE wagon_id = @wagonId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@wagonId", wagonId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
