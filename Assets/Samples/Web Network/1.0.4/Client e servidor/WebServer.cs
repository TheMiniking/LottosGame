using Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebServer : WebServerBase
{
    [SerializeField] string validToken;
    WebSession session;
    List<WebSession> playersConnected = new List<WebSession>();
    List<WebSession> playersBet = new List<WebSession>();

    private void Awake()
    {
        StartServer();
    }
    protected override void Start()
    {
        base.Start();
        RegisterHandler<BetServer>(Bet, false, true);
        RegisterHandler<StopBet>(StopBet, false, true);
        StartCoroutine(StartRun());
    }
    protected override void Update()
    {
        base.Update();


    }

    public override void OnConnectd(WebSession session)
    {
        this.session = session;
        session.SetClientInfos(new Client() { nickName = "Player" + UnityEngine.Random.Range(0, 999), credits = 100 });
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

        while (true)
        {
            canBet = true;
            while (currentTime > 0)
            {
                currentTime--;
                SendToAll(new TimerSync { time = currentTime });
                yield return new WaitForSeconds(1f);
            }
            canBet = false;
            currentTime = 5f;
            //Mathematics.TankCalculeRound();
            var crash = false;
            playerMultiplicador = 1f;
            SendToAll(new StartRun());
            while (!crash)
            {
                playerMultiplicador += UnityEngine.Random.Range(0.01f, 0.05f);
                if (UnityEngine.Random.Range(0, 100) < 10)
                {
                    crash = true;
                }
                yield return new WaitForEndOfFrame();
            }
            SendToAll(new Crash { multply = playerMultiplicador });
            playersBet.Clear();
        }
    }

    void Bet(WebSession session, BetServer msg)
    {
        var client = session.GetClient<Client>();
        Debug.Log($"Cliente = {client==null} : Valor = {msg == null}");
        if (!client.VerifyCredits(msg.value))
        {
            session.SendMsg(new Alert { mensage = "Credito Insuficiente!" });
            return;
        }
        if (!canBet)
        {
            session.SendMsg(new Alert { mensage = "Espere a Rodada terminar" });
            return;
        }
        if (playersBet.Contains(session))
        {
            session.SendMsg(new Alert { mensage = "Aposta ja feita!" });
            return;
        }
        client.Register(msg.value, msg.stop);
        playersBet.Add(session);
        session.SendMsg(new Alert { mensage = "Aposta feita!" });
    }

    void StopBet(WebSession session, StopBet msg)
    {
        var client = session.GetClient<Client>();
        if (!playersBet.Contains(session))
        {
            session.SendMsg(new Alert { mensage = "Aposta nao encontrada!" });
            return;
        }
        var add = client.currentBet.bet * playerMultiplicador;
        client.credits += add;
        session.SendMsg(new Alert { mensage = $"Aposta {client.currentBet.bet:0.00} Multiplicador: {playerMultiplicador} , Total Ganho : {add:0,00}" });
        playersBet.Remove(session);

    }
}
[Serializable]
class Client : IClientInfos
{
    public string nickName;
    public float credits;
    public BetRegister currentBet;
    public List<BetRegister> lastBets;

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
