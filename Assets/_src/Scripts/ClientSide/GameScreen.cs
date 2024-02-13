using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BV;


[Serializable]
public class GameScreen : BaseScreen
{
    public bool logs = false;
    [SerializeField] GameManager gameManager;
    [SerializeField] Player tank;
    [SerializeField] Material fundo;
    [SerializeField] float fundoRealtimeVelocity = 0.02f;
    [SerializeField] float fundoRealtimeAtualPosition;
    [SerializeField] bool fundoOnMove = false;

    [SerializeField] float bet;
    [SerializeField] float stop;

    [SerializeField] TMP_Text txtWalletBalance, txtWalletNickname;
    [SerializeField] TMP_Text txtTimerMult, txtTimerMensagem, txtBonusTotal, txtTotalCashOut, txtTotalCashBet;
    [SerializeField] Button stopAnBet;
    [SerializeField] TMP_Text txtStopAnBet, txtStopVal, txtBetVal;
    [SerializeField] List<Button> betButtons, autoStop = new();
    [SerializeField] List<GameObject> lastResultObj = new();
    [SerializeField] List<BetPlayersHud> playersBet = new();
    [SerializeField] List<float> lastResult = new();
    [SerializeField] List<BetPlayers> playersBetList = new();

    [SerializeField] GameObject boxPrefab;
    [SerializeField] Vector3 initBox = new(0, 0, 0);
    [SerializeField] List<float> bonusList = new();
    [SerializeField] List<GameObject> boxOBJ = new();
    [SerializeField] public List<BoxTank> boxT = new();
    [SerializeField] Vector3 direcaoBonus = new();
    [SerializeField] float velocityBonus = 0.1f;
    [SerializeField] Transform fieldBonus;

    [SerializeField] float bonusTotal;
    [SerializeField] TMP_Text mensagen;
    [SerializeField] Animator animMensagen;

    [SerializeField] public float totalCashOut = 0;
    [SerializeField] public float totalCashBet = 0;

    private void Start()
    {
        fundo.SetInt("_UseScriptTime", 1);
        betButtons.ForEach(x => x.onClick.RemoveAllListeners());
        autoStop.ForEach(x => x.onClick.RemoveAllListeners());
        betButtons[0].onClick.AddListener(() => SetBetText(gameManager.UpDownBetAmount(true)));
        betButtons[1].onClick.AddListener(() => SetBetText(gameManager.UpDownBetAmount(false)));
        autoStop[0].onClick.AddListener(() => SetStopText(gameManager.UpDownAutoStop(true)));
        autoStop[1].onClick.AddListener(() => SetStopText(gameManager.UpDownAutoStop(false)));
    }

    private void FixedUpdate()
    {
        fundoRealtimeAtualPosition = fundoOnMove ? fundoRealtimeAtualPosition + fundoRealtimeVelocity : fundoRealtimeAtualPosition;
        fundo.SetFloat("_RealTimeUpdate", fundoRealtimeAtualPosition);
        if (boxT.Count > 0) boxT.ForEach(x => {
            x.currentBox.gameObject.transform.position += ((velocityBonus * direcaoBonus) * (fundoRealtimeVelocity * (fundoOnMove ? 1 : 0)));
            if (x.currentBox.gameObject.transform.position.x - 100 <= tank.gameObject.transform.position.x - 50) { StartCoroutine(Open(x.currentBox)); }
        });
        txtBonusTotal.text = $"x {bonusTotal:0.00}";
        txtTotalCashOut.text = $"{totalCashOut:0.00}";
        txtTotalCashBet.text = $"{totalCashBet:0.00}";
    }

