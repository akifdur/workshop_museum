using UnityEngine;

public class PresentationElementsSource : MonoBehaviour
{
    [SerializeField] private PresentationElementBase[] elements;
    
    public int ElementsCount => elements.Length;
    public PresentationElementBase GetElement(int index) => elements[index];

#if UNITY_EDITOR
    [ContextMenu("Select Presentation Elements")]
    private void Test()
    {
        elements = GetComponentsInChildren<PresentationElementBase>(false);
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}