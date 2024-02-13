using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BV;

[Serializable]
public class RTPTank : MonoBehaviour
{
    [SerializeField] WebServer webServer;
    public int betBest, betGreat, betNormal;  //Base de configura�ao para chance de usar      [betBest = 15%, betGreat = 35% , betNormal = 50%]
    public float _RTP;              //RTP 0-100% (100 = 100%) retorno de investimento total somando todas jogadas
    public Bet playerBet;           //Jogada do jogador
    public List<Bet> geralBet = new(); //Jogadas de outros jogadores Simulaçao de apostas
    public float roundDistance;     // Tempo de duraçao da rodada
    public int numbRounds;          // Numero de jogadas ja contabilizadas
    public float totalBetIn;        // Total de creditos que entrou
    public float totalBetOut;       // Total de creditos que saiu
    public int bombBrak = 0;        // Total de bombas que foram quebradas
    public float playerCredits;     // Saldo do jogador
    public bool isPlayingRounds = false;
    [SerializeField] List<float> playerWinTotal = new List<float>();
    [SerializeField] List<float> bonusList = new List<float>();
    [SerializeField] List<float> bonusListValue = new List<float>();
    [SerializeField] List<float> bonusListOriginalValue = new List<float>();

    [SerializeField] TMP_Text tPlayerCredits , tPlayerWin, tBestReturn, tRTP, tCreditsIn, tCreditsOut;
    [SerializeField] TMP_InputField dBestChance, dGreatChance, dCreditsInicial, dRounds, dPlayers, dMaxBET,dMaxStop,dBombChance;
    [SerializeField] List<TMP_InputField> dBonus;
    [SerializeField] Toggle resetRTPtoggle, breakOnClashT;
    [SerializeField] TMP_Text roundTxt;
    [SerializeField] Button roundTxtButton;
    [SerializeField] TankConfiguration tankConfiguration = new();

    private void Start()
    {
        tankConfiguration = webServer.TankConfiguration;
    }

    private void FixedUpdate()
    {
        tCreditsIn.text =  totalBetIn > 0 ? $"{totalBetIn}" : "000";
        tCreditsOut.text = totalBetOut > 0 ? $"{totalBetOut}" : "000";
        tPlayerCredits.text = playerCredits > 0 ? $"{playerCredits}":"000";
        tPlayerWin.text = playerWinTotal.Count > 0 ? $"{playerWinTotal[playerWinTotal.Count - 1]:000}" : "000";
        tBestReturn.text = "000";
        tRTP.text = _RTP != 0 ? $"{_RTP:000.00} %" : "000 %";
        roundTxt.text = isPlayingRounds ? "Playng Wait ..." : "Simular Rounds";
        roundTxtButton.interactable = isPlayingRounds?false:true ;
    }

    public void ResetRTP()
    {
        _RTP = 0;
        playerBet = new Bet();
        geralBet = new List<Bet>();
        numbRounds = 0;
        totalBetIn = 0;
        totalBetOut = 0;
    }

    public void ResetOriginalValues()
    {
        ResetRTP();
        dCreditsInicial.text = $"{1000}";
        dRounds.text = $"{10000}";
        dPlayers.text = $"{10}";
        dMaxBET.text = $"{20}";
        dMaxStop.text = $"{20}";
        dBombChance.text = $"{11}";
        dBestChance.text = $"{15}";
        dGreatChance.text = $"{35}";
        dBonus.ForEach(x => x.text = dBonus.IndexOf(x)<bonusListOriginalValue.Count? $"{bonusListOriginalValue[dBonus.IndexOf(x)]}":$"{0.1}");
        resetRTPtoggle.isOn = true;
    }

    public void SimuleRounds()
    {
        if (resetRTPtoggle.isOn)
        {
            ResetRTP();
            bombBrak = 0;
        }
        isPlayingRounds = true;
        bonusListValue.Clear();
        dBonus.ForEach(x => bonusListValue.Add(float.Parse(x.text)));
        betBest = int.Parse(dBestChance.text);
        betGreat = int.Parse(dGreatChance.text);
        betNormal = 100 - betBest - betGreat;
        var parcial = int.Parse(dRounds.text) / 1000;
        for (int i = 0; i < parcial; i++)
        {
            playerCredits = AutoPlay(1000, int.Parse(dPlayers.text), int.Parse(dMaxBET.text), int.Parse(dMaxStop.text), float.Parse(dCreditsInicial.text), false ,breakOnClashT.isOn, int.Parse(dBombChance.text));
        }
        isPlayingRounds = false;
    }

