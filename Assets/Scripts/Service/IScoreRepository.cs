using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScoreRepository
{
    /// <summary>保存一条训练成绩</summary>
    void SaveScore(ScoreData data);
    
    /// <summary>获取指定用户的所有成绩</summary>
    List<ScoreData> GetUserScores(string username);
    
    /// <summary>获取最新一次成绩，无记录时返回null</summary>
    ScoreData GetLatestScore();
}