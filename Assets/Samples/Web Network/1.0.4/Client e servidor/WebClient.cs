using UnityEngine;

public class WebClient : WebClientBase
{
    [SerializeField] string url;
    [SerializeField] string token;
    [SerializeField] CanvasManager canvasManager;
    [SerializeField] GameManager gameManager;
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
        CreateConnection(url, token);
        TryConnect();
        SendMsg(new Balance());
    }
    
    protected override void OnOpen()
    {
        base.OnOpen();
        //SendMsg(new MensageExemple { value1 = "a", value2 = 111 });
    }

  
    protected override void OnClose(int closeCode)
    {
        base.OnClose(closeCode);
        TryConnect();
    }
    bool isRunning = false;
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

    public void SendBet()
    {
        if (isRunning)
        {
            SendMsg(new StopBet { } );
            //SendMsg(new Balance());
        }
        else
        {
            SendMsg(new BetServer { value = 10 });
            //SendMsg(new Balance());

        }
    }


    void Box(Box msg)
    {
        Debug.Log(msg.bonus);
    }

    public void MensageControl(MensageControl msg)
    {
        Debug.Log($"{msg.msg} : {msg.valor} ");
    }
}
