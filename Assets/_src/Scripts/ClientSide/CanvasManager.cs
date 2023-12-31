﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Unity.VisualScripting;


public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance { get; private set; }

    [SerializeField] GameScreen gameScreen;


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

    public void SetWalletBalance(float balance) => gameScreen.SetWalletBalance(balance);

    public void SetBonusTotal(float bonus) => gameScreen.SetBonusTotal(bonus);

    public void SetTankState(string state) => gameScreen.SetTankState(state);

    public void AddVelocityParalax(float value) => gameScreen.AddVelocityParalax(value);

    public void ResetVelocityParalax() => gameScreen.ResetVelocityParalax();
}


