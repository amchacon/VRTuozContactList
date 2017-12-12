using UnityEngine;
using UnityEngine.UI;

public class SelectedContact : MonoBehaviour
{
    public string id;
    public RawImage photo;
    public Text userText;
    public RectTransform rect;
    
    void Awake()
    {
        rect.localPosition = Vector3.zero;
        rect.localScale = Vector3.one;
    }
}
