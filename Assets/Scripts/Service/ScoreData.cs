using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Assets/Scripts/Service/ScoreData.cs

/// <summary>单次训练成绩数据</summary>
[System.Serializable]
public class ScoreData
{
    public string username;           // 用户名
    public string scene;              // 训练场景（如 "Subway"）
    public string skill;              // 技能类型（"CPR" / "AED"）
    public float totalScore;          // 总分（0-100）
    public float compressionDepthAvg; // 平均按压深度（cm）
    public float compressionRateAvg;  // 平均按压频率（次/分）
    public int errorCount;            // 错误次数
    public string timestamp;          // 训练时间（字符串，格式 "yyyy-MM-dd HH:mm:ss"）

    // 预留扩展字段，当前阶段不强制使用
    public List<StepDetail> stepDetails; // 各步骤评分明细
}

/// <summary>训练步骤的评分明细</summary>
[System.Serializable]
public class StepDetail
{
    public string stepName;   // 步骤名称（如 "检查呼吸"）
    public float score;       // 该步骤得分
    public string comment;    // 评语
}