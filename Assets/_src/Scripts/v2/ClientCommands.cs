using GameSpawner;
using Serializer;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Web;
using UnityEngine;

public class ClientCommands : WebClientBase
{
    public static ClientCommands Instance;
    string url;
    [SerializeField] string urlTest;
    [SerializeField] string urlDev;
    [SerializeField] string urlLocalhost = "ws://localhost:1003";
    [SerializeField] bool uselocalhost;
    [SerializeField] string token;
    [SerializeField] bool debug = true;
    public string playerName;
    public bool isRunning, onConnect = false;
    Conection data;
    public bool onTutorial = false;
    [SerializeField] public int atualStatus;// 0: Timer 1: round 2: Crash 3:OutRound
    [SerializeField] string urltoken;
    [SerializeField] string urlLanguage; //pt,en,es
    [SerializeField] public bool tank1OnRunning, tank2OnRunning, tank3OnRunning;
    [SerializeField] LastMulti slots = new() { multis = new multiplier[3] { new multiplier { multiply = 0, tankid = 0 }, new multiplier { multiply = 0, tankid = 1 }, new multiplier { multiply = 0, tankid = 2 } } };

    [SerializeField] bool onBonusDrop = false;
    [SerializeField] public int defaultLanguage = 1;
    string urlCallback ;

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
        RegisterHandler<LastMultiFivity>(LastMultiResponse, (ushort)ReceiveMsgIdc.LastMulti);
        RegisterHandler<BonusDrop>(BonusDropResponse, (ushort)ReceiveMsgIdc.BonusDrop);
#if UNITY_WEBGL && !UNITY_EDITOR
        GetParameters();
        CreateConnection(GetUrl(), urltoken);
#else
        urltoken = token;
        Debug.Log("url token: " + urltoken);
        if (uselocalhost)
        {
           LanguageManager.instance.ChangeLanguage(defaultLanguage);
           CreateConnection(urlLocalhost, token);
        }
        else
        {
            CreateConnection(GetUrl(), token);
        }
#endif        
        data = new Conection() { data = new byte[] { 255, 255 } };
        onConnect = true;
        TryConnect();
    }


    protected override void OnOpen()
    {
        CanvasManager.Instance.ShowLoadingPanel(false);
        base.OnOpen();
    }

    protected override void OnClose(int closeCode)
    {
        CanvasManager.Instance.ShowLoadingPanel(true);
        base.OnClose(closeCode);
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

    public void GetParameters()
    {
        int pm = Application.absoluteURL.IndexOf("?");
        if (pm != -1)
        {
            string queryString = Application.absoluteURL.Split("?")[1];
            NameValueCollection queryParams = HttpUtility.ParseQueryString(queryString);

            urltoken = queryParams["t"];
            byte[] tokenBytes = Convert.FromBase64String(urltoken);
            string decodedString = Encoding.UTF8.GetString(tokenBytes);
            string l = queryParams["h"];// rr = pt,ss = en, zz = es
            LanguageManager.instance.ChangeLanguage(l switch { "ss" => 0, "rr" => 1, "zz" => 2, _ => defaultLanguage });
            defaultLanguage = l switch { "ss" => 0, "rr" => 1, "zz" => 2, _ => defaultLanguage };
            urlCallback = queryParams["cb"];
            string c = queryParams["g"];
            GameManager.Instance.Culture = CultureInfo.GetCultureInfo(c switch { 
                "zzz" => "pt-BR", 
                "aaa" => "en-US", 
                "sss" => "es-ES",
                _ => "pt-BR" });
            if (debug)
            {
                Debug.Log("####### urltoken ####### " + urltoken);
                Debug.Log("####### currency ####### " + c);
                Debug.Log("####### language ####### " + l);
            }
        }
    }
    
    string GetUrl()
    {
        byte[] tokenBytes = Convert.FromBase64String(urltoken);
        string decodedString = Encoding.UTF8.GetString(tokenBytes);
        string[] parts = decodedString.Split('|');
        if ((parts.Length < 5) || (parts.Length > 6) || string.IsNullOrEmpty(parts[3]))
        {
            Debug.LogError("invalid token.");
            Debug.Log("invalid token. " + decodedString);
            return string.Empty;
        }
        string server = parts[3];
        if (BuildType == BuildType.Dev)
        {
            server += "test." + urlDev;
        }
        else
        {
            server += "." + urlTest;
        }

        string nurl = $"wss://{server}";
        Debug.Log("nurl " + nurl);
        return nurl;
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
            Debug.Log($"LogResponse:{msg.name} {msg.code}");
        }

        playerName = msg.name;
    }

    public void BalanceResponse(BalanceResponse msg)
    {
        balance = msg.balance;
        if (onTutorial) return;
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

        //msg.data.tankid tank da mensagem atual 0,1,2 

        if (msg.data.id == 0)// Start Timer
        {
            CanvasManager.Instance.tankList.ForEach(x => x.lastTank = false);
            CanvasManager.Instance.bonus.ForEach(x => x.gameObject.SetActive(false));
            CanvasManager.Instance.ResetBonus();
            CanvasManager.Instance.ResetBets();
            GameManager.Instance.NewMatchInit();            // AutoPlay Start
            StartCoroutine(GameManager.Instance.DisplayTimer(msg.data.value));
            CanvasManager.Instance.SetBetButtonBet();
            CanvasManager.Instance.SetMultiplierTextMensage(true);
            CanvasManager.Instance.SetPlayerState(0, null, true);
            CanvasManager.Instance.ShowCanvasSelectTank(!GameManager.Instance.activeAutoPlay);
            atualStatus = 0;
        }
        else if (msg.data.id == 1)// Start Round 
        {
            CanvasManager.Instance.ShowCanvasSelectTank(false);
            GameManager.Instance.NewMathStart();
            StopAllCoroutines();
            CanvasManager.Instance.SetBetButtonCantBet();
            StartRun(new StartRun());
            StartCoroutine(GameManager.Instance.DisplayMulti(msg.data.value));
            StartCoroutine(ConfirmConnection());
            CanvasManager.Instance.SetMultiplierTextMensage(false);
            atualStatus = 1;
        }
        else if (msg.data.id == 2) // End Round Crash
        {
            switch (msg.data.tankid)
            {
                case 0:
                    tank1OnRunning = false;
                    break;
                case 1:
                    tank2OnRunning = false;
                    break;
                case 2:
                    tank3OnRunning = false;
                    break;
            }
            Crash(new Crash { multply = msg.data.value }, msg.data.tankid);
            if (GameManager.Instance.selectedTankNum == msg.data.tankid)
            {
                GameManager.Instance.isJoin = false;
                Debug.Log($"On Crash Own Tank :{msg.data.tankid}");
            }
            if (!tank1OnRunning && !tank2OnRunning && !tank3OnRunning)
            {
                if (slots.multis[0].multiply != 0 && slots.multis[1].multiply != 0 && slots.multis[2].multiply != 0)
                {
                    CanvasManager.Instance.SetLastPlays(slots);
                }
                slots = new() { multis = new multiplier[3] { new multiplier { multiply = 0, tankid = 0 }, new multiplier { multiply = 0, tankid = 1 }, new multiplier { multiply = 0, tankid = 2 } } };
                isRunning = false;
                CanvasManager.Instance.tankList.ForEach(x => x.Stop());
                CanvasManager.Instance.bonus.ForEach(x => x.GetComponent<MovingBox>().Stop());
                GameManager.Instance.isWalking = false;
                GameManager.Instance.canBet = true;
                GameManager.Instance.ResetVelocityParalax();
                GameManager.Instance.fundoOnMove = false;
                GameManager.Instance.EndMatchStart();
                StopAllCoroutines();
                CanvasManager.Instance.SetMultiplierText(msg.data.value);
            }
            StartCoroutine(ConfirmConnection());
            atualStatus = 2;

        }
        else if (msg.data.id == 3) // Join Round
        {
            Debug.Log($"On Join Tank :{msg.data.tankid}");
            CanvasManager.Instance.SetBetButtonCantBet();
            GameManager.Instance.isJoin = true;
        }
        else if (msg.data.id == 4) // Bet CashOut
        {
            GameManager.Instance.isJoin = false;
            CanvasManager.Instance.SetBetButtonCantBet();
            atualStatus = 3;
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
        if (debug) Debug.Log((msg.multiplier == 0) ? 
            ($"[Client] O Jogador {msg.name} fez aposta pagando {msg.value:0.00}") : 
            ($"[Client] O jogador {msg.name} Retirou {msg.multiplier:0.00}"));
        CanvasManager.Instance.SetBetSlot(msg);
    }

    void RankResponse(Ranking ranking)
    {
        if (debug) Debug.Log("RankResponse mult:" + ranking.rankValue.Length + " / cash :" + ranking.rankMulti.Length);
        Line[] jsonMult = ranking.rankMulti;
        Line[] jsonCash = ranking.rankValue;
        //CanvasManager.Instance.SetRank(jsonMult, jsonCash);
    }

    void LastMultiResponse(LastMultiFivity lastMulti)
    {
        if (debug) Debug.Log("LastMultiResponse:" + lastMulti.multis);
        foreach (LastMulti item in lastMulti.multis)
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

    public void BonusDropResponse(BonusDrop msg)
    {
        if (debug)
        {
            Debug.Log($"[Servidor] BonusDropResponse {msg.prize}");
        }
        if (msg.prize == 0 )
        {
            if (msg.matchId != 0 && onBonusDrop) 
            { 
                Debug.Log("Boom");
                CanvasManager.Instance.bonus.ForEach(x => x.GetComponent<MovingBox>().popUpText.text = "BOOM");
            }
            else
            {
                onBonusDrop = true;
                CanvasManager.Instance.PlayBonus(msg);
                CanvasManager.Instance.tankList.ForEach(x => x.lastTank = true);
                if (debug) Debug.Log("start bonus");

            }
        } 
        else
        {
            CanvasManager.Instance.bonus.ForEach(x => x.GetComponent<MovingBox>().popUpText.text = $"x {msg.prize:0.00}");
            CanvasManager.Instance.ShowBonus(msg.prize);
            if (debug) Debug.Log($"Get bonus{msg.prize}");
        }
        
    }

    public void SendBet()
    {
        if (onTutorial) return;
        if (debug)
        {
            Debug.Log($"[Client] Send Bet tank : {GameManager.Instance.selectedTankNum}");
        }
        SendMsg((ushort)SendMsgIdc.PlayRequest, new PlayRequest ((byte)GameManager.Instance.selectedTankNum));// tankid = 0,1,2
    }

    public void TrySendBet()
    {
        if (!GameManager.Instance.isJoin && !GameManager.Instance.isWalking) 
            SendBet();
    }

    public void StartRun(StartRun msg, bool? tutorial = false)
    {
        if(onTutorial && tutorial == false) return;
        isRunning = true;
        GameManager.Instance.isWalking = true;
        GameManager.Instance.canBet = false;
        //CanvasManager.Instance.SetPlayerState(true,0,true);
        tank1OnRunning = true;
        tank2OnRunning = true;
        tank3OnRunning = true;
        AudioManager.Instance.StopResumeSFX(false);
        GameManager.Instance.fundoOnMove = true;
        if (debug)
        {
            Debug.Log("Start Corrida");
        }
    }

    public void Crash(Crash msg,byte tankid, bool? tutorial = false)
    {
        if (onTutorial && tutorial == false) return;
        CanvasManager.Instance.SetPlayerState(tankid,false);
        AudioManager.Instance.StopResumeSFX(true);
        AudioManager.Instance.PlayOneShot(2);
        onBonusDrop = false;
        switch (tankid)
        {
            case 0:
                slots.multis[0].multiply = msg.multply;
                break;
            case 1:
                slots.multis[1].multiply = msg.multply;
                break;
            case 2:
                slots.multis[2].multiply = msg.multply;
                break;
        }
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
    public byte tankid;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref id);
        reader.Get(ref value);
        reader.Get(ref tankid);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(id);
        write.Put(value);
        write.Put(tankid);
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
public class multiplier : INetSerializable
{
    public float multiply;
    public byte tankid;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref multiply);
        reader.Get(ref tankid);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(multiply);
        write.Put(tankid);
    }
}

[Serializable]
public class LastMulti : INetSerializable
{
    public multiplier[] multis;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref multis);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(multis);
    }
}

[Serializable]
public class LastMultiFivity : INetSerializable
{
    public LastMulti[] multis;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref multis);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(multis);
    }
}
public class BonusDrop : INetSerializable
{
    public int matchId;
    public float prize;
    public double time;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref matchId);
        reader.Get(ref prize);
        reader.Get(ref time);
    }  

    public void Serialize(DataWriter write)
    {
        write.Put(matchId);
        write.Put(prize);
        write.Put(time);
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
    LastMulti = 8,
    BonusDrop = 9
}
enum SendMsgIdc
{
    PlayRequest = 1,
    NextBet = 2,
    GetBalance = 3,
    GetRanking = 4,
    Connection = 5
}