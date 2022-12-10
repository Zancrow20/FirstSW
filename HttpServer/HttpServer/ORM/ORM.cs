using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using HttpServer.Models;

namespace HttpServer.ORM;

public class ORM
{
    private SqlConnection _connection;
    private SqlCommand _command = new SqlCommand();

    public ORM(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        _command = _connection.CreateCommand();
    }

    public async Task<IEnumerable<T>> ExecuteQuery<T>(string query)
    {
        IList<T> list = new List<T>();
        var type = typeof(T);
        
        _command.CommandText = query;
        await _connection.OpenAsync();
        var reader = await _command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var obj = Activator.CreateInstance<T>();
            type.GetProperties().ToList().ForEach(p=>
                p.SetValue(obj, reader[p.Name.ToLower()]));
            
            list.Add(obj);
        }
        await _connection.CloseAsync();

        return list;
    }

    public async Task<int> ExecuteNonQuery<T>(string query)
    {
        _command.CommandText = query;
        await _connection.OpenAsync();
        var noAffectedRows = await _command.ExecuteNonQueryAsync(); 
        _command.Parameters.Clear();
        await _connection.CloseAsync();
        return noAffectedRows;
    }

    public async Task<IEnumerable<T>> Select<T>()
    {
        var query = $"SELECT * FROM {typeof(T).Name}";
        return await ExecuteQuery<T>(query);
    }

    public async Task<IEnumerable<T>> Select<T>(string propertyValue, string propertyName)
    {
        var query = $"SELECT * FROM {typeof(T).Name} " +
                    $"WHERE {propertyName.ToLower()} = '{propertyValue}'";
        return await ExecuteQuery<T>(query);
    }

    public async Task<T?> Select<T>(int id)
    {
        var query = $"SELECT * FROM {typeof(T).Name} WHERE id = {id}";
        return (await ExecuteQuery<T>(query)).FirstOrDefault();
    }

    public async Task<int> Update<T>(T entity)
    {
        var sb = new StringBuilder();
        var id = GetId(entity);
        var properties = entity?.GetType().GetProperties();
        foreach (var property in properties!)
            sb.Append($"{property.Name} = {property.GetValue(entity)}, ");
        
        var nonQuery = $"UPDATE {typeof(T).Name} SET {sb} WHERE id = {id}";
        return await ExecuteNonQuery<T>(nonQuery);
    }

    public async Task<int> Delete<T>(T entity)
    {
        var id = GetId(entity);
        var nonQuery = $"DELETE FROM {typeof(T).Name} WHERE id = {id}";
        return await ExecuteNonQuery<T>(nonQuery);
    }

    public async Task<int> Insert<T>(T entity)
    {
        var args = GetProperties(entity).Where(info => info.Name.ToLower() != "id");
        var names = args.Select(value => value.Name.ToLower());
        var parameters = names.Select(value => "@" + value);
        foreach (var parameter in args)
        {
            var sqlParameter = new SqlParameter($"@{parameter.Name.ToLower()}", parameter.GetValue(entity));
            _command.Parameters.Add(sqlParameter);
        }

        var nonQuery = 
            $"INSERT INTO {typeof(T).Name} ({string.Join(", ", names)}) VALUES ({string.Join(", ", parameters)})";
        return await ExecuteNonQuery<T>(nonQuery);
    }

    public async Task<int> InsertWithId<T>(T entity)
    {
        var args = GetProperties(entity);
        var names = args.Select(value => value.Name.ToLower());
        var parameters = names.Select(value => "@" + value);
        foreach (var parameter in args)
        {
            var sqlParameter = new SqlParameter($"@{parameter.Name.ToLower()}", parameter.GetValue(entity));
            _command.Parameters.Add(sqlParameter);
        }

        var nonQuery = 
            $"INSERT INTO {typeof(T).Name} ({string.Join(", ", names)}) VALUES ({string.Join(", ", parameters)})";
        return await ExecuteNonQuery<T>(nonQuery);
    }

    private static IEnumerable<object?> GetPropertiesValues<T>(T entity) =>
        typeof(T).GetProperties().Select(property => property.GetValue(entity));

    private static PropertyInfo[] GetProperties<T>(T entity) =>
        typeof(T).GetProperties();

    private static object? GetId<T>(T entity) =>
        typeof(T).GetProperties()?.FirstOrDefault(id => Regex.IsMatch(id.Name.ToLower(),"id"))?.GetValue(entity);
}