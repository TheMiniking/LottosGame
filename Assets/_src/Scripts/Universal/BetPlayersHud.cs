using PrimeTween;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class BetPlayersHud : MonoBehaviour
{
    public TMP_Text name;
    public TMP_Text bet;
    public TMP_Text multply;
    public TMP_Text credits;
    public Animator anim;
    public float betVal, multiplierVal, creditsVal;
    public float velocity = 5f;
    public Tween tween;
    private void Update()
    {
        if (!GameManager.Instance.isWalking)
        {
            tween.Stop();
            Destroy(gameObject);
        }
    }

    internal void Clear()
    {
        name.text = string.Empty;
        bet.text = string.Empty;
        multply.text = "x -.--";
        credits.text = $"{GameManager.Instance.MoedaAtual()} -.--";
        creditsVal = 0;
        multiplierVal = 0;
        betVal = 0;
        if (anim != null) anim.Play("Default");
    }

    internal void Set(BetPlayers _bet, bool? rank = null)
    {
        name.text = _bet.name;
        betVal =(float) _bet.value;
        multiplierVal = _bet.multiplier;
        creditsVal =(float) _bet.value * _bet.multiplier;
        bet.text = $"{GameManager.Instance.MoedaAtual(betVal)}";
        if (rank == true)
        {
            multply.text = $"x {multiplierVal:0.00}";
            credits.text = $"{GameManager.Instance.MoedaAtual(creditsVal)}";
            if (anim != null) anim?.Play("BetRank");
            return;
        }
        if (_bet.multiplier >= 1)
        {
            multply.text = $"x {multiplierVal:0.00}";
            credits.text = $"{GameManager.Instance.MoedaAtual(creditsVal)}";
            if (anim != null) anim?.Play("BetWin");
        }
        else
        {
            multply.text = $"--";
            credits.text = $"--";
            if (anim != null) anim?.Play("Normal");
        }
    }

    internal void Reload()
    {
        if (name.text == string.Empty) return;
        bet.text = betVal!= 0? $"{GameManager.Instance.MoedaAtual(betVal)}" : string.Empty;
        multply.text = multiplierVal!= 0? $"x {multiplierVal:0.00}" : string.Empty;
        credits.text = creditsVal!= 0? $"{GameManager.Instance.MoedaAtual(creditsVal)}" : string.Empty;
    }

    public IEnumerator GoBack(RectTransform tank)
    {
        tween = Tween.UIAnchoredPositionX(tank, endValue: -Screen.width / 2, duration: 2.5f, ease: Ease.Linear)
            .OnComplete(() => { Destroy(tank.gameObject); });
        yield return new WaitForSeconds(3);
        Debug.Log("End GoBack");
    }
}