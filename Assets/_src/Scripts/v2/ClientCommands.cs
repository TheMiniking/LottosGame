using GameSpawner;
using Serializer;
using System;
using System.Collections;
using UnityEngine;

public class ClientCommands : WebClientBase
{
    public static ClientCommands Instance;
    string url;
    [SerializeField] string urlTest;
    [SerializeField] string urlDev;
    [SerializeField] string token;
    [SerializeField] bool debug = true;
    public string playerName;
    public bool isRunning, onConnect=false;
    Conection data ;
    public bool onTutorial = false;

    void Awake()
    {
        Instance = this;
    }

    protected override void Start()
    {
        base.Start();
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
        data = new Conection() { data =new byte[] { 255, 255 } };
        onConnect = true;
        TryConnect();
    }


    protected override void OnOpen()
    {
        CanvasManager.Instance.ShowLoadingPanel(false);
        base.OnOpen();
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
        CanvasManager.Instance.ShowLoadingPanel(true);
        base.OnClose(closeCode);
    }

    double balance = 0f;
    int bet = 0;

    public void OnTutorial(bool value)
    {
        onTutorial = value;
        if (!onTutorial)
        {
            CanvasManager.Instance.SetBalanceTxt(balance);
            NextBet(bet);
            CanvasManager.Instance.totalWinAmount = 0f;
        }
    }

    public void GameLoginResponse(GameLoginResponse msg)
    {
        if (debug)
        {
            Debug.Log("LogResponse:" + msg.code);
        }

        playerName = msg.name;
    }

    public void BalanceResponse(BalanceResponse msg)
    {
        if (onTutorial)
        {
            balance = msg.balance;
            return;
        }
        if (debug)
        {
            Debug.Log("BalanceResponse:" + msg.balance);
        }
        CanvasManager.Instance.SetBalanceTxt(msg.balance);
    }

    public void PlayResponse(PlayResponse<MathStatus> msg)
    {
        if (debug)
        {
            Debug.Log($"PlayResponse: id: {msg.data.id} value : {msg.data.value} ");
        }

        if (msg.data.id == 0)// Start Timer
        {
            CanvasManager.Instance.ResetBets();
            GameManager.Instance.NewMatchInit();            // AutoPlay Start
            StartCoroutine(GameManager.Instance.DisplayTimer(msg.data.value));
            CanvasManager.Instance.SetBetButtonBet();
            CanvasManager.Instance.SetMultiplierTextMensage(true);
        }
        else if (msg.data.id == 1)// Start Round 
        {
            GameManager.Instance.NewMathStart();
            StopAllCoroutines();
            CanvasManager.Instance.SetBetButtonCantBet();
            StartRun(new StartRun());
            StartCoroutine(GameManager.Instance.DisplayMulti(msg.data.value));
            StartCoroutine(ConfirmConnection());
            CanvasManager.Instance.SetMultiplierTextMensage(false);
        }
        else if (msg.data.id == 2) // End Round Crash
        {
            GameManager.Instance.EndMatchStart();
            StopAllCoroutines();
            Crash(new Crash { multply = msg.data.value });
            GameManager.Instance.isJoin = false;
            CanvasManager.Instance.SetMultiplierText(msg.data.value);
            StartCoroutine(ConfirmConnection());
        }
        else if (msg.data.id == 3) // Join Round
        {
            CanvasManager.Instance.SetBetButtonCantBet();
            GameManager.Instance.isJoin = true;
        }
        else if (msg.data.id == 4) // Bet CrashOut
        {
            GameManager.Instance.isJoin = false;
            CanvasManager.Instance.SetBetButtonCantBet();
            //CanvasManager.Instance.PlayMessage("Finish Bet");
        }
    }

    void ErrorResponse(ErrorResponse msg)
    {
        if (debug)
        {
            Debug.Log("[Servidor] ErrorResponse:" + msg.error);
        }
    }

    public void BetPlayers(BetPlayers msg)
    {
        if (onTutorial) return;
        if (debug) Debug.Log((msg.multiplier == 0) ? ($"[Client] Jogador {msg.name} fez aposta pagando{msg.value}") : ($"[Cliente] O jogador {msg.name} Retirou {msg.multiplier}"));
        CanvasManager.Instance.SetBetSlot(msg);

    }

    void RankResponse(Ranking ranking)
    {
        if (debug) Debug.Log("RankResponse mult:" + ranking.rankValue.Length + " / cash :" + ranking.rankMulti.Length);
        Line[] jsonMult = ranking.rankMulti;
        Line[] jsonCash = ranking.rankValue;
        CanvasManager.Instance.SetRank(jsonMult, jsonCash);
    }

    void LastMultiResponse(LastMulti lastMulti)
    {
        if (debug) Debug.Log("LastMultiResponse:" + lastMulti.multis);
        foreach (float item in lastMulti.multis)
        {
            CanvasManager.Instance.SetLastPlays(item);
        }
    }

