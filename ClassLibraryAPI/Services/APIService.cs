using System.Net.Http.Json;
using System.Text.Json;
using ClassLibraryAPI.Models;

namespace ClassLibraryAPI.Services;

public class APIService
{
    readonly string _baseAddress;

    /// <summary>
    /// Iniciar a api com baseAdrress
    /// </summary>
    public APIService()
    {
        _baseAddress = "https://restcountries.com";
    }

    /// <summary>
    /// Busca os paises na API
    /// </summary>
    /// <param name="endpoint">O endpoint dos pais</param>
    /// <returns>A resposta que com o status e o resultado da API ou mensagem de erro</returns>
    public async Task<Response> GetCountriesAsync(string endpoint)
    {
        HttpClient client = new()
        {
            BaseAddress = new(_baseAddress)
        };

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,              // Formata o JSON com indentação
                PropertyNameCaseInsensitive = true // Ignora diferenciação de maiúsculas e minúsculas nos nomes das propriedades
            };
            var response = await client.GetFromJsonAsync<List<Country>>(endpoint, options);

            // var response1 = await client.GetAsync(endpoint);
            // var response2 = await response1.Content.ReadAsStreamAsync();
            // var response = await JsonSerializer.DeserializeAsync<List<Country>>(response2, options);

            if (response == null)
            {
                return new()
                {
                    Status = false,
                    Message = "Algo falhou, tenta de novo!"
                };
            }

            return new()
            {
                Status = true,
                Result = response
            };
        }
        catch (Exception error)
        {
            return new()
            {
                Status = false,
                Message = error.Message
            };
        }
    }
}