using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StudyVideoPanel : BasePanel
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoRenderImage;

    [Header("Controls")]
    [SerializeField] private Button playPauseButton;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button backButton;

    private bool isUpdatingSlider;
    private RenderTexture renderTexture;
    private bool isPreparing;

    void Start()
    {
        AutoBind();

        backButton.onClick.AddListener(OnBackClicked);
        playPauseButton.onClick.AddListener(OnPlayPauseClicked);
        progressSlider.onValueChanged.AddListener(OnProgressChanged);

        SetupVideoPlayer();
    }

    void AutoBind()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
        if (videoRenderImage == null)
            videoRenderImage = transform.Find("VideoRenderImage")?.GetComponent<RawImage>();
        if (playPauseButton == null)
            playPauseButton = transform.Find("VideoControlBar/PlayPauseButton")?.GetComponent<Button>();
        if (progressSlider == null)
            progressSlider = transform.Find("VideoControlBar/VideoProgressSlider")?.GetComponent<Slider>();
        if (backButton == null)
            backButton = transform.Find("VideoControlBar/VideoBackButton")?.GetComponent<Button>();

        Debug.Assert(videoPlayer != null, "[StudyVideo] VideoPlayer not found");
        Debug.Assert(videoRenderImage != null, "[StudyVideo] VideoRenderImage not found");
        Debug.Assert(playPauseButton != null, "[StudyVideo] PlayPauseButton not found");
        Debug.Assert(backButton != null, "[StudyVideo] BackButton not found");
    }

    void Update()
    {
        if (videoPlayer == null || !videoPlayer.isPrepared) return;

        if (videoPlayer.isPlaying && videoPlayer.length > 0)
        {
            float progress = (float)(videoPlayer.time / videoPlayer.length);
            isUpdatingSlider = true;
            progressSlider.SetValueWithoutNotify(progress);
            isUpdatingSlider = false;
        }
    }

    /// <summary>
    /// 配置 VideoPlayer：创建 RenderTexture、注册事件、异步从后端获取视频 URL
    /// </summary>
    void SetupVideoPlayer()
    {
        renderTexture = new RenderTexture(1920, 1080, 0);
        renderTexture.Create();

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;

        if (videoRenderImage != null)
            videoRenderImage.texture = renderTexture;

        // 异步加载视频 URL
        LoadVideoAsync("video1");
    }

    async void LoadVideoAsync(string videoId)
    {
        isPreparing = true;
        Debug.Log($"[StudyVideo] Loading video: {videoId}");

        string videoUrl = null;

        try
        {
            VideoInfo info = await ServiceLocator.Instance.VideoProvider.GetVideoAsync(videoId);
            if (info != null && !string.IsNullOrEmpty(info.url))
            {
                videoUrl = info.url;
                Debug.Log($"[StudyVideo] Got URL from API: {info.url} ({info.durationSeconds}s)");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[StudyVideo] Failed to get video from API: {ex.Message}");
        }

        // 如果后端 URL 不可用（假 URL 或无效），fallback 到本地文件
        if (string.IsNullOrEmpty(videoUrl) || videoUrl.Contains("example.com"))
        {
            string localPath = Application.streamingAssetsPath + "/Videos/cprdemo.mp4";
            if (System.IO.File.Exists(localPath))
            {
                videoUrl = localPath;
                Debug.Log($"[StudyVideo] Fallback to local: {localPath}");
            }
            else
            {
                Debug.LogWarning("[StudyVideo] No local video fallback available. Place cprdemo.mp4 in StreamingAssets/Videos/");
                isPreparing = false;
                return;
            }
        }

        videoPlayer.url = videoUrl;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.Prepare();
        isPreparing = false;
    }

    void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogWarning("[StudyVideo] Video error: " + message);
    }

    void OnPlayPauseClicked()
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }

    void OnProgressChanged(float value)
    {
        if (videoPlayer == null || !videoPlayer.isPrepared || isUpdatingSlider) return;
        videoPlayer.time = value * videoPlayer.length;
    }

    void OnBackClicked()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();
        UIManager.Instance.GoBack();
    }
}
