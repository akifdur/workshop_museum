using ClosureSystem;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PresentationWhatWeDid : MonoBehaviour
{
    [SerializeField] private PresentationElementBase presentation;
    
    [SerializeField] private Image artifactsHolder;
    [SerializeField] private AspectRatioFitter artifactsFitter;
    [SerializeField] private Sprite[] artifacts;

    [Space]
    [SerializeField] private Image museumsHolder;
    [SerializeField] private Sprite[] museums;

    [Space] [SerializeField] private Animation curatorsSelection;
    
    [Header("Animation Settings")] 
    [SerializeField] private float fadeDuration = 0.1F;
    [SerializeField] private float restDuration = 2F;
    
    
    private Sequence _presentationSequence;
    
    private void Awake()
    {
        presentation.OnPresentationFocus += StartAnimation;
        presentation.OnPresentationDismiss += StopAnimation;
    }

    private void OnDestroy()
    {
        presentation.OnPresentationFocus -= StartAnimation;
        presentation.OnPresentationDismiss -= StopAnimation;
    }

    private void StartAnimation()
    {
        _presentationSequence?.Kill();
        _presentationSequence = DOTween.Sequence();
        
        var artifactAnimation = DOTween.Sequence().SetLoops(int.MaxValue, LoopType.Restart);
        foreach (var artifact in artifacts)
        {
            var t = Closure.Create(SetArtifact, artifact);
            artifactAnimation.AppendCallback(t.Invoke);
            artifactAnimation.Append(artifactsHolder.DOFade(1F, fadeDuration).SetEase(Ease.InQuad));
            artifactAnimation.AppendInterval(restDuration);
            artifactAnimation.Append(artifactsHolder.DOFade(0F, fadeDuration).SetEase(Ease.OutQuad));
        }

        var museumAnimation = DOTween.Sequence().SetLoops(int.MaxValue, LoopType.Restart);
        foreach (var museum in museums)
        {
            var t = Closure.Create(SetMuseum, museum);
            museumAnimation.AppendCallback(t.Invoke);
            museumAnimation.Append(museumsHolder.DOFade(1F, fadeDuration).SetEase(Ease.InQuad));
            museumAnimation.AppendInterval(restDuration);
            museumAnimation.Append(museumsHolder.DOFade(0F, fadeDuration).SetEase(Ease.OutQuad));
        }

        _presentationSequence.Join(artifactAnimation);
        _presentationSequence.Join(museumAnimation);

        curatorsSelection.gameObject.SetActive(true);
        curatorsSelection.Play();
    }

    private void StopAnimation()
    {
        _presentationSequence?.Kill();
        _presentationSequence = DOTween.Sequence();
        _presentationSequence.Join(museumsHolder.DOFade(0F, fadeDuration).SetEase(Ease.OutQuad));
        _presentationSequence.Join(artifactsHolder.DOFade(0F, fadeDuration).SetEase(Ease.OutQuad));
        
        
        curatorsSelection.gameObject.SetActive(false);
        curatorsSelection.Stop();
    }
    
    private void SetArtifact(Sprite sprite)
    {
        artifactsHolder.sprite = sprite;
        artifactsFitter.aspectRatio = sprite.rect.width / sprite.rect.height;
    }

    private void SetMuseum(Sprite sprite)
    {
        museumsHolder.sprite = sprite;
    }
}
