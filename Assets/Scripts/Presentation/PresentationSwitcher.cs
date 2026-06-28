using UnityEngine;
using UnityEngine.UI;

public class PresentationSwitcher : MonoBehaviour
{
    [SerializeField] private PresentationElementBase presentation;
    [SerializeField] private Image target;
    [SerializeField] private Sprite image;
    
    private void Awake()
    {
        presentation.OnPresentationFocus += SwitchImage;
    }

    private void OnDestroy()
    {
        presentation.OnPresentationFocus -= SwitchImage;
    }

    private void SwitchImage()
    {
        target.sprite = image;   
    }
}