    public float RoundPlay()
    {
        float multplier = 0.01f;
        float timeline = 0f;
        float finalMultplicador = 1.0f;
        int ind = 0;
        var luck = UnityEngine.Random.Range(0, 101);
        var range = luck <= betBest ? UnityEngine.Random.Range(0.02f, 6f) : luck <= betGreat ? UnityEngine.Random.Range(0.02f, 3f) : UnityEngine.Random.Range(0.02f, 0.2f);
        range = float.Parse($"{range:0.00}");
        roundDistance = range;
        while ((roundDistance + 0.01f) > timeline)
        {
            timeline += 0.01f;
            multplier = ind == 20 ? multplier + 0.01f : multplier;
            ind = ind == 20 ? 0 : ind + 1;
            if (ind == 20)
            {
                if (UnityEngine.Random.Range(0, 11) == 0)
                {
                    Debug.Log("Bomb Break");
                    return finalMultplicador;
                }
            }
            finalMultplicador = finalMultplicador + multplier;
            finalMultplicador = float.Parse($"{finalMultplicador:0.00}");
        }
        return finalMultplicador;
    }
    public float RoundPlay(bool bomb, int bombChance)
    {
        float multplier = 0.01f;
        float timeline = 0f;
        float finalMultplicador = 1.0f;
        int ind = 0;
        bonusList.Clear();
        var luck = UnityEngine.Random.Range(0, 101);
        var range = luck <= betBest ? UnityEngine.Random.Range(0.02f, 6f) : luck <= betGreat ? UnityEngine.Random.Range(0.02f, 3f) : UnityEngine.Random.Range(0.02f, 0.2f);
        range = float.Parse($"{range:0.00}");
        roundDistance = range;
        while ((roundDistance + 0.01f) > timeline)
        {
            timeline += 0.01f;
            multplier = ind == 20 ? multplier + 0.01f : multplier;
            ind = ind == 20 ? 0 : ind + 1;
            if (ind == 20)
            {
                var r = UnityEngine.Random.Range(0, bombChance < bonusListValue.Count?bombChance : bonusListValue.Count);
                if (bomb && r == 0)
                {
                    Debug.Log("Bomb Break");
                    return finalMultplicador;
                }else { bonusList.Add(bonusListValue[r]); }
            }
            finalMultplicador = finalMultplicador + multplier;
            finalMultplicador = float.Parse($"{finalMultplicador:0.00}");
        }
        return finalMultplicador;
    }

  
    public float RoundPlay(float duration, bool bomb, int bombChance)
    {
        float multplier = 0.01f;
        float timeline = 0f;
        float finalMultplicador = 1.0f;
        int ind = 0;
        roundDistance = duration;
        while ((roundDistance + 0.01f) > timeline)
        {
            timeline += 0.01f;
            multplier = ind == 20 ? multplier + 0.01f : multplier;
            ind = ind == 20 ? 0 : ind + 1;
            if (ind == 20)
            {
                if (bomb && UnityEngine.Random.Range(0, bombChance) == 0)
                {

                    bombBrak++;
                    return finalMultplicador;
                }
            }
            finalMultplicador = finalMultplicador + multplier;
            finalMultplicador = float.Parse($"{finalMultplicador:0.00}");
        }
        return finalMultplicador;
    }

    public void AtualizeConfigurarion()
    {
        tankConfiguration.bestChance = int.Parse(dBestChance.text);
        tankConfiguration.greatChance = int.Parse(dGreatChance.text);
        tankConfiguration.bombChance = int.Parse(dBombChance.text);
        WebClient.Instance.SendMensagem(tankConfiguration);
    }

