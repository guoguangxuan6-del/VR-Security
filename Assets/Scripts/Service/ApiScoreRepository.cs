using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ApiScoreRepository : IScoreRepository
{
    private const string BaseUrl = "http://123.57.30.132:8080";

    private static readonly HttpClient http = new HttpClient();

    public async Task SaveScoreAsync(ScoreData data)
    {
        string jsonBody = JsonUtility.ToJson(new ScoreSubmitRequest
        {
            scene = data.scene,
            skill = data.skill,
            totalScore = data.totalScore,
            compressionDepthAvg = data.compressionDepthAvg,
            compressionRateAvg = data.compressionRateAvg,
            errorCount = data.errorCount,
            stepDetails = data.stepDetailsJson ?? ""
        });

        string token = PlayerPrefs.GetString("auth_token", "");
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        var req = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/api/v1/scores")
        {
            Content = content
        };
        req.Headers.Add("Accept", "application/json");
        if (!string.IsNullOrEmpty(token))
            req.Headers.Add("Authorization", "Bearer " + token);

        var resp = await http.SendAsync(req);
        string json = await resp.Content.ReadAsStringAsync();
        if (resp.IsSuccessStatusCode)
        {
            Debug.Log($"[ApiScoreRepository] Score saved: {json}");
        }
        else
        {
            Debug.LogWarning($"[ApiScoreRepository] Failed to save score: {json}");
        }
    }

    public async Task<List<ScoreData>> GetUserScoresAsync(string username)
    {
        string url = $"{BaseUrl}/api/v1/scores";
        string token = PlayerPrefs.GetString("auth_token", "");

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("Accept", "application/json");
        if (!string.IsNullOrEmpty(token))
            req.Headers.Add("Authorization", "Bearer " + token);

        var resp = await http.SendAsync(req);
        string json = await resp.Content.ReadAsStringAsync();

        if (resp.IsSuccessStatusCode)
        {
            var envelope = JsonUtility.FromJson<ScoreListResponse>(json);
            if (envelope != null && envelope.code == 200 && envelope.data != null)
            {
                return ConvertScoreList(envelope.data);
            }
        }
        Debug.LogWarning($"[ApiScoreRepository] Failed to get scores: {json}");
        return new List<ScoreData>();
    }

    public async Task<ScoreData> GetLatestScoreAsync()
    {
        string url = $"{BaseUrl}/api/v1/scores/latest";
        string token = PlayerPrefs.GetString("auth_token", "");

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("Accept", "application/json");
        if (!string.IsNullOrEmpty(token))
            req.Headers.Add("Authorization", "Bearer " + token);

        var resp = await http.SendAsync(req);
        string json = await resp.Content.ReadAsStringAsync();

        if (resp.IsSuccessStatusCode)
        {
            var envelope = JsonUtility.FromJson<ScoreSingleResponse>(json);
            if (envelope != null && envelope.code == 200 && envelope.data != null)
            {
                return ConvertScoreDto(envelope.data);
            }
        }
        Debug.LogWarning($"[ApiScoreRepository] Failed to get latest score: {json}");
        return null;
    }

    private static List<ScoreData> ConvertScoreList(List<ScoreDto> dtos)
    {
        var list = new List<ScoreData>();
        if (dtos == null) return list;
        foreach (var dto in dtos)
        {
            list.Add(ConvertScoreDto(dto));
        }
        return list;
    }

    private static ScoreData ConvertScoreDto(ScoreDto dto)
    {
        return new ScoreData
        {
            username = dto.username,
            scene = dto.scene,
            skill = dto.skill,
            totalScore = dto.totalScore,
            compressionDepthAvg = dto.compressionDepthAvg,
            compressionRateAvg = dto.compressionRateAvg,
            errorCount = dto.errorCount,
            timestamp = dto.createdAt,
            stepDetailsJson = dto.stepDetails
        };
    }

    [Serializable]
    private class ScoreListResponse
    {
        public int code;
        public string message;
        public List<ScoreDto> data;
    }

    [Serializable]
    private class ScoreSingleResponse
    {
        public int code;
        public string message;
        public ScoreDto data;
    }
}
