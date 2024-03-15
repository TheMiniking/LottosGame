using TMPro;
using UnityEngine;

public class WinExtra : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    void Update()
    {
        if (text.color.a == 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string _text)
    {
        text.text = _text;
    }
}
