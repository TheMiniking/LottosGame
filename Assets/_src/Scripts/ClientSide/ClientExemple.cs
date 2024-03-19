using BV;
using GameSpawner;
using System.Collections;
using TMPro;
using UnityEngine;

public class ClientExemple : WebClientBase
{
    public static ClientExemple Instance;
    string url;
    [SerializeField] string urlTest;
    [SerializeField] string urlDev;
    [SerializeField] string token;
    [SerializeField] TextMeshProUGUI value;
    bool isJoin = false;
    [SerializeField] string playerName;
    [SerializeField][Range(1, 100)] int betValor;
    [SerializeField] bool debug = true;

    float paralaxPosition = 0;
    void Awake()
    {
        Instance = this;
    }
    protected override void Start()
    {
        base.Start();
        CanvasManager.Instance.ShowLoading("Connecting...");
        RegisterHandler<GameLoginResponse>(GameLoginResponse, (ushort)ReceiveMsgIdc.GameLoginResponse);
        RegisterHandler<BalanceResponse>(BalanceResponse, (ushort)ReceiveMsgIdc.BalanceResponse);
        RegisterHandler<PlayResponse<MathStatus>>(PlayResponse, (ushort)ReceiveMsgIdc.PlayResponse);
        RegisterHandler<NextBetResponse>(NextBetResponse, (ushort)ReceiveMsgIdc.NextBetResponse);
        RegisterHandler<ErrorResponse>(ErrorResponse, (ushort)ReceiveMsgIdc.ErrorResponse);
        RegisterHandler<BetPlayers>(BetPlayers, (ushort)ReceiveMsgIdc.BetPlayers);
        RegisterHandler<Ranking>(RankResponse, (ushort)ReceiveMsgIdc.Ranking);
        RegisterHandler<LastMulti>(LastMultiResponse, (ushort)ReceiveMsgIdc.LastMulti);
#if UNITY_WEBGL && !UNITY_EDITOR
        token = GetTokenID();
#endif
        CreateConnection((BuildType == BuildType.Dev) ? urlDev : urlTest, token);
        TryConnect();
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        //#if UNITY_WEBGL && !UNITY_EDITOR
        //        token = GetTokenID();
        //#endif
        //        SendMsg(new GameLogin { token = token });
    }
    string GetTokenID()
    {
        int pm = Application.absoluteURL.IndexOf("?token=");
        if (pm != -1)
        {
            return Application.absoluteURL.Split("?token=")[1];
        }
        return "0";
    }
    protected override void OnClose(int closeCode)
    {
        base.OnClose(closeCode);
        CanvasManager.Instance.ShowLoading("Connecting...");
    }
    void GameLoginResponse(GameLoginResponse msg)
    {
        CanvasManager.Instance.HideLoading();
        if (debug)
        {
            Debug.Log("LogResponse:" + msg.code);
        }

        playerName = msg.name;
    }

    void BalanceResponse(BalanceResponse msg)
    {
        if (debug)
        {
            Debug.Log("BalanceResponse:" + msg.balance);
        }

        CanvasManager.Instance.SetWalletBalance(msg.balance);
    }

    void PlayResponse(PlayResponse<MathStatus> msg)
    {
        Debug.Log($"PlayResponse: id: {msg.data.id} value : {msg.data.value} ");
        Debug.Log(($"GamaMenager: ") + (WebClient.Instance == null));
        if (msg.data.id == 0)// Start Timer
        {
            CanvasManager.Instance.ResetPlayersBet();
            GameManager.Instance.NewMatchInit();
            StartCoroutine(DisplayTimer(msg.data.value));
            CanvasManager.Instance.SetBetActive();

        }
        else
        if (msg.data.id == 1)// Start Round 
        {
            GameManager.Instance.NewMatchStart();
            StopAllCoroutines();
            CanvasManager.Instance.SetBetDesactive();
            WebClient.Instance.StartRun(new StartRun { });
            StartCoroutine(DisplayMulti(msg.data.value));
            AudioManager.Instance.StopResumeSFX(false);
        }
        else if (msg.data.id == 2) // End Round Crash
        {
            GameManager.Instance.EndMatchStart();
            StopAllCoroutines();
            WebClient.Instance.Crash(new Crash { multply = msg.data.value });
            Debug.Log("Crash " + msg.data.value);
            isJoin = false;
            CanvasManager.Instance.SetMultiplicador(msg.data.value);
            AudioManager.Instance.StopResumeSFX(true);
            AudioManager.Instance.PlayOneShot(2);
        }
        else if (msg.data.id == 3) // Join Round
        {
            CanvasManager.Instance.SetBetDesactive();
            isJoin = true;
        }
        else if (msg.data.id == 4) // Bet CrashOut
        {
            isJoin = false;
            CanvasManager.Instance.SetBetDesactive();
            CanvasManager.Instance.PlayMensagen("Finish Bet");

        }
    }

