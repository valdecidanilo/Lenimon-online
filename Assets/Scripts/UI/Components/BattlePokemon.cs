using UnityEngine;
using UnityEngine.UI;

public class BattlePokemon : MonoBehaviour
{
    public Image image;
    public Image overlay;

    private Vector2 imagePosition;
    private Vector3 imageRotation;
    private Vector2 imageScale;
    
    private Sprite overlaySprite;
    private Color overlayColor;
    private Vector2 overlayPosition;
    private Vector3 overlayRotation;
    private Vector2 overlayScale;

    private void Awake() => SaveAsDefaultValues();

    public void SaveAsDefaultValues()
    {
        RectTransform imageRect = (RectTransform)image.transform;
        imagePosition = imageRect.anchoredPosition;
        imageRotation = imageRect.localEulerAngles;
        imageScale = imageRect.localScale;

        overlaySprite = overlay.sprite;
        overlayColor = overlay.color;
        RectTransform overlayRect = (RectTransform)overlay.transform;
        overlayPosition = overlayRect.anchoredPosition;
        overlayRotation = overlayRect.localEulerAngles;
        overlayScale = overlayRect.localScale;
    }

    public void ResetBattlePokemon()
    {
        RectTransform imageRect = (RectTransform)image.transform;
        imageRect.anchoredPosition = imagePosition;
        imageRect.localEulerAngles = imageRotation;
        imageRect.localScale = imageScale;

        overlay.sprite = overlaySprite;
        overlay.color = overlayColor;
        RectTransform overlayRect = (RectTransform)overlay.transform;
        overlayRect.anchoredPosition = overlayPosition;
        overlayRect.localEulerAngles = overlayRotation;
        overlayRect.localScale = overlayScale;
    }
}
