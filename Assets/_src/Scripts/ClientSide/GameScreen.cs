using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BV;
using System.Text.RegularExpressions;

[Serializable]
public class GameScreen : BaseScreen
{
    public static GameScreen instance;
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
    [SerializeField] TMP_Text txtStopAnBet, txtBetVal;
    [SerializeField] TMP_InputField stopVal, betInput;
    [SerializeField] List<Button> betButtons, autoStop = new();
    [SerializeField] List<GameObject> lastResultObj = new();
    [SerializeField] List<BetPlayersHud> playersBet = new();
    [SerializeField] List<float> lastResult,last50Result = new();
    [SerializeField] List<Sprite> lastResultSprite = new();
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

    //adicionado por robson
    [SerializeField] Toggle autoCashOutToggle;
    [SerializeField] Toggle autoPlayToggle;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        fundo.SetInt("_UseScriptTime", 1);

        autoCashOutToggle.onValueChanged.AddListener(x =>
        {
            GameManager.Instance.AutoCashout(x);
        });
        autoPlayToggle.onValueChanged.AddListener(x =>
        {
            GameManager.Instance.AutoStop(x);
        });
        autoStop[1].onClick.AddListener(() =>
        {
            var v = Regex.Replace(stopVal.text.Replace(".", ","), @"[^0-9,]", string.Empty);
            float.TryParse(v, out float s);
            Debug.LogWarning(v+" less "+s);
            s = Mathf.Max(1f, s - 1.0f);
            stopVal.SetTextWithoutNotify($"x {s.ToString("f2")}");
            GameManager.Instance.SetAutoStop(s);
        });
        autoStop[0].onClick.AddListener(() =>
        {
            var v = Regex.Replace(stopVal.text.Replace(".", ","), @"[^0-9,]", string.Empty);
            float.TryParse(v, out float s);
            Debug.LogWarning(v + " More " + s);
            s = Mathf.Min(1000f, s + 1.0f);
            stopVal.SetTextWithoutNotify($"x {s.ToString("f2")}");
            GameManager.Instance.SetAutoStop(s);
        });
        stopVal.onValueChanged.AddListener(x =>
        {
            string resultado = Regex.Replace(x.Replace(",", "."), "[^0-9.]", "");
            stopVal.SetTextWithoutNotify(resultado);
        });
        stopVal.onEndEdit.AddListener(x =>
        {
            string resultado = Regex.Replace(x.Replace(".", ","), "[^0-9,]", "");
            float n = 0;
            float.TryParse(resultado, out n);
            n = Mathf.Clamp(n, 1, 1000);
            stopVal.SetTextWithoutNotify($"x {n.ToString("f2")}");
            GameManager.Instance.SetAutoStop(n);
        });
        betInput.onEndEdit.AddListener(x =>
        {
            int n = 0;
            int.TryParse(x, out n);
            n = Mathf.Clamp(n, 1, 100);
            betInput.SetTextWithoutNotify(n.ToString());
        });
        ResetBetPlayers();
    }
    IEnumerator MoveCarret()
    {
        yield return new WaitForEndOfFrame();
        stopVal.MoveTextEnd(false);
    }
    private void FixedUpdate()
    {
        fundoRealtimeAtualPosition = fundoOnMove ? fundoRealtimeAtualPosition + fundoRealtimeVelocity : fundoRealtimeAtualPosition;
        fundo.SetFloat("_RealTimeUpdate", fundoRealtimeAtualPosition);
        if (boxT.Count > 0) boxT.ForEach(x =>
        {
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
    public void SetWalletBalance(double balance)
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
        txtStopAnBet.text = $"Bet";
    }

    public void DesactiveBet()
    {
        if (logs) Debug.Log($"DesactiveBet");
        stopAnBet.interactable = false;
        txtStopAnBet.text = $"Wait Round";
    }

    public void SetBetButtonText(float mult)
    {
        if (logs) Debug.Log($"SetBetButtonText: {mult}");
        stopAnBet.interactable = true;
        txtStopAnBet.text = $"Stop x{mult:0.00}";
    }

    public void SetBetText(double betV)
    {
        if (logs) Debug.Log($"SetBetText: {betV}");
        this.bet = (float)betV;
        WebClient.Instance.SetBetValor((float)betV);
        betInput.text = $"{betV}";
    }

    public void SetStopText(float stopV)
    {
        if (logs) Debug.Log($"SetStopText: {stopV}");
        this.stop = stopV;
        //txtStopVal.text = $"{stopV:0.00}";
    }

    public void SetLastResult(float result)
    {
        if (logs) Debug.Log($"SetLastResult: {result}");
        lastResult.Add(result);
        last50Result.Add(result);
        if (lastResult.Count > lastResultObj.Count) lastResult.RemoveAt(0);
        if (last50Result.Count > 50) last50Result.RemoveAt(0);
        lastResultObj.ForEach(x => x.SetActive(false));
        for (int i = 0; i < lastResult.Count; i++)
        {
            lastResultObj[i].SetActive(true);
            lastResultObj[i].transform.GetChild(0).GetComponent<TMP_Text>().text = $"{lastResult[i]:0.00}";
            lastResultObj[i].GetComponent<Image>().sprite = lastResult[i] < 2f ? lastResultSprite[0] : lastResult[i] < 5 ? lastResultSprite[1] : lastResultSprite[2];
        }
    }

    public void ResetBetPlayers()
    {
        if (logs) Debug.Log($"ResetBetPlayers");
        playersBet.ForEach(x => x.Clear());
        ResetBonus();
        bonusTotal = 0;
    }

    public void SetBetPlayersList(BetPlayers bet)
    {
        if (logs) Debug.Log($"SetBetPlayersList: {bet.name}");
        var index = playersBet.FindIndex(b => b.name.text == bet.name);
        if (index == -1)
            index = playersBet.Count - 1;
        var p = playersBet[index];
        p.Set(bet);
        p.transform.SetAsFirstSibling();
        playersBet.RemoveAt(index);
        playersBet.Insert(0, p);

    }

    public void SetBetPlayersWin(BetPlayers bet)
    {
        if (logs) Debug.Log($"SetBetPlayersWin: {bet.name} bet {bet.value} x {bet.multiplier}");
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
        if (logs) Debug.Log($"InstantiateBox");
        var r = new System.Random();
        var g = boxT.Count;
        boxT.Add(new BoxTank { currentBox = boxOBJ[g].transform, boxOpening = false, bonus = bonusList[r.Next(0, bonusList.Count)] });
        boxOBJ[g].SetActive(true);
        boxOBJ[g].GetComponent<Animator>().Play("Inicial");
        boxOBJ[g].GetComponent<Animator>().SetBool("open", false);
        boxOBJ[g].GetComponent<Animator>().SetBool("kabum", false);
        boxOBJ[g].transform.position = initBox;
        boxOBJ[g].transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = boxT[boxT.Count - 1].bonus == 0 ? "BOMB" : $"x {boxT[boxT.Count - 1].bonus}";
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
        b.SetBool(boxT[id].bonus == 0 ? "kabum" : "open", true);
        b.SetBool(boxT[id].bonus == 0 ? "open" : "kabum", false);
        yield return new WaitForSeconds(boxT[id].bonus != 0 ? 0.6f : 0.4f);
        //Debug.Log(boxT[id].bonus != 0 ? $"Bonus :x {boxT[id].bonus:0.00}":"Box Explosiva");
        box.gameObject.GetComponent<Animator>().Play("Inicial");
        box.gameObject.transform.position = initBox;
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


