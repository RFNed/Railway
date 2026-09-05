using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using RailwayApp.Models;

namespace RailwayApp.Data;

public class EmployeeRepository
{
    private static Employee MapEmployee(MySqlDataReader reader)
    {
        return new Employee
        {
            EmployeeId = reader.GetInt32("employee_id"),
            LastName = reader.GetString("last_name"),
            FirstName = reader.GetString("first_name"),
            MiddleName = reader.GetString("middle_name"),
            BirthDate = reader.GetDateOnly("birth_date"),
            Phone = reader.GetString("phone"),
            Email = reader.GetString("email"),
            JobTitle = reader.GetString("job_title"),
            Rating = reader.GetInt32("rating"),
        };
    }

    public static async Task<List<Employee>> GetAll(MySqlConnection conn, string? jobTitle)
    {
        List<Employee> result = [];

        await conn.OpenAsync();

        string query = "SELECT * FROM employees";

        if (!string.IsNullOrEmpty(jobTitle))
            query += " WHERE job_title = @jobTitle";

        await using var cmd = new MySqlCommand(query, conn);
        if (!string.IsNullOrEmpty(jobTitle))
            cmd.Parameters.AddWithValue("@jobTitle", jobTitle);

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(MapEmployee(reader));
        }

        return result;
    }

    public static async Task<Employee?> GetById(MySqlConnection conn, int employeeId)
    {
        await conn.OpenAsync();

        const string query = "SELECT * FROM employees WHERE employee_id = @employee_id;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@employee_id", employeeId);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapEmployee(reader);
        }

        return null;
    }

    public static async Task<bool> Create(MySqlConnection conn, NewEmployee newEmployee)
    {
        await conn.OpenAsync();

        const string query = @"INSERT INTO employees (
            last_name,
            first_name,
            middle_name,
            birth_date,
            phone,
            email,
            job_title,
            rating
        ) VALUES (
            @lastName,
            @firstName,
            @middleName,
            @birthDate,
            @phone,
            @email,
            @jobTitle,
            0
        )";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@lastName", newEmployee.LastName);
        cmd.Parameters.AddWithValue("@firstName", newEmployee.FirstName);
        cmd.Parameters.AddWithValue("@middleName", newEmployee.MiddleName);
        cmd.Parameters.AddWithValue("@birthDate", newEmployee.BirthDate);
        cmd.Parameters.AddWithValue("@phone", newEmployee.Phone);
        cmd.Parameters.AddWithValue("@email", newEmployee.Email);
        cmd.Parameters.AddWithValue("@jobTitle", newEmployee.JobTitle);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Update(MySqlConnection conn, int employeeId, Employee updateEmployee)
    {
        await conn.OpenAsync();

        const string query = @"
            UPDATE employees SET
                last_name = @lastName,
                first_name = @firstName,
                middle_name = @middleName,
                birth_date = @birthDate,
                phone = @phone,
                email = @email,
                job_title = @jobTitle,
                rating = @rating
            WHERE employee_id = @employeeId
        ";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@employeeId", employeeId);
        cmd.Parameters.AddWithValue("@lastName", updateEmployee.LastName);
        cmd.Parameters.AddWithValue("@firstName", updateEmployee.FirstName);
        cmd.Parameters.AddWithValue("@middleName", updateEmployee.MiddleName);
        cmd.Parameters.AddWithValue("@birthDate", updateEmployee.BirthDate);
        cmd.Parameters.AddWithValue("@phone", updateEmployee.Phone);
        cmd.Parameters.AddWithValue("@email", updateEmployee.Email);
        cmd.Parameters.AddWithValue("@jobTitle", updateEmployee.JobTitle);
        cmd.Parameters.AddWithValue("@rating", updateEmployee.Rating);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public static async Task<bool> Delete(MySqlConnection conn, int employeeId)
    {
        await conn.OpenAsync();

        const string query = "DELETE FROM employees WHERE employee_id = @employeeId;";

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@employeeId", employeeId);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }
}