using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TradutorAutomatic : MonoBehaviour
{
    [SerializeField] TMP_Text textToTranslate;
    [SerializeField,Tooltip("0 - English, 1 - Portuguese")] List<string> languages;

    private void OnEnable()
    {
        CanvasManager.Instance.OnTraductionChange += Translate;
        Translate(GameManager.Instance.traduction);
    }

    private void OnDisable()
    {
        CanvasManager.Instance.OnTraductionChange -= Translate;
    }

    private void OnValidate()
    {
        if (textToTranslate == null)
        {
            textToTranslate = GetComponent<TMP_Text>();
            
        }
        if (languages == null)
        {
            languages = new List<string>() { textToTranslate.text , textToTranslate.text };
        }
    }

    public void Translate(int trad)
    {
        textToTranslate.text = languages[trad];
    }
}
