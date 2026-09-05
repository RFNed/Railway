using MySqlConnector;
using RailwayApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Railway.Data;

public class TrainWagonRepository
{
    public static async Task<List<TrainWagon>> GetByTrainId(MySqlConnection conn, int trainId)
    {
        List<TrainWagon> result = [];

        await conn.OpenAsync();

        const string query = @"
            SELECT 
                train_wagon_id,
                train_id,
                wagon_id,
                is_loaded
            FROM train_wagons
            WHERE train_id = @trainId;
        ";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainId", trainId);

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new TrainWagon
            {
                TrainWagonId = reader.GetInt32("train_wagon_id"),
                TrainId = reader.GetInt32("train_id"),
                WagonId = reader.GetInt32("wagon_id"),
                IsLoaded = reader.GetBoolean("is_loaded"),
            });
        }

        return result;
    }

    public static async Task<bool> Add(MySqlConnection conn, NewTrainWagon newTrainWagon)
    {
        await conn.OpenAsync();

        const string query = @"
            INSERT INTO train_wagons (train_id, wagon_id, is_loaded)
            VALUES (@trainId, @wagonId, @isLoaded);
        ";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainId", newTrainWagon.TrainId);
        cmd.Parameters.AddWithValue("@wagonId", newTrainWagon.WagonId);
        cmd.Parameters.AddWithValue("@isLoaded", newTrainWagon.IsLoaded);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> AddRange(MySqlConnection conn, List<NewTrainWagon> wagons)
    {
        await conn.OpenAsync();

        foreach (var item in wagons)
        {
            const string query = @"
                INSERT INTO train_wagons (train_id, wagon_id, is_loaded)
                VALUES (@trainId, @wagonId, @isLoaded);
            ";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@trainId", item.TrainId);
            cmd.Parameters.AddWithValue("@wagonId", item.WagonId);
            cmd.Parameters.AddWithValue("@isLoaded", item.IsLoaded);

            await cmd.ExecuteNonQueryAsync();
        }

        return true;
    }

    public static async Task<bool> Update(MySqlConnection conn, int trainWagonId, UpdateTrainWagon updateTrainWagon)
    {
        await conn.OpenAsync();

        const string query = @"
            UPDATE train_wagons SET
                train_id = @trainId,
                wagon_id = @wagonId,
                is_loaded = @isLoaded
            WHERE train_wagon_id = @trainWagonId;
        ";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainWagonId", trainWagonId);
        cmd.Parameters.AddWithValue("@trainId", updateTrainWagon.TrainId);
        cmd.Parameters.AddWithValue("@wagonId", updateTrainWagon.WagonId);
        cmd.Parameters.AddWithValue("@isLoaded", updateTrainWagon.IsLoaded);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> DeleteByTrainId(MySqlConnection conn, int trainId)
    {
        await conn.OpenAsync();

        const string query = "DELETE FROM train_wagons WHERE train_id = @trainId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainId", trainId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Delete(MySqlConnection conn, int trainWagonId)
    {
        await conn.OpenAsync();

        const string query = "DELETE FROM train_wagons WHERE train_wagon_id = @trainWagonId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainWagonId", trainWagonId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
