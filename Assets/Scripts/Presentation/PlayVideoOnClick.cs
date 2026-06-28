using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideoOnClick : MonoBehaviour
{
    [SerializeField] private PresentationElementBase presentation;
    [SerializeField] private Button playButton;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage rawImagePlayer;
    [SerializeField] private AspectRatioFitter aspectRatioFitter;
    
    private RenderTexture _renderTexture;
    private bool _isPlaying;

    private void OnEnable()
    {
        if (presentation != null)
            presentation.OnPresentationDismiss += StopVideo;
        
        playButton.onClick.AddListener(OnButtonClicked);

        Initialize();
    }

    private void OnDisable()
    {
        if (presentation != null)
            presentation.OnPresentationDismiss -= StopVideo;
        
        playButton.onClick.RemoveListener(OnButtonClicked);

        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
    }

    private void Initialize()
    {
        if (_renderTexture != null)
            return;
        
        var clip = videoPlayer.clip;
        var descriptor = new RenderTextureDescriptor((int)clip.width, (int)clip.height)
        {
            autoGenerateMips = false,
            useMipMap = false,
            depthBufferBits = 0,
        };

        _renderTexture = new RenderTexture(descriptor);
        videoPlayer.targetTexture = _renderTexture;
        rawImagePlayer.texture = _renderTexture;
        aspectRatioFitter.aspectRatio = (float)descriptor.width / descriptor.height;
    }
    
    private void OnButtonClicked()
    {
        if (_isPlaying)
            videoPlayer.Stop();
        else
            videoPlayer.Play();

        _isPlaying = !_isPlaying;
    }
    
    private void StopVideo()
    {
        _isPlaying = false;
        videoPlayer.Stop();
        videoPlayer.frame = 0;
    }
}
