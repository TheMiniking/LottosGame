using System;
using UnityEngine;

public class Automatic : MonoBehaviour
{
    [SerializeField] bool autoPlay;
    [SerializeField] bool autoCashOut;
    [SerializeField] public int round = -1;
    [SerializeField] float stopmultiplier;
    
    void Start()
    {

    }


    void Update()
    {

    }
    //quando inicia o tempo pra entrar na partida
    public void NewMatchInit()
    {
        Debug.Log(autoPlay+" NewMatchInit " + round);
        if (autoPlay && (round == -1 || round > 0))
        {
            ClientExemple.Instance.SendBet();
            CanvasManager.Instance.PlayMensagen("AutoPlay Bet");
            if (round>0)
            round--;
            if (round == 0)
            {
                autoPlay = false;
                CanvasManager.Instance.PlayMensagen("End of AutoPlay");
            }

        }
    }
    //quando come�a a somar o multiplicador.
    public void NewMathStart()
    {

    }
    public void MatchMultiplier(float value)
    {
        Debug.Log("MatchMultiplier "+value);
        if (autoCashOut && value >= stopmultiplier)
        {
            ClientExemple.Instance.SendBet();
            CanvasManager.Instance.PlayMensagen($"CashOut x{value:0.00}");
        }
    }

    internal void EndMatchStart()
    {

    }

    internal void SetAutoStop(float v)
    {
        stopmultiplier = v;
    }

    internal void AutoStop(bool x)
    {
        autoPlay = x;
        CanvasManager.Instance.PlayMensagen(x ? "AutoPlay Active" : "AutoPlay Desactive");
    }

    internal void AutoCashout(bool x)
    {
        autoCashOut = x;
        CanvasManager.Instance.PlayMensagen(x ? "CashOut Active" : "CashOut Desactive");
    }


}
