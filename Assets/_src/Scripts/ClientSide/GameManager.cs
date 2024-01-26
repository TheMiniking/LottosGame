using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] WebClient webClient;
    [SerializeField] GameScreen gameS;
    [SerializeField] public bool isWalking = false;
    [SerializeField] public bool canBet = true;
    [SerializeField] public float credits;
    [SerializeField] float stopValBet = 1.01f;
    [SerializeField] float valueToBet = 1f;

    [SerializeField] Toggle autoStop;
    private void Awake()
    {
        Instance = this;
    }

    public float UpDownAutoStop(float modify, bool toUp)
    {
        stopValBet = toUp ? stopValBet + modify : stopValBet - modify;
        stopValBet = stopValBet < 1.01f ? 1.01f : stopValBet;
        return stopValBet;
    }// Controle de valores, para onde parar automaticamente

    public float UpDownBetAmount(float modify, bool toUp, int step)
    {
        if (toUp) valueToBet = step == 1 ? valueToBet + modify : step == 2 ? valueToBet * 2 : credits;
        else valueToBet = step == 1 ? valueToBet - modify : step == 2 ? valueToBet / 2 : 1f;
        valueToBet = valueToBet < 1 ? 1f : valueToBet > credits ? credits : valueToBet;
        return valueToBet;
    }//Controle de Valores da aposta

    public void SendBet() => webClient.SendBet(valueToBet,autoStop.isOn?stopValBet:0);

}
