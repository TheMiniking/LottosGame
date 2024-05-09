using TMPro;
using UnityEngine;

public class WinExtra : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] CanvasGroup canvasGroup;

    void Update()
    {
        if (canvasGroup.alpha == 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string _text)
    {
        text.text = _text;
    }
}
