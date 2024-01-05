using Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebServer : WebServerBase
{
    [SerializeField] string validToken;
     WebSession session;
    [SerializeField] List<WebSession> playersConnected = new List<WebSession>();
    [SerializeField] List<WebSession> playersBet = new List<WebSession>();

    private void Awake()
    {
        StartServer();
    }
    protected override void Start()
    {
        base.Start();
        RegisterHandler<BetServer>(Bet, false, true);
        RegisterHandler<StopBet>(StopBet, false, true);
        //RegisterHandler<Balance>(SetBalance, false, true);
        StartCoroutine(StartRun());
    }
    protected override void Update()
    {
        base.Update();


    }

    public override void OnConnectd(WebSession session)
    {
        this.session = session;
        var g = new System.Random();
        session.SetClientInfos(new Client() { nickName = "Player" + g.Next(999), credits = 100f });
        Debug.Log("OnConnectd");
        base.OnConnectd(session);
        playersConnected.Add(session);
    }

    public override void OnDisconnectd(WebSession session)
    {
        base.OnDisconnectd(session);
        playersConnected.Remove(session);
    }

    public override bool ValideToken(string token)
    {
        return true;
    }

    void SendToAll<T>(T msg) where T : INetSerializable
    {
        Debug.Log("players total:" + playersConnected.Count);
        playersConnected.ForEach(x => x.SendMsg(msg));

    }

    float currentTime = 5f;
    bool canBet = false;
    float playerMultiplicador = 1f;
    IEnumerator StartRun()
    {
        var r = new System.Random();
        var i = 0;
        while (true)
        {
            canBet = true;
            SendToAll(new TimerSync { time = currentTime });
            while (currentTime > 0)
            {
                currentTime--;
                yield return new WaitForSeconds(1f);
                SendToAll(new TimerSync { time = currentTime });
            }
            canBet = false;
            currentTime = 5f;
            //Mathematics.TankCalculeRound();
            var crash = false;
            playerMultiplicador = 1f;
            SendToAll(new StartRun());
            while (!crash)
            {
                i = i!=20? i++: i=0;
                if (i == 0) SendToAll(new Box { bonus = r.Next(11)/10});
                playerMultiplicador += 0.01f;
                if (playerMultiplicador % 0.2 == 0) SendToAll(new Parallax { velocidade = 0.01f });
                SendToAll(new MultSync { mult = playerMultiplicador });
                if (r.Next(0, 100) < 10)
                {
                    crash = true;
                }
                yield return new WaitForSeconds(playerMultiplicador <=2 ? 0.3f : playerMultiplicador <=5? 0.2f :0.1f);
            }
            SendToAll(new Crash { multply = playerMultiplicador });
            playersBet.Clear();
        }
    }

    void Bet(WebSession session, BetServer msg)
    {
        var client = session.GetClient<Client>();
        if (!client.VerifyCredits(msg.value))
        {
            session.SendMsg(new MensageControl { msg = "Aposta: Credito Insuficiente!" });
            return;
        }
        if (!canBet)
        {
            session.SendMsg(new MensageControl { msg = "Aposta: Espere a Rodada terminar" });
            return;
        }
        if (playersBet.Contains(session))
        {
            session.SendMsg(new MensageControl { msg = "Aposta: Aposta ja feita!" });
            return;
        }
        client.Register(msg.value, msg.stop);
        playersBet.Add(session);
        session.SendMsg(new MensageControl { msg = "Aposta: Aposta feita!" });
    }

    void StopBet(WebSession session, StopBet msg)
    {
        var client = session.GetClient<Client>();
        if (!playersBet.Contains(session))
        {
            session.SendMsg(new MensageControl { msg = "Aposta nao encontrada!" });
            return;
        }
        var add = client.currentBet.bet * playerMultiplicador;
        client.credits += add;
        session.SendMsg(new MensageControl { msg = $"Aposta {client.currentBet.bet:0.00} Multiplicador: {playerMultiplicador} , Total Ganho : {add:0,00}" });
        playersBet.Remove(session);

    }

    void SetBalance(WebSession session, Balance msg)
    {
        var client = session.GetClient<Client>();
        session.SendMsg(new Balance { valor = client.credits, msg = client.nickName });
    }
}
[Serializable]
class Client : IClientInfos
{
    public string nickName;
    public float credits;
    public BetRegister currentBet;
    public List<BetRegister> lastBets = new List<BetRegister>();

    public void Register(float valor, float stop)
    {
        credits -= valor;
        currentBet = new BetRegister { bet = valor, stop = stop };
        lastBets.Add(currentBet);
    }

    public bool VerifyCredits(float bet)
    {
        return credits >= bet;
    }
}

[Serializable]
class BetRegister
{
    public float bet;
    public float stop;
    public float multiplicador;
    public List<float> bonus;
}
