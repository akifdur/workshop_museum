using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PresentationVideoPlayer : MonoBehaviour
{
    [SerializeField] private PresentationElementBase presentation;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage rawImagePlayer;
    [SerializeField] private AspectRatioFitter aspectRatioFitter;

    private RenderTexture _renderTexture;

    private void OnEnable()
    {
        presentation.OnPresentationFocus -= PlayVideo;
        presentation.OnPresentationDismiss -= StopVideo;
        
        presentation.OnPresentationFocus += PlayVideo;
        presentation.OnPresentationDismiss += StopVideo;
    }

    private void OnDestroy()
    {
        presentation.OnPresentationFocus -= PlayVideo;
        presentation.OnPresentationDismiss -= StopVideo;
        
        if (_renderTexture != null) Destroy(_renderTexture);
    }
    
    private void PlayVideo()
    {
        if (!_renderTexture)
        {
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
            aspectRatioFitter.aspectRatio = (float)_renderTexture.width / _renderTexture.height;
        }
        
        videoPlayer.Play();
    }
    
    private void StopVideo()
    {
        videoPlayer.Stop();
    }
}
