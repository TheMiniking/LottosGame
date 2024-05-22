using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LastRoundSingle : MonoBehaviour
{
    [SerializeField] TMP_Text multiplierText;
    [SerializeField] List<Sprite> tanks;
    [SerializeField] List<Color> colors;
    [SerializeField] List<Sprite> fundo;
    [SerializeField] Image tankWinner;
    [SerializeField] CanvasGroup controlGroup;

    private void Start()
    {
        Clear();
    }

    internal void Clear()
    {
        multiplierText.text = $"x --,--";
        controlGroup.alpha = 0;
    }

    internal void Set(LastMultiTriple _multiplier)
    {
        controlGroup.alpha = 1;
        int best = 0;
        for (int i = 0; i < _multiplier.multis.Length; i++)
        {
            best = _multiplier.multis[i] > _multiplier.multis[best] ? i : best;
        }
        multiplierText.text = $"x {_multiplier.multis[best]:0.00}";
        tankWinner.sprite = tanks[best];
        tankWinner.color = colors[best];
        GetComponent<Image>().sprite = _multiplier.multis[best] < 1.5f ? fundo[0] : _multiplier.multis[best] < 2 ? fundo[1] : _multiplier.multis[best] < 4 ? fundo[2]: fundo[3];
    }

}
