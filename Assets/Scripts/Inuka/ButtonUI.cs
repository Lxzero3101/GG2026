using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_Text text;

    private readonly Color normalColor = new Color32(0xF4, 0xD6, 0x4D, 0xFF); // #F4D64D
    private readonly Color hoverColor = new Color32(0x19, 0xFF, 0x14, 0xFF);  // #19FF14

    private void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
        text.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = normalColor;
    }
}
