using System.Net.Http.Json;
using OnlineSurvey.Web.Models;

namespace OnlineSurvey.Web.Services;

public class SurveyApiService : ISurveyApiService
{
    private readonly HttpClient _httpClient;

    public SurveyApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<SurveyResponse>> GetActiveSurveysAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<IEnumerable<SurveyResponse>>("api/surveys/active");
        return response ?? [];
    }

    public async Task<PaginatedResponse<SurveyResponse>?> GetSurveysAsync(int page = 1, int pageSize = 50, SurveyStatus? status = null)
    {
        var url = $"api/surveys?page={page}&pageSize={pageSize}";
        if (status.HasValue)
        {
            url += $"&status={(int)status.Value}";
        }
        return await _httpClient.GetFromJsonAsync<PaginatedResponse<SurveyResponse>>(url);
    }

    public async Task<SurveyDetailResponse?> GetSurveyByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<SurveyDetailResponse>($"api/surveys/{id}");
    }

    public async Task<SurveyResultResponse?> GetSurveyResultsAsync(Guid surveyId)
    {
        return await _httpClient.GetFromJsonAsync<SurveyResultResponse>($"api/responses/surveys/{surveyId}/results");
    }

    public async Task<bool> SubmitResponseAsync(SubmitResponseRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/responses", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<SurveyDetailResponse?> CreateSurveyAsync(CreateSurveyRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/surveys", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SurveyDetailResponse>();
        }
        return null;
    }

    public async Task<SurveyDetailResponse?> ActivateSurveyAsync(Guid surveyId, ActivateSurveyRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/surveys/{surveyId}/activate", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SurveyDetailResponse>();
        }
        return null;
    }

    public async Task<bool> DeleteSurveyAsync(Guid surveyId)
    {
        var response = await _httpClient.DeleteAsync($"api/surveys/{surveyId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<SurveyDetailResponse?> UpdateSurveyAsync(Guid surveyId, UpdateSurveyRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/surveys/{surveyId}", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SurveyDetailResponse>();
        }
        return null;
    }

    public async Task<SurveyDetailResponse?> CloseSurveyAsync(Guid surveyId)
    {
        var response = await _httpClient.PostAsync($"api/surveys/{surveyId}/close", null);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SurveyDetailResponse>();
        }
        return null;
    }
}