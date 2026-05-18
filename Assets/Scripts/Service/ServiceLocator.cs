using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    public static ServiceLocator Instance { get; private set; }

    public ILoginService LoginService { get; private set; }
    public IVideoProvider VideoProvider { get; private set; }
    public IScoreRepository ScoreRepository { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 当前使用本地实现，后端就绪后替换为Api实现
        LoginService = new LocalLoginService();
        VideoProvider = new LocalVideoProvider();
        ScoreRepository = new LocalScoreRepository();
    }
}