using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVideoProvider
{
    /// <summary>获取教学视频的本地路径或远程URL</summary>
    string GetVideoPath(string videoId);
    
    /// <summary>是否拥有指定视频资源</summary>
    bool HasVideo(string videoId);
}