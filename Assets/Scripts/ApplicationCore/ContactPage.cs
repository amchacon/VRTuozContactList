using UnityEngine;

public class ContactPage : MonoBehaviour
{
    public RectTransform rect;

    void Start()
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.localPosition = Vector3.zero;
        rect.localScale = Vector3.one;
    }
}
