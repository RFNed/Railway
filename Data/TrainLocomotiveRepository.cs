using MySqlConnector;
using RailwayApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Railway.Data;

public class TrainLocomotiveRepository
{
    public static async Task<List<TrainLocomotive>> GetByTrainId(MySqlConnection conn, int trainId)
    {
        List<TrainLocomotive> result = [];

        await conn.OpenAsync();

        const string query = @"SELECT * FROM train_locomotives WHERE train_id = @trainId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainId", trainId);

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new TrainLocomotive
            {
                TrainLocomotiveId = reader.GetInt32("train_locomotive_id"),
                TrainId = reader.GetInt32("train_id"),
                LocomotiveId = reader.GetInt32("locomotive_id"),
            });
        }

        return result;
    }

    public static async Task<bool> Add(MySqlConnection conn, NewTrainLocomotive newTrainLocomotive)
    {
        await conn.OpenAsync();

        const string query = @"INSERT INTO train_locomotives (train_id, locomotive_id) VALUES (@trainId, @locomotiveId);";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainId", newTrainLocomotive.TrainId);
        cmd.Parameters.AddWithValue("@locomotiveId", newTrainLocomotive.LocomotiveId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> AddRange(MySqlConnection conn, List<NewTrainLocomotive> locomotives)
    {
        await conn.OpenAsync();

        foreach (var item in locomotives)
        {
            const string query = @"
                INSERT INTO train_locomotives (train_id, locomotive_id)
                VALUES (@trainId, @locomotiveId);
            ";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@trainId", item.TrainId);
            cmd.Parameters.AddWithValue("@locomotiveId", item.LocomotiveId);

            await cmd.ExecuteNonQueryAsync();
        }

        return true;
    }

    public static async Task<bool> DeleteByTrainId(MySqlConnection conn, int trainId)
    {
        await conn.OpenAsync();

        const string query = "DELETE FROM train_locomotives WHERE train_id = @trainId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainId", trainId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Delete(MySqlConnection conn, int trainLocomotiveId)
    {
        await conn.OpenAsync();

        const string query = "DELETE FROM train_locomotives WHERE train_locomotive_id = @trainLocomotiveId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainLocomotiveId", trainLocomotiveId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
