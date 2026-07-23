using System.Threading.Tasks;
using UnityEngine;

public interface IAvatarService
{
    /// <summary>
    /// 上传头像文件到后端，返回完整可访问的 URL
    /// </summary>
    /// <param name="filePath">本地图片路径（jpg/png/webp）</param>
    /// <returns>头像完整 URL，失败返回 null</returns>
    Task<string> UploadAvatarAsync(string filePath);

    /// <summary>
    /// 通过 URL 加载头像纹理，赋值到 Sprite
    /// </summary>
    /// <param name="avatarUrl">头像 URL</param>
    /// <returns>加载好的 Texture2D，失败返回 null</returns>
    Task<Texture2D> LoadAvatarTextureAsync(string avatarUrl);
}
