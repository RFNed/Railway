using MySqlConnector;
using RailwayApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RailwayApp.Data;

public class CityRepository
{
    public static async Task<List<City>> GetAll(MySqlConnection conn)
    {
        List<City> result = [];

        await conn.OpenAsync();

        string query = "SELECT * FROM cities;";

        await using var cmd = new MySqlCommand(query, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new City
            {
                CityId = reader.GetInt32("city_id"),
                CityName = reader.GetString("city_name"),
            });
        }

        return result;
    }

    public static async Task<City?> GetById(MySqlConnection conn, int cityId)
    {
        await conn.OpenAsync();

        const string query = "SELECT * FROM cities WHERE city_id = @cityId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@cityId", cityId);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new City
            {
                CityId = reader.GetInt32("city_id"),
                CityName = reader.GetString("city_name"),
            };
        }

        return null;
    }

    public static async Task<bool> Create(MySqlConnection conn, string cityName)
    {
        await conn.OpenAsync();

        const string query = "INSERT INTO cities (city_name) VALUES (@cityName);";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@cityName", cityName);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Update(MySqlConnection conn, int cityId, string cityName)
    {
        await conn.OpenAsync();

        const string query = "UPDATE cities SET city_name = @cityName WHERE city_id = @cityId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@cityId", cityId);
        cmd.Parameters.AddWithValue("@cityName", cityName);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Delete(MySqlConnection conn, int cityId)
    {
        await conn.OpenAsync();

        const string query = "DELETE FROM cities WHERE city_id = @cityId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@cityId", cityId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }
}
