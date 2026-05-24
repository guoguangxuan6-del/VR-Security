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

    // void SetupVideoPlayer()
    // {
    //     renderTexture = new RenderTexture(1920, 1080, 0);
    //     renderTexture.Create();

    //     videoPlayer.renderMode = VideoRenderMode.RenderTexture;
    //     videoPlayer.targetTexture = renderTexture;
    //     videoPlayer.isLooping = false;
    //     videoPlayer.playOnAwake = false;

    //     if (videoRenderImage != null)
    //         videoRenderImage.texture = renderTexture;

    //     string videoPath = ServiceLocator.Instance.VideoProvider.GetVideoPath("cpr_demo");
    //     if (!string.IsNullOrEmpty(videoPath))
    //     {
    //         videoPlayer.url = videoPath;
    //         videoPlayer.Prepare();
    //     }
    //     else
    //     {
    //         Debug.LogWarning("[StudyVideo] No test video found");
    //     }
    // }

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

        string videoPath = Application.streamingAssetsPath + "/Videos/cprdemo.mp4";

        if (!string.IsNullOrEmpty(videoPath))
        {
            videoPlayer.url = videoPath;
            videoPlayer.errorReceived += OnVideoError;
            videoPlayer.Prepare();
            Debug.Log("[StudyVideo] Loading: " + videoPath);
        }
        else
        {
            Debug.LogWarning("[StudyVideo] No video found in StreamingAssets/Videos/");
        }
    }

    void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogWarning("[StudyVideo] Format unsupported. Transcode to H.264 baseline MP4. Error: " + message);
    }
    void OnPlayPauseClicked()
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }

    // void OnProgressChanged(float value)
    // {
    //     if (videoPlayer == null || videoPlayer.clip == null || !isDraggingSlider) return;
    //     videoPlayer.time = value * videoPlayer.length;
    // }
    void OnProgressChanged(float value)
    {
        if (videoPlayer == null || !videoPlayer.isPrepared || isUpdatingSlider) return;
        videoPlayer.time = value * videoPlayer.length;
    }

    void OnBackClicked()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();
        OnBack();
    }
}