    //public float AutoPlay(int nJogadas, int nMedioJogadores, int maxCoin, int maxStop, float inicialPlayerCoin, bool resetRtp, bool breakOutMoney, int bombChance)
    //{
    //    float coin = inicialPlayerCoin;
    //    playerWinTotal.Clear();
    //    if (resetRtp)
    //    {
    //        ResetRTP();
    //        bombBrak = 0;
    //    }
    //    for (int i = 0; i < nJogadas; i++)
    //    {
    //        var playerBetII = new Bet() { value = 10, stop = UnityEngine.Random.Range(1.01f, maxStop) };
    //        geralBet.Clear();
    //        for (int j = 0; j < nMedioJogadores; j++)
    //        {
    //            MakeFakeBets(maxCoin, maxStop);
    //        }
    //        var atualBet = bombChance > 0 ? RoundPlay(true, bombChance) : RoundPlay(false, 0);
    //        coin -= playerBetII.value;
    //        totalBetIn += playerBetII.value;
    //        geralBet.ForEach(x => { if (x.stop < atualBet)
    //            {
    //                var finalMultplicador = 0f;
    //                bonusList.ForEach(x => finalMultplicador += x);
    //                totalBetOut += (x.value + finalMultplicador) * x.stop;
    //            }});
    //        if (playerBetII.stop < atualBet)
    //        {
    //            var finalMultplicador = 0f;
    //            bonusList.ForEach(x => finalMultplicador += x);
    //            totalBetOut +=(finalMultplicador + playerBetII.value )* playerBetII.stop;
    //            coin += (finalMultplicador + playerBetII.value )* playerBetII.stop;
    //            playerWinTotal.Add((finalMultplicador + playerBetII.value )* playerBetII.stop);
    //        }
    //        if (coin <= 0 && breakOutMoney) break;
    //        Debug.Log($"Rodada :{i}[ {atualBet} / {playerBetII.stop} ] - Gasto Atual : {totalBetIn} - Retorno {totalBetOut}");
    //    }
    //    _RTP = 100 - (((totalBetIn - totalBetOut) / totalBetIn) * 100);
    //    return coin;
    //}

   
    public float AutoPlay(int nJogadas, int nMedioJogadores, int maxCoin, int maxStop, float inicialPlayerCoin, bool resetRtp, bool breakOutMoney, int bombChance)
    {
        var conf = new TankConfiguration()
        {
            bestChance = int.Parse(dBestChance.text),
            greatChance = int.Parse(dGreatChance.text),
            bonusList = bonusListValue,
            bombChance = bombChance,
            maxRange = 6f,
            maxMultiplicador = 0.2f
        };
        float coin = inicialPlayerCoin;
        playerWinTotal.Clear();
        if (resetRtp)
        {
            ResetRTP();
            bombBrak = 0;
        }
        for (int i = 0; i < nJogadas; i++)
        {
            var playerBetII = new Bet() { value = 10, stop = UnityEngine.Random.Range(1.01f, maxStop) };
            geralBet.Clear();
            bonusList.Clear();
            for (int j = 0; j < nMedioJogadores; j++)
            {
                MakeFakeBets(maxCoin, maxStop);
            }
            var atualBet = Mathematics.TankCalculeRound(conf);
            bonusList = atualBet.bonus;
            coin -= playerBetII.value;
            totalBetIn += playerBetII.value;
            geralBet.ForEach(x => {
                if (x.stop < atualBet.timeRound)
                {
                    var finalMultplicador = 0f;
                    bonusList.ForEach(x => finalMultplicador += x);
                    totalBetOut += (x.value + finalMultplicador) * x.stop;
                }
            });
            if (playerBetII.stop < atualBet.timeRound)
            {
                var finalMultplicador = 0f;
                bonusList.ForEach(x => finalMultplicador += x);
                totalBetOut += (finalMultplicador + playerBetII.value) * playerBetII.stop;
                coin += (finalMultplicador + playerBetII.value) * playerBetII.stop;
                playerWinTotal.Add((finalMultplicador + playerBetII.value) * playerBetII.stop);
            }
            if (coin <= 0 && breakOutMoney) break;
            Debug.Log($"Rodada :{i}[ {atualBet.timeRound} / {playerBetII.stop} ] - Gasto Atual : {totalBetIn} - Retorno {totalBetOut}");
        }
        _RTP = 100 - (((totalBetIn - totalBetOut) / totalBetIn) * 100);
        return coin;
    }

    public void MakeFakeBets(int creditosMax, int stopMax)
    {
        var d = UnityEngine.Random.Range(0, 2);
        if (d == 0)
        {
            var c = UnityEngine.Random.Range(1, creditosMax + 1);
            float dd = (float)Math.Round(UnityEngine.Random.Range(1.01f, stopMax), 2);
            geralBet.Add(new Bet() { coin = "C", value = c, addressID = $"Player{UnityEngine.Random.Range(0, 99999)}", stop = dd });
            totalBetIn += c;
        }
    }// Para proposito de testes : Simula apostas feitas por outro jogadores
}