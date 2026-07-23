using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    public static ServiceLocator Instance { get; private set; }

    public ILoginService LoginService { get; private set; }
    public IVideoProvider VideoProvider { get; private set; }
    public IScoreRepository ScoreRepository { get; private set; }
    public IAvatarService AvatarService { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 后端 API 实现（接口已切换为 async/await）
        LoginService = new ApiLoginService();
        VideoProvider = new ApiVideoProvider();
        ScoreRepository = new ApiScoreRepository();
        AvatarService = new ApiAvatarService();
    }
}
