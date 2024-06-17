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
    [SerializeField] int animationLayer = 0; // layer da animação que você deseja verificar

    private void OnEnable()
    {
        winValor.text = $"{GameManager.Instance.MoedaAtual(value)}";
        winText.text = value switch { < 50 => "Win",  < 1000 => "Big Win", _ => "Awsome Win"};
    }

    private void FixedUpdate()
    {
        if (anim.GetCurrentAnimatorStateInfo(animationLayer).normalizedTime >= 1)
        {
            // A animação terminou
            Debug.Log("A animação terminou");
            gameObject.SetActive(false);
        }
    }
}
