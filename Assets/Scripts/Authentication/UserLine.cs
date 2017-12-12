using UnityEngine;
using UnityEngine.UI;

public class UserLine : MonoBehaviour
{
    public Text buttonText;
    public Button buttonComponent;
    public RectTransform rect;

    void Awake()
    {
        rect.localPosition = Vector3.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }
}