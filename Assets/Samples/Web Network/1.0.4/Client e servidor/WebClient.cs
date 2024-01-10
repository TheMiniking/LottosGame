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
        CreateConnection(url, token);
        TryConnect();
        SendMsg(new Balance());
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

    public void MensageControl(MensageControl msg)
    {
        Debug.Log($"{msg.msg} : {msg.valor} ");
    }

    public void BalanceCreditClient( BalanceCreditClient msg)
    {
        gameManager.credits = msg.valor;
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
  
