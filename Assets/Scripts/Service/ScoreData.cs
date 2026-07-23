using System.Collections.Generic;

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
    public List<StepDetail> stepDetails; // 各步骤评分明细（本地使用）

    /// <summary>
    /// stepDetails 的 JSON 字符串形式，与后端 API 的 stepDetails 字段对齐。
    /// 提交成绩时优先使用此字段；若为空则序列化 stepDetails 列表。
    /// </summary>
    public string stepDetailsJson;
}

/// <summary>训练步骤的评分明细</summary>
[System.Serializable]
public class StepDetail
{
    public string stepName;   // 步骤名称（如 "检查呼吸"）
    public float score;       // 该步骤得分
    public string comment;    // 评语
}
