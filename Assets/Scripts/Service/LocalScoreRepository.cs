using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class LocalScoreRepository : IScoreRepository
{
    private string dirPath;
    private string filePath;
    private List<ScoreData> allScores;

    public LocalScoreRepository()
    {
        dirPath = Path.Combine(Application.persistentDataPath, "Scores");
        filePath = Path.Combine(dirPath, "scores.json");
        Directory.CreateDirectory(dirPath);
        LoadScores();
    }

    public Task SaveScoreAsync(ScoreData data)
    {
        allScores.Add(data);
        SaveScores();
        return Task.CompletedTask;
    }

    public Task<List<ScoreData>> GetUserScoresAsync(string username)
    {
        return Task.FromResult(allScores.Where(s => s.username == username).ToList());
    }

    public Task<ScoreData> GetLatestScoreAsync()
    {
        return Task.FromResult(allScores.LastOrDefault());
    }

    private void LoadScores()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            var wrapper = JsonUtility.FromJson<ScoreListWrapper>(json);
            allScores = wrapper?.scores ?? new List<ScoreData>();
        }
        else
        {
            allScores = new List<ScoreData>();
        }
    }

    private void SaveScores()
    {
        var wrapper = new ScoreListWrapper { scores = allScores };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(filePath, json);
    }

    [System.Serializable]
    private class ScoreListWrapper
    {
        public List<ScoreData> scores;
    }
}
