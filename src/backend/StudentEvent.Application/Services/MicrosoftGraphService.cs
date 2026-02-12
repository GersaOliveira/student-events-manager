using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StudentEvent.Application.Interfaces;
using StudentEvent.Domain.Entities;
using System.Text.Json.Serialization;

namespace StudentEvent.Application.Services;

public class MicrosoftGraphService : IMicrosoftGraphService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    // Cache simples em memória para o token
    private static string _cachedToken = string.Empty;
    private static DateTime _tokenExpiration = DateTime.MinValue;

    public MicrosoftGraphService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    private async Task<string> GetAccessTokenAsync()
    {
        // Verifica se o token ainda é válido (com margem de 5 minutos)
        if (!string.IsNullOrEmpty(_cachedToken) && _tokenExpiration > DateTime.UtcNow.AddMinutes(5))
        {
            return _cachedToken;
        }

        var tenantId = _config["MicrosoftGraph:TenantId"];
        var clientId = _config["MicrosoftGraph:ClientId"];
        var clientSecret = _config["MicrosoftGraph:Secret"];

        var url = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

        var values = new Dictionary<string, string>
    {
        { "client_id", clientId! },
        { "client_secret", clientSecret! },
        { "scope", "https://graph.microsoft.com/.default" },
        { "grant_type", "client_credentials" }
    };

        var content = new FormUrlEncodedContent(values);
        var response = await _httpClient.PostAsync(url, content);

        // Se der erro, vamos ler o que a Microsoft escreveu antes de estourar a exceção
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Erro na Microsoft: {response.StatusCode} - {errorContent}");
        }

        var json = await response.Content.ReadFromJsonAsync<TokenResponse>(_jsonOptions);
        
        if (json != null)
        {
            _cachedToken = json.AccessToken;
            _tokenExpiration = DateTime.UtcNow.AddSeconds(json.ExpiresIn);
        }

        return _cachedToken ?? string.Empty;
    }

    public async Task<IEnumerable<Student>> GetExternalStudentsAsync()
    {
        var token = await GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var allStudents = new List<Student>();
        string? nextUrl = "https://graph.microsoft.com/v1.0/users?$select=id,displayName,mail";

        while (!string.IsNullOrEmpty(nextUrl))
        {
            var response = await _httpClient.GetAsync(nextUrl);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<GraphUserResponse>(_jsonOptions);
            
            if (data?.Value != null)
            {
                allStudents.AddRange(data.Value.Select(u => new Student
                {
                    Id = Guid.NewGuid(),
                    MicrosoftId = u.Id,
                    Name = u.DisplayName,
                    Email = u.Mail ?? u.UserPrincipalName
                }));
            }

            nextUrl = data?.NextLink;
        }

        return allStudents;
    }

    public async Task<IEnumerable<Event>> GetExternalEventsAsync(string microsoftId)
    {
        var token = await GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        // Garante que as datas venham em UTC para evitar problemas de fuso horário
        if (!_httpClient.DefaultRequestHeaders.Contains("Prefer")) _httpClient.DefaultRequestHeaders.Add("Prefer", "outlook.timezone=\"UTC\"");

        var allEvents = new List<Event>();
        string? nextUrl = $"https://graph.microsoft.com/v1.0/users/{microsoftId}/events?$select=id,subject,start,end";

        while (!string.IsNullOrEmpty(nextUrl))
        {
            var response = await _httpClient.GetAsync(nextUrl);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Graph Error] Falha ao buscar eventos de {microsoftId}: {response.StatusCode} - {error}");
                break; // Sai do loop em caso de erro
            }

            var data = await response.Content.ReadFromJsonAsync<GraphEventResponse>(_jsonOptions);

            if (data?.Value != null)
            {
                allEvents.AddRange(data.Value.Select(e => new Event
                {
                    Id = Guid.NewGuid(),
                    MicrosoftEventId = e.Id,
                    Subject = e.Subject ?? "Sem Assunto",
                    StartTime = e.Start?.DateTime != null ? DateTime.Parse(e.Start.DateTime) : DateTime.MinValue,
                    EndTime = e.End?.DateTime != null ? DateTime.Parse(e.End.DateTime) : DateTime.MinValue
                }));
            }

            nextUrl = data?.NextLink;
        }

        return allEvents;
    }

    // --- Classes de Mapeamento do Graph (Modelos Internos) ---
    private record GraphUserResponse(List<GraphUser> Value, [property: JsonPropertyName("@odata.nextLink")] string? NextLink);
    private record GraphUser(string Id, string? DisplayName, string? Mail, string? UserPrincipalName);

    private record GraphEventResponse(List<GraphEvent> Value, [property: JsonPropertyName("@odata.nextLink")] string? NextLink);
    private record GraphEvent(string Id, string? Subject, GraphDateTime? Start, GraphDateTime? End);
    private record GraphDateTime(string? DateTime, string? TimeZone);

    // Classe interna auxiliar para o Token
    private class TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}