using System;
using UnityEngine;

public class Automatic : MonoBehaviour
{
    [SerializeField] bool autoPlay;
    [SerializeField] bool autoCashOut;
    [SerializeField] int round = -1;
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
            if(round>0)
            round--;
            if (round == 0) autoPlay = false;
        }
    }
    //quando começa a somar o multiplicador.
    public void NewMathStart()
    {

    }
    public void MatchMultiplier(float value)
    {
        Debug.Log("MatchMultiplier "+value);
        if (autoCashOut && value >= stopmultiplier)
        {
            ClientExemple.Instance.SendBet();
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
    }

    internal void AutoCashout(bool x)
    {
        autoCashOut = x;
    }
}
