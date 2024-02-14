using System;
using TMPro;
using UnityEngine;

public class BetPlayersHud :MonoBehaviour
{
    public TMP_Text name;
    public TMP_Text bet;
    public TMP_Text multply;
    public TMP_Text credits;
    public Animator anim;

    internal void Clear()
    {
        name.text = "";
        bet.text = "";
        multply.text = "";
        credits.text = "";
        anim.Play("Default");
    }

    internal void Set(BetPlayers _bet)
    {
        name.text = _bet.name;
        bet.text = $"{_bet.value:0.00}";
        if(_bet.multiplier >= 1)
        {
            multply.text = $"x {_bet.multiplier:0.00}";
            credits.text = $"{_bet.value * _bet.multiplier:0.00}";
            anim.Play("BetWin");
        }
        else
        {
            multply.text = $"--";
            credits.text = $"--";
            anim.Play("Normal");
        }
    }
}