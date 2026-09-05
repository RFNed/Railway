using MySqlConnector;
using RailwayApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RailwayApp.Data;

public class TrainRepository
{
    private static TrainDetails MapTrainDetails(MySqlDataReader reader)
    {
        return new TrainDetails
        {
            TrainId = reader.GetInt32("train_id"),
            TrainNumber = reader.GetString("train_number"),

            DepartureCityId = reader.GetInt32("departure_city_id"),
            DepartureCityName = reader.GetString("dep_city"),

            ArrivalCityId = reader.GetInt32("arrival_city_id"),
            ArrivalCityName = reader.GetString("arr_city"),

            FormationDatetime = reader.GetDateTime("formation_datetime"),

            ManagerId = reader.GetInt32("manager_id"),
            ManagerName = reader.GetString("manager_name"),

            DriverId = reader.GetInt32("driver_id"),
            DriverName = reader.GetString("driver_name"),

            AssistantId = reader.GetInt32("assistant_id"),
            AssistantName = reader.GetString("assistant_name"),
        };
    }

    public static async Task<List<TrainDetails>> GetAll(MySqlConnection conn)
    {
        List<TrainDetails> result = [];

        await conn.OpenAsync();

        string query = @"
            SELECT 
                t.train_id, t.train_number, t.formation_datetime,
                t.departure_city_id, c1.city_name AS dep_city,
                t.arrival_city_id, c2.city_name AS arr_city,
                t.manager_id, CONCAT(m.last_name, ' ', m.first_name) AS manager_name,
                t.driver_id, CONCAT(d.last_name, ' ', d.first_name) AS driver_name,
                t.assistant_id, CONCAT(a.last_name, ' ', a.first_name) AS assistant_name
            FROM trains t
            JOIN cities c1 ON t.departure_city_id = c1.city_id
            JOIN cities c2 ON t.arrival_city_id = c2.city_id
            JOIN employees m ON t.manager_id = m.employee_id
            JOIN employees d ON t.driver_id = d.employee_id
            JOIN employees a ON t.assistant_id = a.employee_id
            ORDER BY t.train_id DESC;
        ";

        await using var cmd = new MySqlCommand(query, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(MapTrainDetails(reader));
        }

        return result;
    }

    public static async Task<TrainDetails?> GetById(MySqlConnection conn, int trainId)
    {
        await conn.OpenAsync();

        const string query = @"
            SELECT 
                t.train_id, t.train_number, t.formation_datetime,
                t.departure_city_id, c1.city_name AS dep_city,
                t.arrival_city_id, c2.city_name AS arr_city,
                t.manager_id, CONCAT(m.last_name, ' ', m.first_name) AS manager_name,
                t.driver_id, CONCAT(d.last_name, ' ', d.first_name) AS driver_name,
                t.assistant_id, CONCAT(a.last_name, ' ', a.first_name) AS assistant_name
            FROM trains t
            JOIN cities c1 ON t.departure_city_id = c1.city_id
            JOIN cities c2 ON t.arrival_city_id = c2.city_id
            JOIN employees m ON t.manager_id = m.employee_id
            JOIN employees d ON t.driver_id = d.employee_id
            JOIN employees a ON t.assistant_id = a.employee_id
            WHERE t.train_id = @trainId;
        ";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainId", trainId);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapTrainDetails(reader);
        }

        return null;
    }

    public static async Task<bool> Create(MySqlConnection conn, NewTrain newTrain)
    {
        await conn.OpenAsync();

        const string query = @"
            INSERT INTO trains (
                train_number,
                departure_city_id,
                arrival_city_id,
                formation_datetime,
                manager_id,
                driver_id,
                assistant_id
            ) VALUES (
                @trainNumber,
                @departureCityId,
                @arrivalCityId,
                @formationDatetime,
                @managerId,
                @driverId,
                @assistantId
            );
        ";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainNumber", newTrain.TrainNumber);
        cmd.Parameters.AddWithValue("@departureCityId", newTrain.DepartureCityId);
        cmd.Parameters.AddWithValue("@arrivalCityId", newTrain.ArrivalCityId);
        cmd.Parameters.AddWithValue("@formationDatetime", newTrain.FormationDatetime);
        cmd.Parameters.AddWithValue("@managerId", newTrain.ManagerId);
        cmd.Parameters.AddWithValue("@driverId", newTrain.DriverId);
        cmd.Parameters.AddWithValue("@assistantId", newTrain.AssistantId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Update(MySqlConnection conn, int trainId, UpdateTrain updateTrain)
    {
        await conn.OpenAsync();

        const string query = @"
            UPDATE trains SET
                train_number = @trainNumber,
                departure_city_id = @departureCityId,
                arrival_city_id = @arrivalCityId,
                formation_datetime = @formationDatetime,
                manager_id = @managerId,
                driver_id = @driverId,
                assistant_id = @assistantId
            WHERE train_id = @trainId
        ";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainId", trainId);
        cmd.Parameters.AddWithValue("@trainNumber", updateTrain.TrainNumber);
        cmd.Parameters.AddWithValue("@departureCityId", updateTrain.DepartureCityId);
        cmd.Parameters.AddWithValue("@arrivalCityId", updateTrain.ArrivalCityId);
        cmd.Parameters.AddWithValue("@formationDatetime", updateTrain.FormationDatetime);
        cmd.Parameters.AddWithValue("@managerId", updateTrain.ManagerId);
        cmd.Parameters.AddWithValue("@driverId", updateTrain.DriverId);
        cmd.Parameters.AddWithValue("@assistantId", updateTrain.AssistantId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Delete(MySqlConnection conn, int trainId)
    {
        await conn.OpenAsync();

        const string query = "DELETE FROM trains WHERE train_id = @trainId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@trainId", trainId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
