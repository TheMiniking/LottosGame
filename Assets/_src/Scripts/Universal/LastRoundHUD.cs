using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LastRoundHUD : MonoBehaviour
{
    [SerializeField] CanvasGroup round;
    [SerializeField] List<Image> tanks;
    [SerializeField] List<CanvasGroup> winner;
    [SerializeField] List<TMP_Text> bets;

    private void Start()
    {
        Clear();
    }

    internal void Clear()
    {
        bets.ForEach(x => x.text = "x --,--");
        winner.ForEach(x => x.alpha = 0);
        round.alpha = 0;
    }

    internal void Set(LastMultiTriple _bet)
    {
        int best = 0;
        for (int i = 0; i < _bet.multis.Length; i++)
        {
            bets[i].text = $"x {_bet.multis[i]:0.00}";
            best = _bet.multis[i] > _bet.multis[best] ? i : best;
        }
        winner.ForEach(x => x.alpha = winner.IndexOf(x) == best ? 1 : 0);
        round.alpha = 1;
    }

}
