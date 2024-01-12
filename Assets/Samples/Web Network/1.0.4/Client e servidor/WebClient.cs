using UnityEngine;

public class WebClient : WebClientBase
{
    [SerializeField] string url;
    [SerializeField] string token;
    [SerializeField] CanvasManager canvasManager;
    [SerializeField] GameManager gameManager;
    bool isRunning = false;

    protected override void Start()
    {
        base.Start();
        RegisterHandler<StartRun>(StartRun);
        RegisterHandler<Crash>(Crash);
        RegisterHandler<TimerSync>(TimerSync);
        RegisterHandler<MultSync>(MultSync);
        RegisterHandler<MensageControl>(MensageControl);
        RegisterHandler<Balance>(Balance);
        RegisterHandler<Parallax>(Paralax);
        RegisterHandler<Box>(Box);
        RegisterHandler<ButtonBet>(ButtonBet);
        RegisterHandler<BalanceCreditClient>(BalanceCreditClient);
        RegisterHandler<BetPlayers>(BetPlayers);
        CreateConnection(url, token);
        TryConnect();
    }
    
    protected override void OnOpen()
    {
        base.OnOpen();
        SendMsg(new Login { token = "31546" });
        Debug.Log("OnOpen ");
        //SendMsg(new MensageExemple { value1 = "a", value2 = 111 });
    }

  
    protected override void OnClose(int closeCode)
    {
        base.OnClose(closeCode);
        TryConnect();
    }
    #region RespostaServer
    void StartRun(StartRun msg)
    {
        isRunning = true;
        gameManager.isWalking = true;
        gameManager.canBet = false;
        canvasManager.SetTankState("Walking");
        //Debug.Log("Start Corrida");
    }

    void Crash(Crash msg)
    {
        isRunning = false;
        gameManager.isWalking = false;
        gameManager.canBet = true;
        canvasManager.SetTankState("Crash");
        canvasManager.ResetVelocityParalax();
        canvasManager.SetLastPlays(msg.multply);
        Debug.Log("Kabum , Distance x"+ msg.multply);
    }
    void TimerSync(TimerSync msg)
    {
        canvasManager.SetTimer((int)msg.time);
        //Debug.Log($"Time :{msg.time:00:00}");
    }

    void MultSync(MultSync msg)
    {
        canvasManager.SetMultiplicador(msg.mult);
    }

    void Paralax(Parallax msg)
    {
        canvasManager.AddVelocityParalax(msg.velocidade);
    }

    void Balance(Balance msg)
    {
        canvasManager.SetWalletNick(msg.msg);
        canvasManager.SetWalletBalance(msg.valor);
        gameManager.credits = msg.valor;
    } 
    public void ButtonBet(ButtonBet msg)
    {
        if (msg.active)
        {
            canvasManager.SetBetActive();
        }
        else
        {
            canvasManager.SetBetDesactive();
        }
        if(msg.txt != "") canvasManager.SetBetButtonText(msg.txt);
    }
    void Box(Box msg)
    {
        Debug.Log(msg.bonus);
    }

    //Funçao de Controle e coringa para funcoes de pouco uso
    // useValor controla  o uso da funçao, sendo -1 default, apenas debug mensagens do server
    // -1 = default, 0 = usa Valor (funçao) msg (parametro), 1 = usa msg(funçao) valor(parametro) , usa
    public void MensageControl(MensageControl msg)
    {
        if (msg.useValor == -1)
        {
            Debug.Log($"{msg.msg} : {msg.valor} ");
            return;
        }
        if (msg.useValor == 1 ) {
            switch (msg.valor)
            {

            }
        }
        else if (msg.useValor == 0)
        { 
            switch (msg.msg)
            {
                case "Timer":
                    canvasManager.SetTimer((int)msg.valor);
                    break;
                case "ResetBets":
                    canvasManager.ResetPlayersBet();
                    break;
        }
        }

    }

    public void BalanceCreditClient( BalanceCreditClient msg)
    {
        gameManager.credits = msg.valor;
    }


    public void BetPlayers( BetPlayers msg)
    {
        Debug.Log(msg.multply == 0?$"[Client] Jogador{msg.msg} fez aposta pagando{msg.valor}": $"[Cliente] O jogador {msg.msg} Retirou e ganhou{msg.valor*msg.multply}");
        if (msg.multply == 0)
        {
            canvasManager.SetPlayersBet(msg);
        }
        else
        {
            canvasManager.SetPlayersWin(msg);
        }
    }

    #endregion

    #region ClientToServer   //nao esta funcionando
    public void SendBet(float bet)
    {
        if (isRunning)
        {
            SendMsg(new StopBet { } );
            Debug.Log("Stop bet [Client]");
            //SendMsg(new Balance());
        }
        else
        {
            SendMsg(new BetServer { value = bet });
            Debug.Log("Start bet [Client] : "+ bet);
            //SendMsg(new Balance());

        }
    }

    public void GetBalance()
    {
        SendMsg(new BalanceCreditServer());
    }

    public void SetBetValor(float bet )
    {
        SendMsg(new SetBet { valor = bet });
        canvasManager.SetBetButtonText($"Bet {bet:0.00}");
    }
}
    #endregion
  
