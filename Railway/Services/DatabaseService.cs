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

    public async Task<List<TrainDetails>> GetTrainsAsync()
    {
        var result = new List<TrainDetails>();
        await using var connection = GetConnection();
        await connection.OpenAsync();

        const string query = @"
            SELECT t.train_id, t.train_number, t.formation_datetime,
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
            ORDER BY t.formation_datetime DESC, t.train_id DESC;";

        await using var cmd = new MySqlCommand(query, connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new TrainDetails
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

    public async Task<List<City>> GetCitiesAsync()
    {
        var result = new List<City>();
        await using var connection = GetConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand("SELECT city_id, city_name FROM cities ORDER BY city_name;", connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(new City { CityId = reader.GetInt32("city_id"), CityName = reader.GetString("city_name") });
        return result;
    }

    public async Task<List<LookupItem>> GetEmployeesByRoleAsync(string role)
    {
        var result = new List<LookupItem>();
        await using var connection = GetConnection();
        await connection.OpenAsync();
        const string query = "SELECT employee_id, CONCAT(last_name, ' ', first_name, IF(middle_name = '', '', CONCAT(' ', middle_name))) AS full_name FROM employees WHERE job_title = @role ORDER BY last_name, first_name;";
        await using var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@role", role);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(new LookupItem { Id = reader.GetInt32("employee_id"), Name = reader.GetString("full_name") });
        return result;
    }

    public async Task RecalculateEmployeeRatingsAsync()
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();
        const string query = @"
            UPDATE employees e
            SET e.rating = CASE
                WHEN (SELECT COUNT(*) FROM trains) = 0 THEN 0
                ELSE ROUND(100.0 * (
                    SELECT COUNT(DISTINCT t.train_id)
                    FROM trains t
                    WHERE t.manager_id = e.employee_id
                       OR t.driver_id = e.employee_id
                       OR t.assistant_id = e.employee_id
                ) / (SELECT COUNT(*) FROM trains))
            END;";
        await using var cmd = new MySqlCommand(query, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<Employee>> GetEmployeesAsync()
    {
        var result = new List<Employee>();
        await using var connection = GetConnection();
        await connection.OpenAsync();
        const string query = @"
            SELECT e.employee_id, e.last_name, e.first_name, e.middle_name, e.birth_date, e.phone, e.email, e.job_title,
                   CAST(CASE WHEN totals.total_count = 0 THEN 0
                             ELSE ROUND(100.0 * COUNT(DISTINCT CASE WHEN t.train_id IS NOT NULL THEN t.train_id END) / totals.total_count) END AS UNSIGNED) AS calculated_rating
            FROM employees e
            LEFT JOIN trains t ON t.manager_id = e.employee_id OR t.driver_id = e.employee_id OR t.assistant_id = e.employee_id
            CROSS JOIN (SELECT COUNT(*) AS total_count FROM trains) totals
            GROUP BY e.employee_id, e.last_name, e.first_name, e.middle_name, e.birth_date, e.phone, e.email, e.job_title, totals.total_count
            ORDER BY e.last_name, e.first_name;";
        await using var cmd = new MySqlCommand(query, connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Employee
            {
                EmployeeId = reader.GetInt32("employee_id"),
                LastName = reader.GetString("last_name"),
                FirstName = reader.GetString("first_name"),
                MiddleName = reader.GetString("middle_name"),
                BirthDate = reader.GetDateOnly("birth_date"),
                Phone = reader.GetString("phone"),
                Email = reader.GetString("email"),
                JobTitle = reader.GetString("job_title"),
                Rating = reader.GetInt32("calculated_rating")
            });
        }
        return result;
    }

    public async Task<bool> CreateEmployeeAsync(NewEmployee employee)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();
        const string query = "INSERT INTO employees (last_name, first_name, middle_name, birth_date, phone, email, job_title, rating) VALUES (@lastName, @firstName, @middleName, @birthDate, @phone, @email, @jobTitle, 0);";
        await using var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@lastName", employee.LastName); cmd.Parameters.AddWithValue("@firstName", employee.FirstName); cmd.Parameters.AddWithValue("@middleName", employee.MiddleName);
        cmd.Parameters.AddWithValue("@birthDate", employee.BirthDate); cmd.Parameters.AddWithValue("@phone", employee.Phone); cmd.Parameters.AddWithValue("@email", employee.Email); cmd.Parameters.AddWithValue("@jobTitle", employee.JobTitle);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> UpdateEmployeeAsync(int employeeId, UpdateEmployee employee)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();
        const string query = "UPDATE employees SET last_name=@lastName, first_name=@firstName, middle_name=@middleName, birth_date=@birthDate, phone=@phone, email=@email, job_title=@jobTitle, rating=@rating WHERE employee_id=@employeeId;";
        await using var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@employeeId", employeeId); cmd.Parameters.AddWithValue("@lastName", employee.LastName); cmd.Parameters.AddWithValue("@firstName", employee.FirstName); cmd.Parameters.AddWithValue("@middleName", employee.MiddleName);
        cmd.Parameters.AddWithValue("@birthDate", employee.BirthDate); cmd.Parameters.AddWithValue("@phone", employee.Phone); cmd.Parameters.AddWithValue("@email", employee.Email); cmd.Parameters.AddWithValue("@jobTitle", employee.JobTitle); cmd.Parameters.AddWithValue("@rating", employee.Rating);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteEmployeeAsync(int employeeId)
    {
        await using var connection = GetConnection(); await connection.OpenAsync();
        await using var cmd = new MySqlCommand("DELETE FROM employees WHERE employee_id=@employeeId;", connection);
        cmd.Parameters.AddWithValue("@employeeId", employeeId);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> CreateTrainAsync(NewTrain train)
    {
        await using var connection = GetConnection(); await connection.OpenAsync();
        const string query = "INSERT INTO trains (train_number, departure_city_id, arrival_city_id, formation_datetime, manager_id, driver_id, assistant_id) VALUES (@number,@departure,@arrival,@formation,@manager,@driver,@assistant);";
        await using var cmd = new MySqlCommand(query, connection);
        AddTrainParameters(cmd, train.TrainNumber, train.DepartureCityId, train.ArrivalCityId, train.FormationDatetime, train.ManagerId, train.DriverId, train.AssistantId);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> UpdateTrainAsync(Train train)
    {
        await using var connection = GetConnection(); await connection.OpenAsync();
        const string query = "UPDATE trains SET train_number=@number, departure_city_id=@departure, arrival_city_id=@arrival, formation_datetime=@formation, manager_id=@manager, driver_id=@driver, assistant_id=@assistant WHERE train_id=@id;";
        await using var cmd = new MySqlCommand(query, connection);
        AddTrainParameters(cmd, train.TrainNumber, train.DepartureCityId, train.ArrivalCityId, train.FormationDatetime, train.ManagerId, train.DriverId, train.AssistantId); cmd.Parameters.AddWithValue("@id", train.TrainId);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static void AddTrainParameters(MySqlCommand cmd, string number, int departure, int arrival, DateTime formation, int manager, int driver, int assistant)
    {
        cmd.Parameters.AddWithValue("@number", number); cmd.Parameters.AddWithValue("@departure", departure); cmd.Parameters.AddWithValue("@arrival", arrival); cmd.Parameters.AddWithValue("@formation", formation); cmd.Parameters.AddWithValue("@manager", manager); cmd.Parameters.AddWithValue("@driver", driver); cmd.Parameters.AddWithValue("@assistant", assistant);
    }

    public async Task<bool> DeleteTrainAsync(int trainId)
    {
        await using var connection = GetConnection(); await connection.OpenAsync();
        await using var cmd = new MySqlCommand("DELETE FROM trains WHERE train_id=@id;", connection); cmd.Parameters.AddWithValue("@id", trainId); return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<TrainComposition> GetCompositionAsync(int trainId)
    {
        var composition = new TrainComposition();
        await using var connection = GetConnection(); await connection.OpenAsync();
        const string locoQuery = "SELECT l.locomotive_id, l.locomotive_number FROM train_locomotives tl JOIN locomotives l ON l.locomotive_id=tl.locomotive_id WHERE tl.train_id=@trainId ORDER BY tl.train_locomotive_id;";
        await using (var cmd = new MySqlCommand(locoQuery, connection)) { cmd.Parameters.AddWithValue("@trainId", trainId); await using var reader = await cmd.ExecuteReaderAsync(); while (await reader.ReadAsync()) composition.Locomotives.Add(new Locomotive { LocomotiveId=reader.GetInt32("locomotive_id"), LocomotiveNumber=reader.GetString("locomotive_number") }); }
        const string wagonQuery = "SELECT tw.train_wagon_id, w.wagon_id, w.wagon_number, w.wagon_type_id, wt.wagon_code, wt.description, tw.is_loaded FROM train_wagons tw JOIN wagons w ON w.wagon_id=tw.wagon_id JOIN wagon_types wt ON wt.wagon_type_id=w.wagon_type_id WHERE tw.train_id=@trainId ORDER BY tw.train_wagon_id;";
        await using (var cmd = new MySqlCommand(wagonQuery, connection)) { cmd.Parameters.AddWithValue("@trainId", trainId); await using var reader = await cmd.ExecuteReaderAsync(); while (await reader.ReadAsync()) { composition.Wagons.Add(new WagonWithType { WagonId=reader.GetInt32("wagon_id"), WagonNumber=reader.GetString("wagon_number"), WagonTypeId=reader.GetInt32("wagon_type_id"), WagonTypeCode=reader.GetString("wagon_code"), TypeDescription=reader.GetString("description") }); composition.TrainWagons.Add(new TrainWagon { TrainWagonId=reader.GetInt32("train_wagon_id"), TrainId=trainId, WagonId=reader.GetInt32("wagon_id"), IsLoaded=reader.GetBoolean("is_loaded") }); } }
        return composition;
    }

    public async Task<List<Locomotive>> GetLocomotivesAsync()
    {
        var result=new List<Locomotive>(); await using var connection=GetConnection(); await connection.OpenAsync(); await using var cmd=new MySqlCommand("SELECT locomotive_id, locomotive_number FROM locomotives ORDER BY locomotive_number;",connection); await using var reader=await cmd.ExecuteReaderAsync(); while(await reader.ReadAsync()) result.Add(new Locomotive{LocomotiveId=reader.GetInt32("locomotive_id"),LocomotiveNumber=reader.GetString("locomotive_number")}); return result;
    }

    public async Task<List<WagonWithType>> GetWagonsAsync()
    {
        var result=new List<WagonWithType>(); await using var connection=GetConnection(); await connection.OpenAsync(); const string q="SELECT w.wagon_id,w.wagon_number,w.wagon_type_id,wt.wagon_code,wt.description FROM wagons w JOIN wagon_types wt ON wt.wagon_type_id=w.wagon_type_id ORDER BY w.wagon_number;"; await using var cmd=new MySqlCommand(q,connection); await using var reader=await cmd.ExecuteReaderAsync(); while(await reader.ReadAsync()) result.Add(new WagonWithType{WagonId=reader.GetInt32("wagon_id"),WagonNumber=reader.GetString("wagon_number"),WagonTypeId=reader.GetInt32("wagon_type_id"),WagonTypeCode=reader.GetString("wagon_code"),TypeDescription=reader.GetString("description")}); return result;
    }

    public async Task<bool> AddLocomotiveToTrainAsync(int trainId,int locomotiveId)
    {
        await using var connection=GetConnection(); await connection.OpenAsync(); await using var countCmd=new MySqlCommand("SELECT COUNT(*) FROM train_locomotives WHERE train_id=@trainId;",connection); countCmd.Parameters.AddWithValue("@trainId",trainId); if(Convert.ToInt32(await countCmd.ExecuteScalarAsync())>=2) throw new InvalidOperationException("В состав можно назначить не более двух локомотивов."); await using var cmd=new MySqlCommand("INSERT INTO train_locomotives(train_id,locomotive_id) VALUES(@trainId,@locomotiveId);",connection); cmd.Parameters.AddWithValue("@trainId",trainId); cmd.Parameters.AddWithValue("@locomotiveId",locomotiveId); return await cmd.ExecuteNonQueryAsync()>0;
    }

    public async Task<int> GetTrainLocomotiveIdAsync(int trainId, int locomotiveId)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();
        await using var cmd = new MySqlCommand("SELECT train_locomotive_id FROM train_locomotives WHERE train_id=@trainId AND locomotive_id=@locomotiveId LIMIT 1;", connection);
        cmd.Parameters.AddWithValue("@trainId", trainId);
        cmd.Parameters.AddWithValue("@locomotiveId", locomotiveId);
        object? value = await cmd.ExecuteScalarAsync();
        return value == null ? 0 : Convert.ToInt32(value);
    }

    public async Task<bool> RemoveLocomotiveFromTrainAsync(int trainLocomotiveId)
    { await using var connection=GetConnection(); await connection.OpenAsync(); await using var cmd=new MySqlCommand("DELETE FROM train_locomotives WHERE train_locomotive_id=@id;",connection); cmd.Parameters.AddWithValue("@id",trainLocomotiveId); return await cmd.ExecuteNonQueryAsync()>0; }

    public async Task<bool> AddWagonToTrainAsync(int trainId,int wagonId,bool loaded)
    { await using var connection=GetConnection(); await connection.OpenAsync(); await using var cmd=new MySqlCommand("INSERT INTO train_wagons(train_id,wagon_id,is_loaded) VALUES(@trainId,@wagonId,@loaded);",connection); cmd.Parameters.AddWithValue("@trainId",trainId); cmd.Parameters.AddWithValue("@wagonId",wagonId); cmd.Parameters.AddWithValue("@loaded",loaded); return await cmd.ExecuteNonQueryAsync()>0; }

    public async Task<bool> UpdateTrainWagonLoadAsync(int trainWagonId,bool loaded)
    { await using var connection=GetConnection(); await connection.OpenAsync(); await using var cmd=new MySqlCommand("UPDATE train_wagons SET is_loaded=@loaded WHERE train_wagon_id=@id;",connection); cmd.Parameters.AddWithValue("@loaded",loaded); cmd.Parameters.AddWithValue("@id",trainWagonId); return await cmd.ExecuteNonQueryAsync()>0; }

    public async Task<bool> RemoveWagonFromTrainAsync(int trainWagonId)
    { await using var connection=GetConnection(); await connection.OpenAsync(); await using var cmd=new MySqlCommand("DELETE FROM train_wagons WHERE train_wagon_id=@id;",connection); cmd.Parameters.AddWithValue("@id",trainWagonId); return await cmd.ExecuteNonQueryAsync()>0; }

    public async Task<List<Locomotive>> GetAllFreeLocomotivesAsync(int trainId)
    {
        var result=new List<Locomotive>(); await using var connection=GetConnection(); await connection.OpenAsync(); const string q="SELECT l.locomotive_id,l.locomotive_number FROM locomotives l WHERE NOT EXISTS(SELECT 1 FROM train_locomotives tl WHERE tl.locomotive_id=l.locomotive_id) ORDER BY l.locomotive_number;"; await using var cmd=new MySqlCommand(q,connection); cmd.Parameters.AddWithValue("@trainId",trainId); await using var reader=await cmd.ExecuteReaderAsync(); while(await reader.ReadAsync()) result.Add(new Locomotive{LocomotiveId=reader.GetInt32("locomotive_id"),LocomotiveNumber=reader.GetString("locomotive_number")}); return result;
    }

    public async Task<List<WagonWithType>> GetAllFreeWagonsAsync(int trainId)
    {
        var result=new List<WagonWithType>(); await using var connection=GetConnection(); await connection.OpenAsync(); const string q="SELECT w.wagon_id,w.wagon_number,w.wagon_type_id,wt.wagon_code,wt.description FROM wagons w JOIN wagon_types wt ON wt.wagon_type_id=w.wagon_type_id WHERE NOT EXISTS(SELECT 1 FROM train_wagons tw WHERE tw.wagon_id=w.wagon_id) ORDER BY w.wagon_number;"; await using var cmd=new MySqlCommand(q,connection); cmd.Parameters.AddWithValue("@trainId",trainId); await using var reader=await cmd.ExecuteReaderAsync(); while(await reader.ReadAsync()) result.Add(new WagonWithType{WagonId=reader.GetInt32("wagon_id"),WagonNumber=reader.GetString("wagon_number"),WagonTypeId=reader.GetInt32("wagon_type_id"),WagonTypeCode=reader.GetString("wagon_code"),TypeDescription=reader.GetString("description")}); return result;
    }

    public async Task<ManagerTrainStatisticsData> GetManagerStatisticsAsync(int managerId, DateTime from, DateTime to)
    {
        await using var connection=GetConnection(); await connection.OpenAsync(); const string q="SELECT COUNT(*) AS total_count, SUM(CASE WHEN manager_id=@managerId THEN 1 ELSE 0 END) AS manager_count FROM trains WHERE formation_datetime >= @from AND formation_datetime <= @to;"; await using var cmd=new MySqlCommand(q,connection); cmd.Parameters.AddWithValue("@managerId",managerId); cmd.Parameters.AddWithValue("@from",from); cmd.Parameters.AddWithValue("@to",to); await using var reader=await cmd.ExecuteReaderAsync(); if(!await reader.ReadAsync()) return new ManagerTrainStatisticsData(); return new ManagerTrainStatisticsData{ManagerTrainCount=reader.IsDBNull(reader.GetOrdinal("manager_count"))?0:Convert.ToInt32(reader["manager_count"]),TotalTrainCount=Convert.ToInt32(reader["total_count"])};
    }
    public async Task<bool> CreateLocomotiveAsync(string number)
    { await using var connection=GetConnection(); await connection.OpenAsync(); await using var cmd=new MySqlCommand("INSERT INTO locomotives(locomotive_number) VALUES(@number);",connection); cmd.Parameters.AddWithValue("@number",number); return await cmd.ExecuteNonQueryAsync()>0; }

    public async Task<bool> UpdateLocomotiveAsync(int id,string number)
    { await using var connection=GetConnection(); await connection.OpenAsync(); await using var cmd=new MySqlCommand("UPDATE locomotives SET locomotive_number=@number WHERE locomotive_id=@id;",connection); cmd.Parameters.AddWithValue("@id",id); cmd.Parameters.AddWithValue("@number",number); return await cmd.ExecuteNonQueryAsync()>0; }

    public async Task<bool> DeleteLocomotiveAsync(int id)
    { await using var connection=GetConnection(); await connection.OpenAsync(); await using var cmd=new MySqlCommand("DELETE FROM locomotives WHERE locomotive_id=@id;",connection); cmd.Parameters.AddWithValue("@id",id); return await cmd.ExecuteNonQueryAsync()>0; }

    public async Task<bool> CreateWagonAsync(string number,int typeId)
    { await using var connection=GetConnection(); await connection.OpenAsync(); await using var cmd=new MySqlCommand("INSERT INTO wagons(wagon_number,wagon_type_id) VALUES(@number,@typeId);",connection); cmd.Parameters.AddWithValue("@number",number); cmd.Parameters.AddWithValue("@typeId",typeId); return await cmd.ExecuteNonQueryAsync()>0; }

    public async Task<bool> UpdateWagonAsync(int id,string number,int typeId)
    { await using var connection=GetConnection(); await connection.OpenAsync(); await using var cmd=new MySqlCommand("UPDATE wagons SET wagon_number=@number,wagon_type_id=@typeId WHERE wagon_id=@id;",connection); cmd.Parameters.AddWithValue("@id",id); cmd.Parameters.AddWithValue("@number",number); cmd.Parameters.AddWithValue("@typeId",typeId); return await cmd.ExecuteNonQueryAsync()>0; }

    public async Task<bool> DeleteWagonAsync(int id)
    { await using var connection=GetConnection(); await connection.OpenAsync(); await using var cmd=new MySqlCommand("DELETE FROM wagons WHERE wagon_id=@id;",connection); cmd.Parameters.AddWithValue("@id",id); return await cmd.ExecuteNonQueryAsync()>0; }

    public async Task<List<WagonType>> GetWagonTypesAsync()
    { var result=new List<WagonType>(); await using var connection=GetConnection(); await connection.OpenAsync(); await using var cmd=new MySqlCommand("SELECT wagon_type_id,wagon_code,description FROM wagon_types ORDER BY wagon_code;",connection); await using var reader=await cmd.ExecuteReaderAsync(); while(await reader.ReadAsync()) result.Add(new WagonType{WagonTypeId=reader.GetInt32("wagon_type_id"),WagonCode=reader.GetString("wagon_code"),Description=reader.GetString("description")}); return result; }

}

public class ManagerTrainStatisticsData
{
    public int ManagerTrainCount { get; set; }
    public int TotalTrainCount { get; set; }
}
