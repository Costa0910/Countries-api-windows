//using System.Data.SQLite;

using ClassLibraryAPI.Models;
using Microsoft.Data.Sqlite;

namespace ClassLibraryAPI.Services;
/// <summary>
/// Class to craete db, Countries Table, and manipulate it data
/// </summary>
public class SqLiteService
{
    readonly SqliteConnection _connection;
    SqliteCommand _command;

    /// <summary>
    /// Save countries to local sqlite db
    /// </summary>
    public SqLiteService()
    {
        if (Directory.Exists("Img") == false)
            Directory.CreateDirectory("Img");
        if (Directory.Exists("SQLiteDB") == false)
            Directory.CreateDirectory("SQLiteDB");
        const string path = "SQLiteDB/Countries.sqlite";
        _connection = new($"Data Source={path}");
    }

    /// <summary>
    /// Save given countries to db
    /// </summary>
    /// <param name="countries">Countries to save</param>
    /// <param name="status">IProgress param to report status of saving countries to db</param>
    public async Task<Response> SaveCountriesAsync(List<Country> countries, IProgress<double> status)
    {
        var reponse = await CreateDbAsync();
        if (reponse.Status == false) // if table not created return response
            return reponse;

        try
        {
            await _connection.OpenAsync();

            const string sql = @"
                INSERT INTO Countries (Name, Capital, Region, Subregion, Gini, Languages, Population, Continents, Flags) 
                VALUES (@Name, @Capital, @Region, @Subregion, @Gini, @Languages, @Population, @Continents, @Flags)";

            double count = 0;
            double total = countries.Count;

            foreach (var country in countries)
            {
                await using (_command = new(sql, _connection))
                {
                    _command.Parameters.AddWithValue("@Name", $"{country.Name.Common};{country.Name.Official}");
                    _command.Parameters.AddWithValue("@Capital", string.Join(";", country.Capital));
                    _command.Parameters.AddWithValue("@Region", country.Region);
                    _command.Parameters.AddWithValue("@Subregion", country.Subregion);
                    _command.Parameters.AddWithValue("@Gini", ParseDicToString(country.Gini));
                    _command.Parameters.AddWithValue("@Languages", ParseDicToString(country.Languages));
                    _command.Parameters.AddWithValue("@Population", country.Population);
                    _command.Parameters.AddWithValue("@Continents", string.Join(";", country.Continents));

                    var imgStream = await DownloadImgAsync(country.Flags);
                    _command.Parameters.AddWithValue("@Flags", imgStream);
                    await _command.ExecuteNonQueryAsync();
                }
                count++;

                // update Progress bar status here
                status.Report(count / total * 100);
            }

            return new Response () { Status = true, Message = "Países salvos com sucesso" };
        }
        catch (Exception e)
        {
            return new Response () { Status = false, Message = e.Message };
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    /// <summary>
    /// Download Image from remote url
    /// </summary>
    /// <param name="countryFlags">Url of Image to download</param>
    /// <returns>Return a bytes of downloaded image</returns>
    async Task<byte[]> DownloadImgAsync(Flags countryFlags)
    {
        var url = countryFlags.Png ?? countryFlags.Svg;

        using var client = new HttpClient();

        try
        {
            return await client.GetByteArrayAsync(url);
        }
        catch (Exception e)
        {
            // DialogService.ShowMessage("Erro ao tentar baixar imagem", $"Nao foi possivel baixar a imagem: {e.Message}");

            return null;
        }
    }

    /// <summary>
    /// Format Gini to string separate by ;
    /// </summary>
    /// <param name="countryGini">Ginis to concat</param>
    /// <returns>Return a concat string of given dictionary</returns>
    string ParseDicToString(Dictionary<string, double> countryGini)
    {
        var str = string.Empty;

        foreach (var country in countryGini)
        {
            if (string.IsNullOrEmpty(str))
                str = $"{country.Key}__{country.Value}";
            else
                str = string.Join(";", str, $"{country.Key}__{country.Value}");
        }

        return str;
    }

    /// <summary>
    /// Format languages to string separate by ;
    /// </summary>
    /// <param name="countrycountry">Languages to concat</param>
    /// <returns>Return a concat string of given dictionary</returns>
    string ParseDicToString(Dictionary<string, string> countrycountry)
    {
        var str = string.Empty;

        foreach (var country in countrycountry)
        {
            if (string.IsNullOrEmpty(str))
                str = $"{country.Key}__{country.Value}";
            else
                str = string.Join(";", str, $"{country.Key}__{country.Value}");
        }

        return str;
    }

    /// <summary>
    /// Get all countries in Countries Table
    /// </summary>
    /// <param name="gettingStatus"></param>
    /// <returns></returns>
    public async Task<Response> GetCountriesAsync(IProgress<double> gettingStatus)
    {
        List<Country> countries = [];

        try
        {
            await _connection.OpenAsync();
            const string sql = "select * from Countries";
            _command = new(sql, _connection);

            var reader = await _command.ExecuteReaderAsync();

            double total = 250;
            double count = 0;

            while (await reader.ReadAsync())
            {
                var names = (string)reader["Name"];
                var capital = (string)reader["Capital"];
                var continents = (string)reader["Continents"];

                var country = new Country
                {
                    Name = new() { Common = names.Split(";")[0], Official = names.Split(";")[1] },
                    Capital = capital.Split(";").ToList(),
                    Region = (string)reader["Region"],
                    Subregion = (string)reader["Subregion"],
                    Gini = ConvertToDic((string)reader["Gini"]),
                    Languages = ConvertToDicStr((string)reader["Languages"]),
                    Population = (long)reader["Population"],
                    Continents = continents.Split(";").ToList(),
                    Flags = new() { Png = GetImg((byte[])reader["Flags"], names) }
                };

                countries.Add(country);
                count++;

                // Reporting status
                gettingStatus.Report(count / total * 100);
            }
        }
        catch (Exception e)
        {
            return new Response() { Status = false, Message = e.Message };
        }
        finally
        {
            await _connection.CloseAsync();
        }

        return new Response() { Status = true, Message = "Países carregados com sucesso", Result = countries };
    }

    /// <summary>
    /// Get Image from Bytes
    /// </summary>
    /// <param name="bytes">Bytes of image</param>
    /// <param name="path">Name to give image</param>
    /// <returns>Return a path of newly created image</returns>
    string GetImg(byte[] bytes, string path)
    {
        // format
        path = string.Join("", path.Split(" "));

        var newPath = @$"Img/{path}.png";
        newPath = Path.GetFullPath(newPath);

        try
        {
            File.WriteAllBytes(newPath, bytes);

            return newPath;
        }
        catch (Exception e)
        {
            //DialogService.ShowMessage("Erro ao tentar carregar imagem", $"Nao foi possivel carregar a bandeira do Pais: {e.Message}");

            return null;
        }
    }

    /// <summary>
    /// Split a ginven string to Dictionary
    /// </summary>
    /// <param name="s">A string to split and transform into dictionary</param>
    /// <returns>Returno dictionary with strign key and string value</returns>
    Dictionary<string, string> ConvertToDicStr(string s)
    {
        Dictionary<string, string> languages = new();
        var languagesStr = s.Split(";").ToList();

        foreach (var langStr in languagesStr)
        {
            var temp = langStr.Split("__");
            languages.Add(temp[0], temp[1]);
        }

        return languages;
    }

    /// <summary>
    /// Split a ginven string to Dictionary
    /// </summary>
    /// <param name="s">A string to split and transform into dictionary</param>
    /// <returns>Returno dictionary with strign key and double value</returns>
    Dictionary<string, double> ConvertToDic(string s)
    {
        Dictionary<string, double> gini = new();
        var ginis = s.Split(";").ToList();

        foreach (var strGini in ginis)
        {
            var temp = strGini.Split("__");
            gini.Add(temp[0], Convert.ToDouble(temp[1]));
        }

        return gini;
    }

    /// <summary>
    /// Delete all rows in Countries table
    /// </summary>
    public async Task<Response> DeleteDataAsync()
    {
        try
        {
            await _connection.OpenAsync();
            const string sql = "delete from Countries";
            _command = new(sql, _connection);
            await _command.ExecuteNonQueryAsync();
            return new Response() { Status = true, Message = "Dados apagados com sucesso" };
        }
        catch (Exception e)
        {
            //DialogService.ShowMessage("Erro", e.Message);
            return new Response() { Status = false, Message = e.Message };
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    /// <summary>
    /// Create a Countries table if it does not exist
    /// </summary>
    async Task<Response> CreateDbAsync()
    {
        try
        {
            await _connection.OpenAsync();
            const string sqlCommand = @"
                CREATE TABLE IF NOT EXISTS Countries (
                    Name TEXT, 
                    Capital TEXT, 
                    Region TEXT, 
                    Subregion TEXT, 
                    Gini TEXT, 
                    Languages TEXT, 
                    Population INTEGER, 
                    Continents TEXT, 
                    Flags BLOB
                )";
            _command = new(sqlCommand, _connection);
            await _command.ExecuteNonQueryAsync();
            return new Response() { Status = true, Message = "Tabela criada com sucesso" };
        }
        catch (Exception e)
        {
            //DialogService.ShowMessage("Erro", e.Message);
            return new Response() { Status = false, Message = e.Message };
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }
}