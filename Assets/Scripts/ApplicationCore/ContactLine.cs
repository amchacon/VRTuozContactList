using System;
using UnityEngine;
using UnityEngine.UI;

public class ContactLine : MonoBehaviour
{
    [System.Serializable]
    public struct Info
    {
        public string id;
        public string user;
        public string number;
        public Texture photo;
        public string email;
    }
    public Info info = new Info();

    [Header("Component References")]
    [SerializeField] RawImage photoImageComponent;
    [SerializeField] Text userTextComponent;
    [SerializeField] RectTransform rect;
    [SerializeField] Image selectedIconImageComponent;
    [SerializeField] Outline outlineComponent;

    void Awake()
    {
        rect.localPosition = Vector3.zero;
        rect.localScale = Vector3.one;
    }

    public void Initialize()
    {
        photoImageComponent.texture = info.photo;
        userTextComponent.text = info.user;
    }

    public void ToggleIconStatus()
    {
        outlineComponent.enabled = !outlineComponent.enabled;
        selectedIconImageComponent.enabled = !selectedIconImageComponent.enabled;
    }
}
