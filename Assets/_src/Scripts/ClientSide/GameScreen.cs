﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class GameScreen : BaseScreen
{
    [SerializeField] WebClient webClient;
    [SerializeField] GameManager gameManager;
    [SerializeField] Player tank;
    [SerializeField] Material fundo;
    [SerializeField] float fundoRealtimeVelocity;
    [SerializeField] float fundoRealtimeAtualPosition;
    [SerializeField] bool fundoOnMove = false;

    [SerializeField] float bet;
    [SerializeField] float stop;

    [SerializeField] TMP_Text txtWalletBalance, txtWalletNickname;
    [SerializeField] TMP_Text txtTimerMult , txtTimerMensagem, txtBonusTotal;
    [SerializeField] Button stopAnBet;
    [SerializeField] TMP_Text txtStopAnBet, txtStopVal, txtBetVal;
    [SerializeField] List<Button> betButtons ,autoStop = new() ;

    private void Start()
    {
        fundo.SetInt("_UseScriptTime", 1);
        betButtons.ForEach(x => x.onClick.RemoveAllListeners());
        autoStop.ForEach(x => x.onClick.RemoveAllListeners());
        betButtons[0].onClick.AddListener(() => SetBetText(gameManager.UpDownBetAmount(1f, true,1)));
        betButtons[1].onClick.AddListener(() => SetBetText(gameManager.UpDownBetAmount(1f, false, 1)));
        betButtons[2].onClick.AddListener(() => SetBetText(gameManager.UpDownBetAmount(0f, true, 2)));
        betButtons[3].onClick.AddListener(() => SetBetText(gameManager.UpDownBetAmount(0f, false, 2)));
        betButtons[4].onClick.AddListener(() => SetBetText(gameManager.UpDownBetAmount(0f, true, 3)));
        betButtons[5].onClick.AddListener(() => SetBetText(gameManager.UpDownBetAmount(0f, false, 3)));
        autoStop[0].onClick.AddListener(() => SetStopText(gameManager.UpDownAutoStop(0.1f, true)));
        autoStop[1].onClick.AddListener(() => SetStopText(gameManager.UpDownAutoStop(0.1f, false)));
        autoStop[2].onClick.AddListener(() => SetStopText(gameManager.UpDownAutoStop(1f, true)));
        autoStop[3].onClick.AddListener(() => SetStopText(gameManager.UpDownAutoStop(1f, false)));
    }

    private void FixedUpdate()
    {
        fundoRealtimeAtualPosition = fundoOnMove ? fundoRealtimeAtualPosition + fundoRealtimeVelocity : fundoRealtimeAtualPosition;
        fundo.SetFloat("_RealTimeUpdate", fundoRealtimeAtualPosition);
    }

    public void SetWalletNickname(string nickname) => txtWalletNickname.text = nickname;
    public void SetWalletBalance(float balance) => txtWalletBalance.text = $"{balance:0.00}";
    public void SetTimer(int time) => txtTimerMult.text = $"{time:00:00}";
    public void SetMultplicador(float mult) => txtTimerMult.text = $"x {mult:00.00}";
    public void SetTimerMensagem(string time) => txtTimerMensagem.text = time;
    public void SetBonusTotal(float bonus) => txtBonusTotal.text = $"x {bonus:0.00}";
    public void SetTankState(string state)
    {
        switch (state)
        {
            case "Walking": 
                tank.Walking(true);
                fundoOnMove = true;
                break;
            case "Crash": 
                tank.Walking(false);
                tank.Crash(true);
                fundoOnMove = false;
                break;
            case "Repair":
                //Adicionar animaçao de reparo
                break;
        }
    }

    public void AddVelocityParalax(float value) => fundoRealtimeVelocity = fundoRealtimeVelocity == 0.2f ? fundoRealtimeVelocity : fundoRealtimeVelocity + value;
    
    public void ResetVelocityParalax() => fundoRealtimeVelocity = 0.05f;

    public void ActiveBet() => stopAnBet.interactable = true;

    public void DesactiveBet() => stopAnBet.interactable = false;

    public void SetBetButtonText(string txt) => txtStopAnBet.text = txt;

    public void SetBetText(float betV)
    {
        this.bet = betV;
        webClient.SetBetValor(betV);
        txtBetVal.text = $"{betV:0.00}";
    }

    public void SetStopText(float stopV)
    {
        this.stop = stopV;
        txtStopVal.text = $"{stopV:0.00}";
    }
}
