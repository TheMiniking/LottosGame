using System;
using TMPro;
using UnityEngine;

public class BetPlayersHud : MonoBehaviour
{
    public TMP_Text name;
    public TMP_Text bet;
    public TMP_Text multply;
    public TMP_Text credits;
    public Animator anim;

    internal void Clear()
    {
        name.text = string.Empty;
        bet.text = string.Empty;
        multply.text = string.Empty;
        credits.text = string.Empty;
        anim.Play("Default");
    }

    internal void Set(BetPlayers _bet, bool? rank = null)
    {
        name.text = _bet.name;
        bet.text = $"{_bet.value:0.00}";
        if (rank == true)
        {
            multply.text = $"x {_bet.multiplier:0.00}";
            credits.text = $"{CanvasManager.Instance.traduction switch { 0 => "$", 1 => "R$" }}{_bet.value * _bet.multiplier:#,0.00}";
            anim.Play("BetRank");
            return;
        }
        if (_bet.multiplier >= 1)
        {
            multply.text = $"x {_bet.multiplier:0.00}";
            credits.text = $"{CanvasManager.Instance.traduction switch { 0 => "$", 1 => "R$" }}{_bet.value * _bet.multiplier:#,0.00}";
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