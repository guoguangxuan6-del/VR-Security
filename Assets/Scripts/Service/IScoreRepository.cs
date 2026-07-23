using System.Collections.Generic;
using System.Threading.Tasks;

public interface IScoreRepository
{
    /// <summary>保存一条训练成绩</summary>
    Task SaveScoreAsync(ScoreData data);

    /// <summary>获取指定用户的所有成绩</summary>
    Task<List<ScoreData>> GetUserScoresAsync(string username);

    /// <summary>获取最新一次成绩，无记录时返回null</summary>
    Task<ScoreData> GetLatestScoreAsync();
}
