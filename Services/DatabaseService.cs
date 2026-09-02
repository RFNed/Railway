using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using RailwayApp.Models;

namespace RailwayApp.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    private MySqlConnection GetConnection() => new(_connectionString);
    public async Task<List<TrainItem>> GetTrainsAsync()
    {
        var result = new List<TrainItem>();
        await using var connection = GetConnection();
        await connection.OpenAsync();

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
            ORDER BY t.train_id DESC;";

        await using var cmd = new MySqlCommand(query, connection);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new TrainItem
            {
                TrainId = reader.GetInt32("train_id"),
                TrainNumber = reader.GetString("train_number"),
                FormationDatetime = reader.GetDateTime("formation_datetime"),
                DepartureCityId = reader.GetInt32("departure_city_id"),
                DepartureCityName = reader.GetString("dep_city"),
                ArrivalCityId = reader.GetInt32("arrival_city_id"),
                ArrivalCityName = reader.GetString("arr_city"),
                ManagerId = reader.GetInt32("manager_id"),
                ManagerName = reader.GetString("manager_name"),
                DriverId = reader.GetInt32("driver_id"),
                DriverName = reader.GetString("driver_name"),
                AssistantId = reader.GetInt32("assistant_id"),
                AssistantName = reader.GetString("assistant_name")
            });
        }

        return result;
    }
    public async Task<bool> UpdateTrainAsync(TrainItem train)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();

        const string query = @"
            UPDATE trains SET
                train_number = @trainNumber,
                departure_city_id = @depCityId,
                arrival_city_id = @arrCityId,
                manager_id = @managerId,
                driver_id = @driverId,
                assistant_id = @assistantId
            WHERE train_id = @trainId;";

        await using var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@trainNumber", train.TrainNumber);
        cmd.Parameters.AddWithValue("@depCityId", train.DepartureCityId);
        cmd.Parameters.AddWithValue("@arrCityId", train.ArrivalCityId);
        cmd.Parameters.AddWithValue("@managerId", train.ManagerId);
        cmd.Parameters.AddWithValue("@driverId", train.DriverId);
        cmd.Parameters.AddWithValue("@assistantId", train.AssistantId);
        cmd.Parameters.AddWithValue("@trainId", train.TrainId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }
    public async Task<List<LookupItem>> GetCitiesAsync()
    {
        var list = new List<LookupItem>();
        await using var connection = GetConnection();
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand("SELECT city_id, city_name FROM cities ORDER BY city_name", connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new LookupItem { Id = reader.GetInt32("city_id"), Name = reader.GetString("city_name") });
        }
        return list;
    }
    public async Task<List<LookupItem>> GetEmployeesByRoleAsync(string role)
    {
        var list = new List<LookupItem>();
        await using var connection = GetConnection();
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand("SELECT employee_id, CONCAT(last_name, ' ', first_name) AS full_name FROM employees WHERE job_title = @role", connection);
        cmd.Parameters.AddWithValue("@role", role);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new LookupItem { Id = reader.GetInt32("employee_id"), Name = reader.GetString("full_name") });
        }
        return list;
    }
}