    public void SetBetValor(int valor)
    {
        betValor = valor;
        betValor = (betValor > 100) ? 100 : ((betValor < 1) ? 1 : betValor);
        SendMsg((ushort)SendMsgIdc.NextBet, new NextBet { bet = (byte)betValor });
    }

    public void AddBetValor(int valor)
    {
        betValor += valor;
        betValor = (betValor > 100) ? 100 : ((betValor < 1) ? 1 : betValor);
        SendMsg((ushort)SendMsgIdc.NextBet, new NextBet { bet = (byte)betValor });
    }

    public void NextBet(bool up)
    {
        betValor = up ? (betValor + 1) : (betValor - 1);
        betValor = (betValor > 100) ? 100 : ((betValor < 1) ? 1 : betValor);
        SendMsg((ushort)SendMsgIdc.NextBet, new NextBet { bet = (byte)betValor });
    }

    void NextBetResponse(NextBetResponse msg)
    {
        GameScreen.instance.SetBetText(msg.money);
        betValor = (int)msg.money;
    }


    public void SendBet()
    {
        SendMsg((ushort)SendMsgIdc.PlayRequest, new PlayRequest());
    }


    public void BetPlayers(BetPlayers msg)
    {
        if (debug)
        {
            Debug.Log((msg.multiplier == 0) ? ($"[Client] Jogador{msg.name} fez aposta pagando{msg.value}") : ($"[Cliente] O jogador {msg.name} Retirou {msg.multiplier}"));
        }

        if (msg.multiplier == 0)
        {
            CanvasManager.Instance.SetPlayersBet(msg);
            if (msg.name == playerName)
            {
                GameScreen.instance.totalCashBet += (float)msg.value;
            }

            GameScreen.instance.playerInBet += 1;
        }
        else
        {
            CanvasManager.Instance.SetPlayersWin(msg);
            if (msg.name == playerName)
            {
                GameScreen.instance.totalCashOut += (float)(msg.value * msg.multiplier);
                GameScreen.instance.PlayMensagen($"You Win $ {msg.value * msg.multiplier:#,0.00}");
            }

            GameScreen.instance.playerInBetWinner += 1;
        }
    }

    void ErrorResponse(ErrorResponse msg)
    {
        Debug.Log("ErrorResponse:" + msg.error);

    }
    void RankResponse(Ranking ranking)
    {
        Debug.Log("RankResponse mult:" + JsonUtility.ToJson(ranking));
        Line[] jsonMult = ranking.rankMulti;
        Line[] jsonCash = ranking.rankValue;
        GameScreen.instance.SetRank(jsonMult, jsonCash);
    }

    void LastMultiResponse(LastMulti lastMulti)
    {
        Debug.Log("LastMultiResponse:" + lastMulti.multis.Length);
        for (int i = 0; i < lastMulti.multis.Length; i++)

        {
            CanvasManager.Instance.SetLastPlays(lastMulti.multis[i]);
        }
    }

    IEnumerator DisplayTimer(float time)
    {
        while (time > 0)
        {
            time--;
            CanvasManager.Instance.SetTimer((int)time);
            yield return new WaitForSeconds(1);
        }

    }

    //IEnumerator DisplayMulti(float multiSum)
    //{
    //    float multiplier = 1;
    //    float delay = 0.1f;
    //    float delayServer = 0.5f;
    //    float adjustmentFactor = delayServer / delay;
    //    while (true)
    //    {
    //        multiSum += multiSum * (.001f * delay);
    //        multiplier += multiSum;
    //        CanvasManager.Instance.SetMultiplicador(multiplier);
    //        if (isJoin)
    //        {
    //            CanvasManager.Instance.SetBetButtonStop(multiplier);

    //            GameManager.Instance.MatchMultiplier(multiplier);
    //        }
    //        //value.text = multiplier.ToString("f2") + "x";
    //        yield return new WaitForSeconds(delay);
    //    }
    //}
    public IEnumerator DisplayMulti(float multiSum)
    {
        Debug.Log("DisplayMulti " + multiSum);
        float multiplier = 1;
        float timer = Time.time - multiSum;
        while (true)
        {
            multiplier = MultiplierCalculator(Time.time - timer);
            if (isJoin)
            {
                CanvasManager.Instance.SetBetButtonStop(multiplier);
                GameManager.Instance.MatchMultiplier(multiplier);// Auto CashOut
            }
            CanvasManager.Instance.SetMultiplicador(multiplier);
            //value.text = multiplier.ToString("f2") + "x";
            yield return new WaitForSeconds(0.03f);
        }
    }
    float MultiplierCalculator(float tempoDecorrido)
    {
        //Debug.Log("MultiplierCalculator " + tempoDecorrido);
        return 1.01f + (2 * Mathf.Pow(tempoDecorrido / 10, 1.5f));
    }
}
