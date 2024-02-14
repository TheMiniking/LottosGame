﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Unity.VisualScripting;


public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;

    [SerializeField] GameScreen gameScreen;
    [SerializeField] TextMeshProUGUI loadingMsg;
    [SerializeField] GameObject loadingPanel;


    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        gameScreen.SetActive(true);
        //CreditConversor();
    }

    void Update()
    {
        //if(loginScreen.ActiveSelf())loginScreen.Update();
        //if(menuScreen.ActiveSelf()) menuScreen.Update();
    }
    public void ShowLoading(string text)
    {
        loadingPanel.SetActive(true);
        loadingMsg.text = text;
    }
    public void HideLoading()
    {
        loadingPanel.SetActive(false);
        loadingMsg.text = "";
    }

    void DesactiveAll()
    {
        gameScreen.SetActive(false);
    }
    public void ActiveGame(bool active)
    {
        DesactiveAll();
        gameScreen.SetActive(active);
    }
    public void ActiveLogin()
    {
        DesactiveAll();
    }

    //public void ActiveLoading()
    //{
    //    loading.SetActive(true);
    //}
    //public void DisableLoading()
    //{
    //    loading.SetActive(false);
    //}
    //internal void SetBet(byte currentBet)
    //{
    //    gameScreen.SetBet(currentBet);
    //}

    public void SetTimer(int time) => gameScreen.SetTimer(time);

    public void SetMultiplicador(float time) => gameScreen.SetMultplicador(time);

    public void SetTimerMensagem(string time) => gameScreen.SetTimerMensagem(time);

    public void SetWalletNick(string user) => gameScreen.SetWalletNickname(user);

    public void SetWalletBalance(double balance) => gameScreen.SetWalletBalance(balance);

    public void SetBonusTotal(float bonus) => gameScreen.SetBonusTotal(bonus);

    public void SetTankState(string state) => gameScreen.SetTankState(state);

    public void AddVelocityParalax(float value) => gameScreen.AddVelocityParalax(value);

    public void ResetVelocityParalax() => gameScreen.ResetVelocityParalax();

    public void SetBetActive() => gameScreen.ActiveBet();

    public void SetBetDesactive() => gameScreen.DesactiveBet();

    public void SetBetButtonStop(float mult) => gameScreen.SetBetButtonText(mult);

    public void SetLastPlays(float last) => gameScreen.SetLastResult(last);

    public void SetPlayersBet(BetPlayers bet) => gameScreen.SetBetPlayersList(bet);

    public void SetPlayersWin(BetPlayers bet) => gameScreen.SetBetPlayersList(bet);

    public void ResetPlayersBet() => gameScreen.ResetBetPlayers();

    public void InstancieBox() => gameScreen.InstantiateBox();

    public void InstancieBox(float bonus) => gameScreen.InstantiateBox(bonus);

    public void PlayMensagen(string msg) => gameScreen.PlayMensagen(msg);

    public void AddCashOut(float cash)=> gameScreen.totalCashOut += cash;

    public void AddCashBet(float cash) => gameScreen.totalCashBet += cash;
}



