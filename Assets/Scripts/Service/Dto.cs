using System;
using System.Collections.Generic;

// 本文件定义与后端 API 对应的数据传输对象（DTO）
// 字段命名与后端 JSON 完全一致，用于 JsonUtility.FromJson 反序列化

/// <summary>
/// 登录/注册响应 data 字段
/// </summary>
[Serializable]
public class AuthData
{
    public string token;
    public long expiresAt;
}

/// <summary>
/// 训练场景
/// </summary>
[Serializable]
public class SceneDto
{
    public int id;
    public string name;
    public string description;
    public string type;    // "basic" | "advanced"
    public string icon;    // 前端图标标识
    public int sortOrder;
    public string createdAt;
}

/// <summary>
/// 场景列表响应（包装数组）
/// </summary>
[Serializable]
public class SceneListWrapper
{
    public List<SceneDto> data;
}

/// <summary>
/// 知识条目
/// </summary>
[Serializable]
public class KnowledgeDto
{
    public int id;
    public string title;
    public string content;
    public string category;
    public string tags;
    public string createdAt;
}

/// <summary>
/// 视频信息响应 data 字段
/// </summary>
[Serializable]
public class VideoInfo
{
    public string videoId;
    public string url;
    public int durationSeconds;
}

/// <summary>
/// 成绩 DTO（与后端 ScoreDto 对齐）
/// </summary>
[Serializable]
public class ScoreDto
{
    public int id;
    public string username;
    public string scene;
    public string skill;
    public float totalScore;
    public float compressionDepthAvg;
    public float compressionRateAvg;
    public int errorCount;
    public string stepDetails;   // JSON 字符串，前端按需解析
    public string createdAt;
}

/// <summary>
/// 成绩提交请求 body
/// </summary>
[Serializable]
public class ScoreSubmitRequest
{
    public string scene;
    public string skill;
    public float totalScore;
    public float compressionDepthAvg;
    public float compressionRateAvg;
    public int errorCount;
    public string stepDetails;   // JSON 字符串
}

/// <summary>
/// 当前用户信息
/// </summary>
[Serializable]
public class UserInfo
{
    public int id;
    public string username;
    public string createdAt;
    public string avatarUrl;    // 头像 URL（可能为 null 或空）
}

/// <summary>
/// QA 预设问题响应 data 字段
/// </summary>
[Serializable]
public class QaPresetsData
{
    public List<string> presets;
}

/// <summary>
/// QA 提问请求 body
/// </summary>
[Serializable]
public class QaRequest
{
    public string question;
    public List<QaHistoryItem> history;
}

/// <summary>
/// QA 历史对话项
/// </summary>
[Serializable]
public class QaHistoryItem
{
    public string role;     // "user" | "assistant"
    public string content;
}

/// <summary>
/// QA 回答响应 data 字段
/// </summary>
[Serializable]
public class QaAnswerData
{
    public string answer;
}

/// <summary>头像上传响应</summary>
[Serializable]
public class AvatarUploadData
{
    public string avatar_url;
}

/// <summary>学员 DTO</summary>
[Serializable]
public class StudentDto
{
    public int id;
    public string name;
    public string phone;
    public string email;
    public string groupName;
    public string certStatus;  // "certified" | "training" | "expired"
    public string trainedAt;
    public string createdAt;
}
