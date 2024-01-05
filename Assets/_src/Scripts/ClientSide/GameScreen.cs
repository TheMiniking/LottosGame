using System;
using System.Collections;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class GameScreen : BaseScreen
{
    [SerializeField] Player tank;
    [SerializeField] Material fundo;
    [SerializeField] float fundoRealtimeVelocity;
    [SerializeField] float fundoRealtimeAtualPosition;
    [SerializeField] bool fundoOnMove = false;

    [SerializeField] TMP_Text txtWalletBalance, txtWalletNickname;
    [SerializeField] TMP_Text txtTimerMult , txtTimerMensagem, txtBonusTotal;

    private void Start()
    {
        fundo.SetInt("_UseScriptTime", 1);
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

}
