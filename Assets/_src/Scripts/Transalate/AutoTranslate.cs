using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AutoTranslate : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI value;
    [SerializeField] Text value2;
    [SerializeField] string key;

    private void Awake()
    {
        value = GetComponent<TextMeshProUGUI>();
        value2 = GetComponent<Text>();
    }
    private void Start()
    {
        if (value != null)
            value.text = LanguageManager.instance?.TryTranslate(key, value.text);
        if (value2 != null)
            value2.text = LanguageManager.instance?.TryTranslate(key, value2.text);
        StartCoroutine(Rebuild());
    }
    private void Reset()
    {
        value = GetComponent<TextMeshProUGUI>();
        value2 = GetComponent<Text>();
    }
    internal void ForceTranslate()
    {
        if (value != null)
            value.text = LanguageManager.instance.TryTranslate(key, value.text);
        else if (value2 != null)
            value2.text = LanguageManager.instance.TryTranslate(key, value2.text);
        StartCoroutine(Rebuild());
    }
    private void OnEnable()
    {
        if (LanguageManager.instance != null) LanguageManager.instance.RegisterComponent(this);

    }
    IEnumerator Rebuild()
    {
        var r = TryGetComponent<ContentSizeFitter>(out ContentSizeFitter csf);
        Debug.Log(name + " Rebuild " + r);
        if (r)
        {
            yield return new WaitForEndOfFrame();
            csf.enabled = false;
            yield return new WaitForEndOfFrame();
            csf.enabled = true;
        }
    }
    private void OnDisable()
    {
        LanguageManager.instance?.UnregisterComponent(this);
    }
}