    public void SetWalletNickname(string nickname)
    {
        if (logs) Debug.Log($"SetWalletNickname: {nickname}");
        txtWalletNickname.text = nickname;
    }
    public void SetWalletBalance(float balance)
    {
        if (logs) Debug.Log($"SetWalletBalance: {balance}");
        txtWalletBalance.text = $"{balance:0.00}";
    }
    public void SetTimer(int time)
    {
        if (logs) Debug.Log($"SetTimer: {time}");
        txtTimerMult.text = $"{time:00:00}";
        txtTimerMensagem.text = "Next Round in:";

    }
    public void SetMultplicador(float mult)
    {
        if (logs) Debug.Log($"SetMultplicador: {mult}");
        txtTimerMult.text = $"x {mult:00.00}";
        txtTimerMensagem.text = "Stop in:";
    }
    public void SetTimerMensagem(string time)
    {
        if (logs) Debug.Log($"SetTimerMensagem: {time}");
        txtTimerMensagem.text = time;
    }
    public void SetBonusTotal(float bonus)
    {
        if (logs) Debug.Log($"SetBonusTotal: {bonus}");
        txtBonusTotal.text = $"x {bonus:0.00}";
    }
    public void SetTankState(string state)
    {
        if (logs) Debug.Log($"SetTankState: {state}");
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

    public void AddVelocityParalax(float value)
    {
        if (logs) Debug.Log($"AddVelocityParalax: {value}");
        fundoRealtimeVelocity = fundoRealtimeVelocity == 0.2f ? fundoRealtimeVelocity : fundoRealtimeVelocity + value;
    }

    public void ResetVelocityParalax()
    {
        if (logs) Debug.Log($"ResetVelocityParalax: {fundoRealtimeVelocity}");
        fundoRealtimeVelocity = 0.07f;
    }

    public void ActiveBet()
    {
        if (logs) Debug.Log($"ActiveBet");
        stopAnBet.interactable = true;
    }

    public void DesactiveBet()
    {
        if (logs) Debug.Log($"DesactiveBet");
        stopAnBet.interactable = false;
    }

    public void SetBetButtonText(string txt)
    {
        if (logs) Debug.Log($"SetBetButtonText: {txt}");
        txtStopAnBet.text = txt;
    }

    public void SetBetText(float betV)
    {
        if (logs) Debug.Log($"SetBetText: {betV}");
        this.bet = betV;
        WebClient.Instance.SetBetValor(betV);
        txtBetVal.text = $"{betV:0.00}";
    }

    public void SetStopText(float stopV)
    {
        if (logs) Debug.Log($"SetStopText: {stopV}");
        this.stop = stopV;
        txtStopVal.text = $"{stopV:0.00}";
    }

    public void SetLastResult(float result)
    {
        if (logs) Debug.Log($"SetLastResult: {result}");
        lastResult.Add(result);
        if(lastResult.Count > 9 ) lastResult.RemoveAt(0);
        lastResultObj.ForEach(x => x.SetActive(false));
        for (int i = 0; i < lastResult.Count; i++)
        {
            lastResultObj[i].SetActive(true);
            lastResultObj[i].transform.GetChild(0).GetComponent<TMP_Text>().text = $"{lastResult[i]:0.00}";
            lastResultObj[i].GetComponent<Image>().color = lastResult[i] < 2f ? Color.red : lastResult[i] < 5 ? Color.yellow : Color.green;
        }
    }

    public void ResetBetPlayers()
    {
        if (logs) Debug.Log($"ResetBetPlayers");
        playersBetList.Clear();
        playersBet.ForEach(x => x.gameObject.SetActive(false));
        ResetBonus();
        bonusTotal = 0;
    }

    public void SetBetPlayersList(BetPlayers bet)
    {
        if (logs) Debug.Log($"SetBetPlayersList: {bet.msg}");
        playersBetList.Add(bet);
        if(playersBetList.Count > playersBet.Count) playersBetList.RemoveAt(0);
        playersBetList.ForEach(x => {
            playersBet[playersBetList.IndexOf(x)].gameObject.SetActive(true);
            playersBet[playersBetList.IndexOf(x)].name.text = x.msg;
            playersBet[playersBetList.IndexOf(x)].bet.text = $"{x.valor:0.00} C";
            playersBet[playersBetList.IndexOf(x)].credits.text = $" --.-- C";
            playersBet[playersBetList.IndexOf(x)].multply.text = $"x --.--";
            playersBet[playersBetList.IndexOf(x)].anim.Play("Normal");
            //playersBet[playersBetList.IndexOf(x)].GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);
        });
    }

    public void SetBetPlayersWin(BetPlayers bet)
    {
        if (logs) Debug.Log($"SetBetPlayersWin: {bet.msg}");
        //ResetBetPlayers();
        playersBetList.Add(bet);
        if (playersBetList.Count > playersBet.Count) playersBetList.RemoveAt(0);
        playersBetList.ForEach(x => {
            playersBet[playersBetList.IndexOf(x)].gameObject.SetActive(true);
            playersBet[playersBetList.IndexOf(x)].name.text = x.msg;
            playersBet[playersBetList.IndexOf(x)].bet.text = $"{x.valor:0.00} C";
            playersBet[playersBetList.IndexOf(x)].credits.text = $"{x.valor * x.multply:0.00} C";
            playersBet[playersBetList.IndexOf(x)].multply.text = $"x {x.multply:0.00}";
            playersBet[playersBetList.IndexOf(x)].anim.Play("BetWin");
            //playersBet[playersBetList.IndexOf(x)].GetComponent<Image>().color = new Color(0,1,0,0.3f);
        });
    }

    public void InstantiateBox()
    {
        if (logs) Debug.Log($"InstantiateBox");
        var r = new System.Random();
        var g = boxT.Count;
        boxT.Add(new BoxTank { currentBox = boxOBJ[g].transform , boxOpening = false, bonus = bonusList[r.Next(0, bonusList.Count)] });
        boxOBJ[g].SetActive(true);
        boxOBJ[g].GetComponent<Animator>().Play("Inicial");
        boxOBJ[g].GetComponent<Animator>().SetBool("open", false);
        boxOBJ[g].GetComponent<Animator>().SetBool("kabum", false);
        boxOBJ[g].transform.position = initBox;
        boxOBJ[g].transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = boxT[boxT.Count-1].bonus == 0 ? "BOMB" : $"x {boxT[boxT.Count - 1].bonus}";
    }

    public void InstantiateBox(float bonuss)
    {
        if (logs) Debug.Log($"InstantiateBox: {bonuss}");
        var r = new System.Random();
        var g = boxT.Count;
        boxT.Add(new BoxTank { currentBox = boxOBJ[g].transform, boxOpening = false, bonus = bonuss });
        boxOBJ[g].SetActive(true);
        boxOBJ[g].GetComponent<Animator>().Play("Inicial");
        boxOBJ[g].GetComponent<Animator>().SetBool("open", false);
        boxOBJ[g].GetComponent<Animator>().SetBool("kabum", false);
        boxOBJ[g].transform.position = initBox;
        boxOBJ[g].transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = bonuss == 0 ? "BOMB" : $"x {bonuss:0.00}";
    }

    public IEnumerator Open(Transform box)
    {
        if (logs) Debug.Log($"Open");
        var b = box.GetComponent<Animator>();
        int id = boxT.FindIndex(b => b.currentBox == box);
        b.SetBool(boxT[id].bonus == 0?"kabum":"open", true);
        b.SetBool(boxT[id].bonus == 0 ?  "open" :"kabum", false);
        yield return new WaitForSeconds(boxT[id].bonus != 0 ? 0.6f : 0.4f);
        //Debug.Log(boxT[id].bonus != 0 ? $"Bonus :x {boxT[id].bonus:0.00}":"Box Explosiva");
        box.gameObject.GetComponent<Animator>().Play("Inicial");
        box.gameObject.transform.position = initBox;
        box.gameObject.SetActive(false);
        if(boxT.Count > id)
        {
        WebClient.Instance.AddBonus(boxT[id].bonus);
        bonusTotal += boxT[id].bonus;
        boxT.RemoveAt(id);
        }
    }

    public void ResetBonus()
    {
        if (logs) Debug.Log($"ResetBonus");
        boxOBJ.ForEach(x => x.SetActive(false));
        boxT.Clear();
    }

    public void PlayMensagen(string msg)
    {
        mensagen.text = msg;
        animMensagen.Play("PopUp");
    }

}