    public void NextBet(int value)
    {
        if (onTutorial) return;
        if (debug) Debug.Log("[Client] Next Bet " + value);
        SendMsg((ushort)SendMsgIdc.NextBet, new NextBet { bet = (byte)value });
    }

    public void NextBetResponse(NextBetResponse msg)
    {
        if (onTutorial)
        {
            bet = (int)msg.money;
            return;
        }
        if (debug)
        {
            Debug.Log($"[Servidor] NextBetResponse: Bet: {msg.money} ");
        }
        GameManager.Instance.bet = (int)msg.money;
        CanvasManager.Instance.SetBetInput((int)msg.money);
    }

    public void SendBet()
    {
        if (onTutorial) return;
        if (debug)
        {
            Debug.Log("[Client] Send Bet");
        }

        SendMsg((ushort)SendMsgIdc.PlayRequest, new PlayRequest());
    }

    public void StartRun(StartRun msg)
    {
        isRunning = true;
        GameManager.Instance.isWalking = true;
        GameManager.Instance.canBet = false;
        CanvasManager.Instance.SetPlayerState("Walking");
        AudioManager.Instance.StopResumeSFX(false);
        GameManager.Instance.fundoOnMove = true;
        if (debug)
        {
            Debug.Log("Start Corrida");
        }
    }

    public void Crash(Crash msg)
    {
        isRunning = false;
        GameManager.Instance.isWalking = false;
        GameManager.Instance.canBet = true;
        GameManager.Instance.ResetVelocityParalax();
        CanvasManager.Instance.SetPlayerState("Lost");
        CanvasManager.Instance.SetLastPlays(msg.multply);
        GameManager.Instance.fundoOnMove = false;
        AudioManager.Instance.StopResumeSFX(true);
        AudioManager.Instance.PlayOneShot(2);
        if (debug)
        {
            Debug.Log("Kabum , Distance x" + msg.multply);
        }
    }
    
    DateTime time = DateTime.Now;
    IEnumerator ConfirmConnection()
    {
        //if (debug) Debug.Log($"[Client] ConfirmConnection : {onConnect}");
        while (true)
        {
            if (time + TimeSpan.FromSeconds(60f) <= DateTime.Now )
            {
                SendMsg(ushort.MaxValue, data);
                time = DateTime.Now;
                if (debug) Debug.Log($"[Client] ConfirmConnection Sent");
                yield return new WaitForSeconds(60f);
            }
            else
            {
                yield return new WaitForSeconds(60f);
            }
            
        }
    }
}

[Serializable]
public class Conection : INetSerializable
{
    public byte[] data;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref data);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(data);
    }
}

[Serializable]
public class BetPlayers : INetSerializable
{
    public string name;
    public double value;
    public float multiplier;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref name);
        reader.Get(ref value);
        reader.Get(ref multiplier);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(name);
        write.Put(value);
        write.Put(multiplier);
    }
}

[Serializable]
public class MathStatus : INetSerializable
{
    public byte id;
    public float value;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref id);
        reader.Get(ref value);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(id);
        write.Put(value);
    }
}

public class StartRun : INetSerializable
{
    public void Deserialize(DataReader reader)
    {
    }

    public void Serialize(DataWriter write)
    {
    }
}
public class Crash : INetSerializable
{
    public float multply;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref multply);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(multply);
    }
}
[Serializable]
public class ClientWallet : INetSerializable
{
    public string name;
    public double balance;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref name);
        reader.Get(ref balance);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(name);
        write.Put(balance);
    }
}
[Serializable]
public class Ranking : INetSerializable
{
    public Line[] rankValue;
    public Line[] rankMulti;


    public void Serialize(DataWriter writer)
    {
        writer.Put(rankValue);
        writer.Put(rankMulti);
    }
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref rankValue);
        reader.Get(ref rankMulti);
    }
}
public class Line : INetSerializable
{
    public string name;
    public float multi;
    public byte bet;
    public void Serialize(DataWriter writer)
    {
        writer.Put(name);
        writer.Put(multi);
        writer.Put(bet);
    }
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref name);
        reader.Get(ref multi);
        reader.Get(ref bet);
    }
}

[Serializable]
public class LastMulti : INetSerializable
{
    public float[] multis;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref multis);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(multis);
    }
}
enum ReceiveMsgIdc
{
    GameLoginResponse = 1,
    BalanceResponse = 2,
    PlayResponse = 3,
    NextBetResponse = 4,
    ErrorResponse = 5,
    BetPlayers = 6,
    Ranking = 7,
    LastMulti = 8
}
enum SendMsgIdc
{
    PlayRequest = 1,
    NextBet = 2,
    GetBalance = 3,
    GetRanking = 4,
    Connection = 5
}