using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LastSlot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI multply;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Image Image;
    [SerializeField] List<Sprite> sprites;

    public void SetBet(float valor)
    {
        multply.text = $"x {valor:0.00}";
        canvasGroup.alpha = 1;
        Image.sprite = (valor < 1.5f) ? sprites[0] : ((valor < 2f) ? sprites[3] : ((valor < 5) ? sprites[1] : sprites[2]));

    }
}
