using Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebServer : WebServerBase
{
    [SerializeField] string validToken;
    [SerializeField] bool autoStart = true;
    WebSession session;
    [SerializeField] List<WebSession> playersConnected = new List<WebSession>();
    [SerializeField] List<WebSession> playersBet = new List<WebSession>();
    [SerializeField] TankConfiguration TankConfiguration = new TankConfiguration();
    [SerializeField] ServerHUD serverHUD;

    private void Awake()
    {
        if (autoStart) StartServer();
    }
    protected override void Start()
    {
        base.Start();
        RegisterHandler<BalanceCreditServer>(BalanceCreditServer, false, true);
        RegisterHandler<BetServer>(BetServer, false, true);
        RegisterHandler<StopBet>(StopBet, false, true);
        RegisterHandler<SetBet>(SetBet, false, true);
        RegisterHandler<AddBonus>(AddBonus, false, true);
        RegisterHandler<Login>(Login, false, true);
        StartCoroutine(StartRun());
        serverHUD?.ServerStatus(true);
    }
    protected override void Update()
    {
        base.Update();


    }

    void Login(WebSession session, Login msg)
    {
        Debug.Log(msg.token);
    }

    public override void OnConnectd(WebSession session)
    {
        this.session = session;
        var g = new System.Random();
        session.SetClientInfos(new Client() { nickName = "Player" + g.Next(999), credits = 100f });
        Debug.Log("OnConnectd");
        base.OnConnectd(session);
        playersConnected.Add(session);
        session.SendMsg(new Balance { msg = session.GetClient<Client>().nickName, valor = session.GetClient<Client>().credits });
        serverHUD?.TotalPlayers(playersConnected.Count);
    }

    public override void OnDisconnectd(WebSession session)
    {
        base.OnDisconnectd(session);
        playersConnected.Remove(session);
        serverHUD?.TotalPlayers(playersConnected.Count);
    }

    public override bool ValideToken(string token)
    {
        return true;
    }

    void SendToAll<T>(T msg) where T : INetSerializable
    {
        //Debug.Log("players total:" + playersConnected.Count);
        playersConnected.ForEach(x => x.SendMsg(msg));

    }
    void SendToAllUpdateCredit()
    {
        playersConnected.ForEach(x =>
        {
            var g = x.GetClient<Client>();
            Debug.Log($"Atualizando creditos Server > Client: {g.credits}");
            x.SendMsg(new BalanceCreditClient { valor = g.credits });
        });

    }
    float currentTime = 5f;
    bool canBet = false;
    float playerMultiplicador = 1f;
    float totalBonus = 0f;
    bool crash = false;
    IEnumerator StartRun()
    {
        serverHUD?.UpdateLastResult(0);
        yield return new WaitForSeconds(5);
        var r = new System.Random();
        var timeline = 0f;
        while (true)
        {
            SendToAllUpdateCredit();
            SendToAll(new MensageControl { msg = "ResetBets", useValor = 0 });
            SendToAll(new MensageControl { msg = "Timer", valor = currentTime, useValor = 0 });
            canBet = true;
            var luck = r.Next(0, 101);
            var range = luck <= TankConfiguration.bestChance ? (float)(r.NextDouble() * TankConfiguration.maxMultiplicador - 0.02) :
                        luck <= TankConfiguration.greatChance ? (float)(r.NextDouble() * (TankConfiguration.maxMultiplicador / 2) - 0.02) :
                        (float)(r.NextDouble() * (TankConfiguration.maxMultiplicador / 4) - 0.02);
            range = float.Parse($"{range:0.00}");
            playersConnected.ForEach(x => x.GetClient<Client>().isStopBet = false);
            playersConnected.ForEach(x => x.SendMsg(new ButtonBet { active = true, txt = $"Bet {x.GetClient<Client>().betValor:0.00}" }));
            totalBonus = 0;
            playerMultiplicador = 1f;
            serverHUD?.UpdateMultiplier(playerMultiplicador, totalBonus);
            while (currentTime > 0)
            {
                serverHUD?.UpdateAtualFase($"Wait Bets ... {currentTime:00:00}");
                currentTime--;
                yield return new WaitForSeconds(1f);
                SendToAll(new MensageControl { msg = "Timer", valor = currentTime, useValor = 0 });
            }
            canBet = false;
            currentTime = TankConfiguration.timeWait;
            crash = false;
            timeline = 0f;
            SendToAll(new StartRun());
            bool bomb = false;
            while (!crash)
            {
                serverHUD?.UpdateAtualFase($" Walking...");
                playerMultiplicador = float.Parse($"{playerMultiplicador + 0.01f:0.00}");
                SendToAll(new MultSync { mult = playerMultiplicador });
                if ((timeline * 100) % 20 == 0)
                {
                    var box = TankConfiguration.bonusList[r.Next(TankConfiguration.bombChance <= TankConfiguration.bonusList.Count ? TankConfiguration.bombChance : TankConfiguration.bonusList.Count)];
                    SendToAll(new Box { bonus = box});
                    SendToAll(new Parallax { velocidade = 0.02f });
                    bomb = box == 0;
                }
                serverHUD?.UpdateMultiplier(playerMultiplicador,totalBonus);
                playersConnected.ForEach(x =>
                {
                    var c = x.GetClient<Client>();
                    bool b = playersBet.Contains(x);
                    if (!c.isStopBet && b) {
                        x.SendMsg(new ButtonBet { active = true, txt = $"Stop x {playerMultiplicador}" });
                        if ( c.currentBet.stop <= playerMultiplicador)
                        {
                            Debug.Log("Try Stop Bet [Server] Resposta");
                            StopBet(x, new StopBet());
                        }
                     }
                    else if (!b) x.SendMsg(new ButtonBet { active = false, txt = "Wait Next Round" });
                });
                timeline = float.Parse($"{timeline + 0.01f:0.00}");
                if (timeline >= range ) { crash = true; }
                yield return new WaitForSeconds(playerMultiplicador <= 2 ? 0.2f : playerMultiplicador <= 5 ? 0.1f : 0.05f);
            }
            SendToAll(new Crash { multply = playerMultiplicador });
            serverHUD?.UpdateLastResult(playerMultiplicador);
            playersBet.Clear();
        }
    }


    void AddBonus(WebSession session, AddBonus msg)
    {
        if (msg.valor != 0) totalBonus += msg.valor;
        else crash = true;
        Debug.Log(msg.valor != 0 ? $"[Server] Add Bonus {msg.valor:0.00}, total : x{totalBonus:0.00}" : "[Server] kabum");
    }

    void BetServer(WebSession session, BetServer msg)
    {
        Debug.Log("Bet [Server] Resposta");
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
        session.SendMsg(new Balance { msg = client.nickName, valor = client.credits });
        session.SendMsg(new MensageControl { msg = "Aposta: Aposta feita!" });
        session.SendMsg(new ButtonBet { active = true, txt = "Wait Start..." });
        SendToAll(new BetPlayers { msg = client.nickName, valor = msg.value });
        serverHUD?.UpdateTotalIn(msg.value);
    }

    void StopBet(WebSession session, StopBet msg)
    {
        var client = session.GetClient<Client>();
        Debug.Log("BetStop [Server] Resposta");
        if (!playersBet.Contains(session))
        {
            session.SendMsg(new MensageControl { msg = "Espere a proxima Rodada" });
            return;
        }
        var add = client.currentBet.bet * (playerMultiplicador + totalBonus);
        client.credits += add;
        session.SendMsg(new MensageControl { msg = $"Aposta {client.currentBet.bet:0.00} Multiplicador: {playerMultiplicador}+ Bonus {totalBonus} , Total Ganho : {add:0.00}", useValor = -1 });
        session.SendMsg(new Balance { msg = client.nickName, valor = client.credits });
        session.SendMsg(new ButtonBet { active = false, txt = $" Winner x {playerMultiplicador+totalBonus:0.00}" });
        SendToAll(new BetPlayers { msg = client.nickName, valor = client.currentBet.bet, multply = playerMultiplicador + totalBonus });
        playersBet.Remove(session);
        serverHUD?.UpdateTotalOut(add);
    }

    void BalanceCreditServer(WebSession session, BalanceCreditServer msg)
    {
        var client = session.GetClient<Client>();
        session.SendMsg(new BalanceCreditClient { valor = client.credits });

    }

    void SetBet(WebSession session, SetBet msg)
    {
        var client = session.GetClient<Client>();
        client.betValor = msg.valor;
        Debug.Log("Valor Bet Atual : " + client.betValor);
    }

}
[Serializable]
class Client : IClientInfos
{
    public string nickName;
    public float credits;
    public float betValor;
    public bool isStopBet = false;
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
