using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [SerializeField] List<Toggle> valUpgrade = new();
    [SerializeField] List<int> stepUpgree = new();
    [SerializeField] bool stepStop = false;

    [SerializeField] List<Toggle> autoPlayAction = new();
    [SerializeField] List<Toggle> autoPlayRounds = new();
    [SerializeField] List<int> rounds;
    [SerializeField] List<TMP_InputField> autoPlayStopIncDec = new();

    [SerializeField] Toggle autoStop, autoPlay;
    [SerializeField] bool autoPlayStop = false;
    [SerializeField] float autoStopInicial;
    [SerializeField] int autoStopRoundAtual;
    [SerializeField] int autoStopRoundAtualCicle;

    [SerializeField] bool automaticPlay = false;
    [SerializeField] bool automaticPlayThisTurn = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if(isWalking && automaticPlayThisTurn) automaticPlayThisTurn = false;
    }

    public void CheckAutoPlay()
    {
        if (!automaticPlayThisTurn && automaticPlay)
        {
            automaticPlayThisTurn = true;
            SendBetAutomatic();
        }
    }

    public void SetUpgradeToggle(int t)
    {
        if (valUpgrade[t].isOn) valUpgrade.ForEach(x => { if (x != valUpgrade[t]) x.isOn = false; });
    }

    public void AutoCashoutCheck() => autoStop.isOn = autoPlayStop ? true : autoStop.isOn;

    public void SetAutoPlayToggle()
    {
        autoPlayStop = !autoPlayStop;
        autoPlayAction.ForEach(x => { x.interactable = autoPlayStop; x.isOn = false; });
        autoStop.isOn = !autoPlayStop ? autoStop.isOn : true;
    }

    public void SetAutoPlayRoundsToggle()
    {
        if (autoPlayAction[0].isOn || autoPlayAction[1].isOn)
        {
            autoPlayAction[2].isOn = true;
            return;
        }
        autoPlayRounds.ForEach(x => { x.interactable = autoPlayAction[2].isOn; x.isOn = false; });
        autoPlayRounds[0].isOn = autoPlayAction[2].isOn;
    }

    public void SetAutoPlayRoundsAtual(int t)
    {
        if (autoPlayRounds[t].isOn) autoPlayRounds.ForEach(x => { if (x != autoPlayRounds[t]) x.isOn = false; });
        var i = 0;
        autoPlayRounds.ForEach(x => { if (!x.isOn) i++; });
        if (i == autoPlayRounds.Count) autoPlayRounds[0].isOn = autoPlayAction[2].isOn;
        autoStopRoundAtualCicle = rounds[t];
        autoStopRoundAtual = 0;

    }

    public void SetAutoPlayStopIncDecAtual(int t)
    {
        autoPlayStopIncDec[t].interactable = autoPlayAction[t].isOn;
        autoPlayAction[2].isOn = true;
        autoPlayRounds.ForEach(x => { x.interactable = true; });
        autoPlayRounds[0].isOn = true;
    }

    public float UpDownAutoStop(float modify, bool toUp)
    {
        stopValBet = toUp ? stopValBet + modify : stopValBet - modify;
        stopValBet = stopValBet < 1.01f ? 1.01f : stopValBet;
        return stopValBet;
    }// Controle de valores, para onde parar automaticamente
    public float UpDownAutoStop(bool toUp)
    {
        if (stepStop)
        {
            var step = StepCount();
            if (toUp) stopValBet = step != -1 ? stopValBet + stepUpgree[step] : stopValBet + 1;
            else stopValBet = step != -1 ? stopValBet - stepUpgree[step] : stopValBet - 1;
            stopValBet = stopValBet < 1f ? 1f : stopValBet;
            return stopValBet;
        }
        else
        {
            stopValBet = toUp ? stopValBet + 1 : stopValBet - 1;
            stopValBet = stopValBet < 1f ? 1f : stopValBet;
            return stopValBet;
        }
    }// Controle de valores, para onde parar automaticamente

    public float UpDownBetAmount(bool toUp)
    {
        var step = StepCount();
        if (toUp) valueToBet = step != -1 ? valueToBet + stepUpgree[step] : valueToBet + 1;
        else valueToBet = step != -1 ? valueToBet - stepUpgree[step] : valueToBet - 1;
        valueToBet = valueToBet < 1 ? 1f : valueToBet > credits ? credits : valueToBet;
        return valueToBet;
    }//Controle de Valores da aposta

    public float UpDownBetAmount(float modify, bool toUp, int step)
    {
        if (toUp) valueToBet = step == 1 ? valueToBet + modify : step == 2 ? valueToBet * 2 : credits;
        else valueToBet = step == 1 ? valueToBet - modify : step == 2 ? valueToBet / 2 : 1f;
        valueToBet = valueToBet < 1 ? 1f : valueToBet > credits ? credits : valueToBet;
        return valueToBet;
    }//Controle de Valores da aposta

    public int StepCount()
    {
        int step = -1;
        valUpgrade.ForEach(x => { if (x.isOn) step = valUpgrade.IndexOf(x); });
        return step;
    }

    public void SendBet()
    {
        if (autoPlayStop)
        {
            var max = autoPlayStopIncDec[0].text != "" ? int.Parse(autoPlayStopIncDec[0].text) : 0;
            var min = autoPlayStopIncDec[1].text != "" ? int.Parse(autoPlayStopIncDec[1].text) : 0;
            autoStopInicial = credits;
            autoStopRoundAtual = 0;
            automaticPlay = true;
            webClient.SendBet(valueToBet, autoStop.isOn ? stopValBet : 0);
        }
        else { webClient.SendBet(valueToBet, autoStop.isOn ? stopValBet : 0); }
    }
    public void SendBetAutomatic()
    {
            var max = autoPlayStopIncDec[0].text != "" ? int.Parse(autoPlayStopIncDec[0].text) : 0;
            var min = autoPlayStopIncDec[1].text != "" ? int.Parse(autoPlayStopIncDec[1].text) : 0;
            if (autoPlayAction[0].isOn && autoPlayAction[1].isOn) // Usa valor Maximo e Minimo que os creditos devem chegar, para se alcançar algum desses
            {
                if (credits > (autoStopInicial - min))
                {
                    if (credits > (autoStopInicial + max)) { automaticPlay = false; } //Stop chegou no maximo
                    else
                    {
                        webClient.SendBet(valueToBet, stopValBet);
                        automaticPlay = true;
                        Debug.Log($"SendBet Inc/Dec ");
                    }
                }
                else { automaticPlay = false; }    //stop chegou no minimo
            }
            else if (autoPlayAction[0].isOn)
            {                  // Usa Somente valor Maximo que os creditos devem chegar
                if (credits < (autoStopInicial + max))
                {
                    webClient.SendBet(valueToBet, stopValBet);
                    automaticPlay = true;
                    Debug.Log($"SendBet Inc ");
                }
                else { automaticPlay = false; }//stop
            }
            else if (autoPlayAction[1].isOn)                    // Usa Somente valor Minimo que os creditos devem chegar
            {
                if (credits > (autoStopInicial + min))
                {
                    webClient.SendBet(valueToBet, stopValBet);
                    automaticPlay = true;
                    Debug.Log($"SendBet Dec ");
                }
                else { automaticPlay = false; }//stop
            }
            else if (autoPlayAction[2].isOn && autoStopRoundAtual < autoStopRoundAtualCicle)
            {
                webClient.SendBet(valueToBet, stopValBet);
                automaticPlay = true;
                Debug.Log($"SendBet Repeat ");
                autoStopRoundAtual++;
            }
            else { automaticPlay = false; } //stop
    }
}
