using Sirenix.OdinInspector;
using UnityEngine;

public class WebClient : WebClientBase
{
    [SerializeField] string url;
    [SerializeField] string token;
    protected override void Start()
    {
        base.Start();
        RegisterHandler<StartRun>(StartRun);
        RegisterHandler<Crash>(Crash);
        RegisterHandler<TimerSync>(TimerSync);
        RegisterHandler<Alert>(MensageAlert);
        CreateConnection(url, token);
        TryConnect();
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
        Debug.Log("Start Corrida");
    }

    void Crash(Crash msg)
    {
        isRunning = false;
        Debug.Log("Kabum , Distance x"+ msg.multply);
    }
    void TimerSync(TimerSync msg)
    {
        Debug.Log($"Time :{msg.time:00:00}");
    }

    public void SendBet()
    {
        if (isRunning)
        {
            SendMsg(new StopBet { } );
        }
        else
        {
            SendMsg(new BetServer { value = 10 });

        }
    }

    public void MensageAlert(Alert msg)
    {
        Debug.Log(msg.mensage);
    }

}
