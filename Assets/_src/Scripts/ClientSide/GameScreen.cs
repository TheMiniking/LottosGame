﻿using BV;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GameScreen : BaseScreen
{
    public static GameScreen instance;
    public bool logs = false;
    //---------- Mobile / Desktop ------------
    [SerializeField] int screen = 0;
    [SerializeField] GameObject mobile, mobileFundo;
    [SerializeField] GameObject desktop, desktopFundo;

    // ---------- Tank e Background -------------
    [SerializeField] Player tank, tankMobile;
    [SerializeField] Material fundo;
    [SerializeField] float fundoRealtimeVelocity = 0.02f;
    [SerializeField] float fundoRealtimeAtualPosition;
    [SerializeField] bool fundoOnMove = false;

    //----------- Box -------------------
    [SerializeField] float bonusTotal;
    [SerializeField] List<float> bonusList = new();
    [SerializeField] GameObject boxPrefab;
    [SerializeField] Transform fieldBonus;
    [SerializeField] Vector3 initBox, initBoxMobile;
    [SerializeField] Vector3 direcaoBonus = new();
    [SerializeField] float velocityBonus = 0.1f;
    [SerializeField] List<GameObject> boxOBJ = new();
    [SerializeField] public List<BoxTank> boxT = new();

    //-------------Rank-----------------
    [SerializeField] GameObject rankCanvas, rankCanvasMobile;
    [SerializeField] public List<BetPlayers> rankMultiplier = new();
    [SerializeField] public List<BetPlayers> rankCash = new();
    [SerializeField] public List<BetPlayersHud> rankSlotsMultiplier, rankSlotsCash, rankSlotsCashMobile, rankSlotsMultiplierMobile = new();

    // ---------- UI ----------------
    // ---------- Player ----------------
    [SerializeField] TMP_Text txtWalletBalance, txtWalletNickname, txtWalletBalanceMobile, txtWalletNicknameMobile;
    //---------- Players --------------
    [Header("Players")]
    [SerializeField] Button playerControl;
    [SerializeField] Button rankControl, playerControlMobile, rankControlMobile;
    [SerializeField] GameObject playerPanel, playerPanelMobile;


    // ---------- Game ----------------
    public float bet, stop = 0;
    [SerializeField] public float totalCashOut, totalCashBet, playerInBet, playerInBetWinner = 0;
    [SerializeField] public TMP_Text playerInBetTxt, playerInBetWinnerTxt, playerInBetTxtMobile, playerInBetWinnerTxtMobile;
    [SerializeField] TMP_Text txtTimerMult, txtTimerMensagem, txtBonusTotal, txtTotalCashOut, txtTotalCashBet;
    [SerializeField] TMP_Text txtTimerMultMobile, txtTimerMensagemMobile, txtBonusTotalMobile, txtTotalCashOutMobile, txtTotalCashBetMobile;
    [SerializeField] TMP_Text mensagen, roundsTxt;
    [SerializeField] TMP_Text mensagenMobile, roundsTxtMobile;
    [SerializeField] Animator animMensagen, animMensagenMobile;
    [SerializeField] List<Sprite> lastResultSprite = new();
    [SerializeField] GameObject lastFivityOBJ, lastFivityOBJMobile, roundsPanelMobile;
    [SerializeField] List<float> lastResult, lastFivityResult = new();
    [SerializeField] List<LastSlot> lastResultObj, lastSlotFivity = new();
    [SerializeField] List<LastSlot> lastResultObjMobile, lastSlotFivityMobile = new();

    // ---------- Bet Painel ----------------
    [SerializeField] Button stopAnBet, stopAnBetMobile;
    [SerializeField] TMP_Text txtStopAnBet, txtBetVal, txtStopAnBetMobile, txtBetValMobile, stopValMobile;
    [SerializeField] TMP_InputField stopVal, betInput;
    [SerializeField] List<Button> betButtons, autoStop, autoStopMobile = new();
    [SerializeField] Toggle autoCashOutToggle, autoCashOutToggleMobile;//adicionado por robson
    [SerializeField] Toggle autoPlayToggle, autoPlayToggleMobile;//adicionado por robson
    [SerializeField] WinExtra extraWin;
    [SerializeField] Transform extraWinPanel, extraWinPanelMobile;

    // ---------- Bet Game ----------------
    [SerializeField] List<BetPlayers> playersBetList = new();
    [SerializeField] List<BetPlayersHud> playersBet, playersBetMobile = new();

    //---------- Mobile Teclado ------------
    [SerializeField] GameObject teclado;
    [SerializeField] List<Button> tecladoButtons = new();   // 0-9 = num, 10 = ponto, 11 = backspace, 12 = enter,13 = cancel
    [SerializeField] TMP_Text tecladoText;
    [SerializeField]
    string tecladoTextValue = string.Empty;
    [SerializeField] bool tecladoMode = false;              // false = bet, true = auto stop

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SetCanvas();
        fundo.SetInt("_UseScriptTime", 1);

        playerControl.onClick.AddListener(ShowPlayers);
        rankControl.onClick.AddListener(ShowRank);
        playerControlMobile.onClick.AddListener(ShowPlayers);
        rankControlMobile.onClick.AddListener(ShowRank);

        autoCashOutToggle.onValueChanged.AddListener(x => GameManager.Instance.AutoCashout(x));
        autoPlayToggle.onValueChanged.AddListener(x => GameManager.Instance.AutoStop(x));
        autoCashOutToggleMobile.onValueChanged.AddListener(x => GameManager.Instance.AutoCashout(x));
        autoPlayToggleMobile.onValueChanged.AddListener(x =>
        {
            GameManager.Instance.AutoStop(x);
            if (x)
            {
                roundsPanelMobile.SetActive(true);
            }
        });
        autoStop[1].onClick.AddListener(() =>
        {
            string v = Regex.Replace(stopVal.text.Replace(".", ","), @"[^0-9,]", string.Empty);
            float.TryParse(v, out float s);
            Debug.LogWarning(v + " less " + s);
            s = Mathf.Max(1f, s - 0.10f);
            SetStopText(s);
        });
        autoStop[0].onClick.AddListener(() =>
        {
            string v = Regex.Replace(stopVal.text.Replace(".", ","), @"[^0-9,]", string.Empty);
            float.TryParse(v, out float s);
            Debug.LogWarning(v + " More " + s);
            s = Mathf.Min(1000f, s + 0.10f);
            SetStopText(s);
        });
        autoStopMobile[1].onClick.AddListener(() =>
        {
            string v = Regex.Replace(stopValMobile.text.Replace(".", ","), @"[^0-9,]", string.Empty);
            float.TryParse(v, out float s);
            Debug.LogWarning(v + " less " + s);
            s = Mathf.Max(1f, s - 1.0f);
            SetStopText(s);
        });
        autoStopMobile[0].onClick.AddListener(() =>
        {
            string v = Regex.Replace(stopValMobile.text.Replace(".", ","), @"[^0-9,]", string.Empty);
            float.TryParse(v, out float s);
            Debug.LogWarning(v + " More " + s);
            s = Mathf.Min(1000f, s + 1.0f);
            SetStopText(s);
        });
        stopVal.onValueChanged.AddListener(x =>
        {
            string resultado = Regex.Replace(x.Replace(".", ","), "[^0-9,]", string.Empty);
            stopVal.SetTextWithoutNotify(resultado);
        });
        stopVal.onEndEdit.AddListener(x =>
        {
            string resultado = Regex.Replace(x.Replace(".", ","), "[^0-9,]", string.Empty);
            float n = 0;
            float.TryParse(resultado, out n);
            n = Mathf.Clamp(n, 1, 1000);
            SetStopText(n);
        });
        betInput.onEndEdit.AddListener(x =>
        {
            int n = 0;
            int.TryParse(x, out n);
            n = Mathf.Clamp(n, 1, 100);
            SetBetText(n);
        });
        ResetBetPlayers();
        ShowPlayers();
    }

    void SetCanvas()
    {
        screen = (Screen.width > Screen.height) ? 0 : 1;
        desktop.SetActive(screen == 0);
        desktopFundo.SetActive(screen == 0);
        mobile.SetActive(screen == 1);
        mobileFundo.SetActive(screen == 1);
    }
    void SetCanvas(bool desk)
    {
        desktop.gameObject.SetActive(desk);
        desktopFundo.gameObject.SetActive(desk);
        mobile.gameObject.SetActive(!desk);
        mobileFundo.gameObject.SetActive(!desk);
    }


    public void SetStopText(float stop)
    {
        if (logs)
        {
            Debug.Log($"SetStopText: {stop}");
        }

        this.stop = stop;
        stopVal.SetTextWithoutNotify($"x {stop:0.00}");
        stopValMobile.text = $"x {stop:0.00}";
        GameManager.Instance.SetAutoStop(stop);
    }

    IEnumerator MoveCarret()
    {
        yield return new WaitForEndOfFrame();
        stopVal.MoveTextEnd(false);
    }
    void FixedUpdate()
    {
        fundoRealtimeAtualPosition = fundoOnMove ? (fundoRealtimeAtualPosition + fundoRealtimeVelocity) : fundoRealtimeAtualPosition;
        fundo.SetFloat("_RealTimeUpdate", fundoRealtimeAtualPosition);
        if (boxT.Count > 0)
        {
            boxT.ForEach(x =>
        {
            x.currentBox.gameObject.transform.position += velocityBonus * direcaoBonus * (fundoRealtimeVelocity * (fundoOnMove ? 1 : 0));
            if (x.currentBox.gameObject.transform.position.x - 100 <= ((screen == 0) ? tank.gameObject.transform.position.x : tankMobile.gameObject.transform.position.x) - 50) { StartCoroutine(Open(x.currentBox)); }
        });
        }

        txtBonusTotal.text = $"x {bonusTotal:0.00}";
        txtBonusTotalMobile.text = $"x {bonusTotal:0.00}";
        txtTotalCashOut.text = $"$ {totalCashOut:#,0.00}";
        txtTotalCashOutMobile.text = $"$ {totalCashOut:#,0.00}";
        txtTotalCashBet.text = $"$ {totalCashBet:#,0.00}";
        txtTotalCashBetMobile.text = $"{totalCashBet:#,0.00}";
        playerInBetTxt.text = $" {playerInBet}";
        playerInBetTxtMobile.text = $" {playerInBet}";
        playerInBetWinnerTxt.text = $"{playerInBetWinner}";
        playerInBetWinnerTxtMobile.text = $"{playerInBetWinner}";
    }

    public void SetRoundsTxt(int rounds)
    {
        roundsTxt.text = (rounds == -1) ? "Inf" : ($"{rounds}");
        roundsTxtMobile.text = (rounds == -1) ? "Inf" : ($"{rounds}");
    }

    public void SetWalletNickname(string nickname)
    {
        if (logs)
        {
            Debug.Log($"SetWalletNickname: {nickname}");
        }

        txtWalletNickname.text = nickname;
    }
    public void SetWalletBalance(double balance)
    {
        if (logs)
        {
            Debug.Log($"SetWalletBalance: {balance}");
        }

        txtWalletBalance.text = $"$ {balance:#,0.00}";
        txtWalletBalanceMobile.text = $"$ {balance:#,0.00}";
    }
    public void SetTimer(int time)
    {
        if (logs)
        {
            Debug.Log($"SetTimer: {time}");
        }

        txtTimerMult.text = $"   {time:00:00}";
        txtTimerMensagem.text = "Next Round in:";
        txtTimerMultMobile.text = $"   {time:00:00}";
        txtTimerMensagemMobile.text = "Next Round in:";

    }
    public void SetMultplicador(float mult)
    {
        if (logs)
        {
            // Debug.Log($"SetMultplicador: {mult}");
        }

        txtTimerMult.text = $"x {mult:00.00}";
        txtTimerMensagem.text = "Stop in:";
        txtTimerMultMobile.text = $"x {mult:00.00}";
        txtTimerMensagemMobile.text = "Stop in:";
    }
    public void SetTimerMensagem(string time)
    {
        if (logs)
        {
            Debug.Log($"SetTimerMensagem: {time}");
        }

        txtTimerMensagem.text = time;
        txtTimerMensagemMobile.text = time;
    }
    public void SetBonusTotal(float bonus)
    {
        if (logs)
        {
            Debug.Log($"SetBonusTotal: {bonus}");
        }

        txtBonusTotal.text = $"x {bonus:0.00}";
    }
    public void SetTankState(string state)
    {
        if (logs)
        {
            Debug.Log($"SetTankState: {state}");
        }

        switch (state)
        {
            case "Walking":
                tank.Walking(true);
                tankMobile.Walking(true);
                fundoOnMove = true;
                break;
            case "Crash":
                tank.Walking(false);
                tank.Crash(true);
                tankMobile.Walking(false);
                tankMobile.Crash(true);
                fundoOnMove = false;
                break;
            case "Repair":
                //Adicionar animaçao de reparo
                break;
        }
    }

    public void AddVelocityParalax(float value)
    {
        if (logs)
        {
            Debug.Log($"AddVelocityParalax: {value}");
        }

        fundoRealtimeVelocity = (fundoRealtimeVelocity == 0.2f) ? fundoRealtimeVelocity : (fundoRealtimeVelocity + value);
    }

    public void ResetVelocityParalax()
    {
        if (logs)
        {
            Debug.Log($"ResetVelocityParalax: {fundoRealtimeVelocity}");
        }

        fundoRealtimeVelocity = 0.07f;
    }

    public void ActiveBet()
    {
        if (logs)
        {
            Debug.Log($"ActiveBet");
        }

        stopAnBet.interactable = true;
        txtStopAnBet.text = $"Bet";
        stopAnBetMobile.interactable = true;
        txtStopAnBetMobile.text = $"Bet";
    }

    public void DesactiveBet()
    {
        if (logs)
        {
            Debug.Log($"DesactiveBet");
        }

        stopAnBet.interactable = false;
        txtStopAnBet.text = $"Wait Round";
        stopAnBetMobile.interactable = false;
        txtStopAnBetMobile.text = $"Wait Round";
    }

    public void SetBetButtonText(float mult)
    {
        if (logs)
        {
            Debug.Log($"SetBetButtonText: {mult}");
        }

        stopAnBet.interactable = true;
        txtStopAnBet.text = $"Stop x{mult:0.00}";
        stopAnBetMobile.interactable = true;
        txtStopAnBetMobile.text = $"Stop x{mult:0.00}";
    }

    public void SetBetText(double betV)
    {
        if (logs)
        {
            Debug.Log($"SetBetText: {betV}");
        }

        bet = (float)betV;
        betInput.SetTextWithoutNotify($"{bet}");
        WebClient.Instance.SetBetValor((float)betV);
        betInput.text = $"{betV}";
        txtBetValMobile.text = $"{betV}";
    }

    public void SetLastResult(float result)
    {
        if (logs)
        {
            Debug.Log($"SetLastResult: {result}");
        }

        lastResult.Add(result);
        lastFivityResult.Add(result);
        if (lastResult.Count > ((screen == 0) ? lastResultObj.Count : lastResultObjMobile.Count))
        {
            lastResult.RemoveAt(0);
        }

        if (lastFivityResult.Count > ((screen == 0) ? lastSlotFivity.Count : lastSlotFivityMobile.Count))
        {
            lastFivityResult.RemoveAt(0);
        }

        SetLastFivitySlots(result);
        //lastResultObj.ForEach(x => x.SetActive(false));
        LastSlot slot = lastResultObj.Last();
        slot.SetBet(result);
        slot.transform.SetAsFirstSibling();
        lastResultObj.Remove(slot);
        lastResultObj.Insert(0, slot);
        LastSlot slotII = lastResultObjMobile.Last();
        slotII.SetBet(result);
        slotII.transform.SetAsFirstSibling();
        lastResultObjMobile.Remove(slotII);
        lastResultObjMobile.Insert(0, slotII);
    }

    public void ActiveSlotFivity()
    {
        lastFivityOBJ.SetActive(!lastFivityOBJ.activeSelf);
        lastFivityOBJMobile.SetActive(!lastFivityOBJMobile.activeSelf);
    }

    public void SetLastFivitySlots(float mult)
    {
        LastSlot slot = lastSlotFivity.Last();
        slot.SetBet(mult);
        slot.transform.SetAsFirstSibling();
        lastSlotFivity.Remove(slot);
        lastSlotFivity.Insert(0, slot);
        LastSlot slotI = lastSlotFivityMobile.Last();
        slotI.SetBet(mult);
        slotI.transform.SetAsFirstSibling();
        lastSlotFivityMobile.Remove(slotI);
        lastSlotFivityMobile.Insert(0, slotI);
    }

    public void ResetBetPlayers()
    {
        if (logs)
        {
            Debug.Log($"ResetBetPlayers");
        }

        playersBet.ForEach(x => x.Clear());
        playersBetMobile.ForEach(x => x.Clear());
        ResetBonus();
        bonusTotal = 0;
        playerInBet = 0;
        playerInBetWinner = 0;
    }

    public void SetBetPlayersList(BetPlayers bet)
    {
        if (logs)
        {
            Debug.Log($"SetBetPlayersList: {bet.name}");
        }

        int index = playersBet.FindIndex(b => b.name.text == bet.name);
        if (index == -1)
        {
            index = playersBet.Count - 1;
        }

        BetPlayersHud p = playersBet[index];
        p.Set(bet);
        p.transform.SetAsFirstSibling();
        playersBet.RemoveAt(index);
        playersBet.Insert(0, p);
        index = playersBetMobile.FindIndex(b => b.name.text == bet.name);
        if (index == -1)
        {
            index = playersBetMobile.Count - 1;
        }

        BetPlayersHud g = playersBetMobile[index];
        g.Set(bet);
        g.transform.SetAsFirstSibling();
        playersBetMobile.RemoveAt(index);
        playersBetMobile.Insert(0, g);
        if (!playerShow && (bet.multiplier > 0))
        {
            WinExtra h = Instantiate(extraWin, (screen == 0) ? extraWinPanel : extraWinPanelMobile);
            h.SetText($"{bet.name}  $ {bet.multiplier * bet.value:#,0.00}");
        }
    }

    public void SetBetPlayersWin(BetPlayers bet)
    {
        if (logs)
        {
            Debug.Log($"SetBetPlayersWin: {bet.name} bet {bet.value} x {bet.multiplier}");
        }
        ////ResetBetPlayers();
        //playersBetList.Add(bet);
        //if (playersBetList.Count > playersBet.Count) playersBetList.RemoveAt(0);
        //playersBetList.ForEach(x => {
        //    playersBet[playersBetList.IndexOf(x)].gameObject.SetActive(true);
        //    playersBet[playersBetList.IndexOf(x)].name.text = x.name;
        //    playersBet[playersBetList.IndexOf(x)].bet.text = $"{x.value:0.00} C";
        //    playersBet[playersBetList.IndexOf(x)].credits.text = $"{x.value * x.multiplier:0.00} C";
        //    playersBet[playersBetList.IndexOf(x)].multply.text = $"x {x.multiplier:0.00}";
        //    playersBet[playersBetList.IndexOf(x)].anim.Play("BetWin");
        //    //playersBet[playersBetList.IndexOf(x)].GetComponent<Image>().color = new Color(0,1,0,0.3f);
        //});
    }

    public void InstantiateBox()
    {
        if (logs)
        {
            Debug.Log($"InstantiateBox");
        }

        System.Random r = new System.Random();
        int g = boxT.Count;
        boxT.Add(new BoxTank { currentBox = boxOBJ[g].transform, boxOpening = false, bonus = bonusList[r.Next(0, bonusList.Count)] });
        boxOBJ[g].SetActive(true);
        boxOBJ[g].GetComponent<Animator>().Play("Inicial");
        boxOBJ[g].GetComponent<Animator>().SetBool("open", false);
        boxOBJ[g].GetComponent<Animator>().SetBool("kabum", false);
        boxOBJ[g].transform.position = (screen == 0) ? initBox : initBoxMobile;
        boxOBJ[g].transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = (boxT[boxT.Count - 1].bonus == 0) ? "BOMB" : ($"x {boxT[boxT.Count - 1].bonus}");
    }

    public void InstantiateBox(float bonuss)
    {
        if (logs)
        {
            Debug.Log($"InstantiateBox: {bonuss}");
        }

        System.Random r = new System.Random();
        int g = boxT.Count;
        boxT.Add(new BoxTank { currentBox = boxOBJ[g].transform, boxOpening = false, bonus = bonuss });
        boxOBJ[g].SetActive(true);
        boxOBJ[g].GetComponent<Animator>().Play("Inicial");
        boxOBJ[g].GetComponent<Animator>().SetBool("open", false);
        boxOBJ[g].GetComponent<Animator>().SetBool("kabum", false);
        boxOBJ[g].transform.position = (screen == 0) ? initBox : initBoxMobile;
        boxOBJ[g].transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = (bonuss == 0) ? "BOMB" : ($"x {bonuss:0.00}");
    }

    public IEnumerator Open(Transform box)
    {
        if (logs)
        {
            Debug.Log($"Open");
        }

        Animator b = box.GetComponent<Animator>();
        int id = boxT.FindIndex(b => b.currentBox == box);
        b.SetBool((boxT[id].bonus == 0) ? "kabum" : "open", true);
        b.SetBool((boxT[id].bonus == 0) ? "open" : "kabum", false);
        yield return new WaitForSeconds((boxT[id].bonus != 0) ? 0.6f : 0.4f);
        //Debug.Log(boxT[id].bonus != 0 ? $"Bonus :x {boxT[id].bonus:0.00}":"Box Explosiva");
        box.gameObject.GetComponent<Animator>().Play("Inicial");
        box.gameObject.transform.position = (screen == 0) ? initBox : initBoxMobile;
        box.gameObject.SetActive(false);
        if (boxT.Count > id)
        {
            WebClient.Instance.AddBonus(boxT[id].bonus);
            bonusTotal += boxT[id].bonus;
            boxT.RemoveAt(id);
        }
    }

    public void ResetBonus()
    {
        if (logs)
        {
            Debug.Log($"ResetBonus");
        }

        boxOBJ.ForEach(x => x.SetActive(false));
        boxT.Clear();
    }

    public void PlayMensagen(string msg)
    {
        mensagen.text = msg;
        mensagenMobile.text = msg;
        animMensagen.Play("PopUp");
        animMensagenMobile.Play("PopUp");
    }


    public void SelectTeclado(int tecladoSelect)
    {
        if (logs)
        {
            Debug.Log($"SelectTeclado: {((tecladoSelect == 0) ? " Bet " : "AutoStop")}");
        }

        tecladoMode = (tecladoSelect == 0) ? false : true;
        teclado.SetActive(true);
        tecladoButtons[10].interactable = tecladoSelect == 1;
        tecladoText.text = (tecladoSelect == 0) ? ($"{tecladoTextValue}") : ($"x {tecladoTextValue:0.00}");
    }

    public void SelectTecladoNumber(int number) // 10 = ponto
    {
        if (logs)
        {
            Debug.Log($"SelectTecladoNumber: {number}");
        }

        tecladoTextValue = (number == 10) ? (tecladoTextValue + ".") : (tecladoTextValue + number);
        tecladoText.text = tecladoMode ? ($"x {tecladoTextValue:0.00}") : ($"{tecladoTextValue}");
    }

    public void SelectTecladoBackspace()
    {
        if (logs)
        {
            Debug.Log($"SelectTecladoBackspace");
        }

        if (tecladoTextValue.Length > 0)
        {
            tecladoTextValue = tecladoTextValue.Substring(0, tecladoTextValue.Length - 1);
        }

        tecladoText.text = tecladoMode ? ($"x {tecladoTextValue:0.00}") : ($"{tecladoTextValue}");
    }

    public void SelectTecladoEnter()
    {
        float v = float.Parse(tecladoTextValue.Replace(".", ","));
        if (logs)
        {
            Debug.Log($"SelectTecladoEnter");
        }

        if (!tecladoMode)
        {
            ClientExemple.Instance.SetBetValor(int.Parse(tecladoTextValue));
        }
        else { SetStopText(v); }
        tecladoTextValue = string.Empty;
        teclado.SetActive(false);
    }

    public void SetRank(Line[] rankMult, Line[] rankCashh)
    {
        List<BetPlayers> mult = new List<BetPlayers>();
        List<BetPlayers> cash = new List<BetPlayers>();
        foreach (Line item in rankMult)
        {
            mult.Add(new BetPlayers { name = item.name, value = item.bet, multiplier = item.multi });
        }
        foreach (Line item in rankCashh)
        {
            cash.Add(new BetPlayers { name = item.name, value = item.bet, multiplier = item.multi });
        }
        mult.Sort((x, y) => y.multiplier.CompareTo(x.multiplier));
        cash.Sort((x, y) => (y.value * y.multiplier).CompareTo(x.value * x.multiplier));
        rankMultiplier = mult;
        rankCash = cash;
        if (rankMultiplier.Count >= rankSlotsMultiplier.Count)
        {
            rankSlotsMultiplier.ForEach(x => x.Set(mult[rankSlotsMultiplier.IndexOf(x)], true));
            rankSlotsMultiplierMobile.ForEach(x => x.Set(mult[rankSlotsMultiplierMobile.IndexOf(x)], true));
        }
        else
        {
            rankMultiplier.ForEach(x => rankSlotsMultiplier[rankMultiplier.IndexOf(x)].Set(x, true));
            rankMultiplier.ForEach(x => rankSlotsMultiplierMobile[rankMultiplier.IndexOf(x)].Set(x, true));
        }
        if (cash.Count >= rankSlotsCash.Count)
        {
            rankSlotsCash.ForEach(x => x.Set(cash[rankSlotsCash.IndexOf(x)], true));
            rankSlotsCashMobile.ForEach(x => x.Set(cash[rankSlotsCashMobile.IndexOf(x)], true));
        }
        else
        {
            rankCash.ForEach(x => rankSlotsCash[rankCash.IndexOf(x)].Set(x, true));
            rankCash.ForEach(x => rankSlotsCashMobile[rankCash.IndexOf(x)].Set(x, true));
        }
    }

    bool rankShow = false;

    public void ShowRank()
    {
        if (playerShow)
        {
            ShowPlayers();
        }
        if (rankShow)
        {
            rankCanvas.GetComponent<Animator>().Play("Hide");
            rankCanvasMobile.GetComponent<Animator>().Play("Hide");
            rankShow = false;
        }
        else
        {
            rankCanvas.GetComponent<Animator>().Play("Show");
            rankCanvasMobile.GetComponent<Animator>().Play("Show");
            rankShow = true;
        }
        rankMultiplier.Sort((x, y) => y.multiplier.CompareTo(x.multiplier));
        rankCash.Sort((x, y) => (y.value * y.multiplier).CompareTo(x.value * x.multiplier));
        if (rankMultiplier.Count >= rankSlotsMultiplier.Count)
        {
            rankSlotsMultiplier.ForEach(x => x.Set(rankMultiplier[rankSlotsMultiplier.IndexOf(x)], true));
        }
        else
        {
            rankMultiplier.ForEach(x => rankSlotsMultiplier[rankMultiplier.IndexOf(x)].Set(x, true));
        }
        if (rankCash.Count >= rankSlotsCash.Count)
        {
            rankSlotsCash.ForEach(x => x.Set(rankCash[rankSlotsCash.IndexOf(x)], true));
        }
        else
        {
            rankCash.ForEach(x => rankSlotsCash[rankCash.IndexOf(x)].Set(x, true));
        }

    }

    bool playerShow = false;
    public void ShowPlayers()
    {
        if (rankShow)
        {
            ShowRank();
        }
        if (playerShow)
        {
            playerPanel.GetComponent<Animator>().Play("Hide");
            playerPanelMobile.GetComponent<Animator>().Play("Hide");
            playerShow = false;
        }
        else
        {
            playerPanel.GetComponent<Animator>().Play("Show");
            playerPanelMobile.GetComponent<Animator>().Play("Show");
            playerShow = true;
        }
    }
}


