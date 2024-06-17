using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BigWin : MonoBehaviour
{
    [SerializeField] public float value;
    [SerializeField] TMP_Text winValor;
    [SerializeField] TMP_Text winText;
    [SerializeField] Animator anim;
    [SerializeField] int animationLayer = 0; // layer da anima��o que voc� deseja verificar

    private void OnEnable()
    {
        winValor.text = $"{GameManager.Instance.MoedaAtual(value)}";
        winText.text = value switch { < 50 => "Win",  < 1000 => "Big Win", _ => "Awsome Win"};
    }

    private void FixedUpdate()
    {
        if (anim.GetCurrentAnimatorStateInfo(animationLayer).normalizedTime >= 1)
        {
            // A anima��o terminou
            Debug.Log("A anima��o terminou");
            gameObject.SetActive(false);
        }
    }
}
