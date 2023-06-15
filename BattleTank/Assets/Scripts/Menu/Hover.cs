using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image buttonImage;
    private Color originalColor;
    private TextMeshPro textColor;
    private Color textHighlightColor ;
    void Start(){
        buttonImage = GetComponent<Image>();
        originalColor = buttonImage.color;
        textColor = GetComponentInChildren<TextMeshPro>();
        textHighlightColor = new Color(1f, 0.3f, 0.3f, 1f);
    }

    public void OnPointerEnter(PointerEventData eventData){
        Color highlightColor = originalColor;
        highlightColor.a = 1f;
        buttonImage.color = highlightColor;
        //textColor.color = textHighlightColor;
    }

    public void OnPointerExit(PointerEventData eventData){
        buttonImage.color = originalColor;
        //textColor.color = new Color(1f ,1f ,1f ,1f);
    }
}

