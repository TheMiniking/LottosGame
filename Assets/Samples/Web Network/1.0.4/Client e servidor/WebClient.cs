using GameSpawner;
using Serializer;
using UnityEngine;

namespace BV
{

    public class WebClient : WebClientBase
    {
        public static WebClient Instance;
        public bool logDebug = false;
        [SerializeField] string url;
        [SerializeField] string token;
        bool isRunning = false;

        void Awake()
        {
            Instance = this;
        }

        //protected override void Start()
        //{
        //    base.Start();

        //    //RegisterHandler<GameLoginResponse>(GameLoginResponse);
        //    //RegisterHandler<BalanceResponse>(BalanceResponse);
        //    //RegisterHandler<PlayResponse<MathStatus>>(PlayResponse, 35559);
        //    //RegisterHandler<StartRun>(StartRun);
        //    //RegisterHandler<Crash>(Crash);
        //    //RegisterHandler<TimerSync>(TimerSync);
        //    //RegisterHandler<MultSync>(MultSync);
        //    //RegisterHandler<MensageControl>(MensageControl);
        //    //RegisterHandler<Balance>(Balance);
        //    //RegisterHandler<Parallax>(Paralax);
        //    //RegisterHandler<Box>(Box);
        //    //RegisterHandler<ButtonBet>(ButtonBet);
        //    //RegisterHandler<BalanceCreditClient>(BalanceCreditClient);
        //    //RegisterHandler<BetPlayers>(BetPlayers);
        //    CreateConnection(url, token);
        //    TryConnect();
        //}

        //protected override void OnOpen()
        //{
        //    base.OnOpen();
        //    SendMsg(new GameLogin { token = token });
        //    Debug.Log("OnOpen ");
        //    //SendMsg(new MensageExemple { value1 = "a", value2 = 111 });
        //}


        protected override void OnClose(int closeCode)
        {
            base.OnClose(closeCode);
            TryConnect();
        }

        public void SendMensagem<T>(T msg) where T : INetSerializable
        {
            SendMsg(0, msg);
        }

        #region RespostaServer

        //void GameLoginResponse(GameLoginResponse msg)
        //{
        //    Debug.Log("LogResponse:" + msg.code);
        //}

        //void BalanceResponse(BalanceResponse msg)
        //{
        //    Debug.Log("BalanceResponse:" + msg.balance);
        //}

        //void PlayResponse(PlayResponse<MathStatus> msg)
        //{
        //    Debug.Log($"PlayResponse: id: { msg.data.id} value : {msg.data.value} ");
        //}

        public void StartRun(StartRun msg)
        {
            isRunning = true;
            GameManager.Instance.isWalking = true;
            GameManager.Instance.canBet = false;
            CanvasManager.Instance.SetTankState("Walking");
            //CanvasManager.Instance.ResetPlayersBet();
            //Debug.Log("Start Corrida");
        }

        public void Crash(Crash msg)
        {
            isRunning = false;
            GameManager.Instance.isWalking = false;
            GameManager.Instance.canBet = true;
            CanvasManager.Instance.SetTankState("Crash");
            CanvasManager.Instance.ResetVelocityParalax();
            CanvasManager.Instance.SetLastPlays(msg.multply);
            Debug.Log("Kabum , Distance x" + msg.multply);
        }
        public void TimerSync(TimerSync msg)
        {
            CanvasManager.Instance.SetTimer((int)msg.time);
            //Debug.Log($"Time :{msg.time:00:00}");
        }

        public void MultSync(MultSync msg)
        {
            CanvasManager.Instance.SetMultiplicador(msg.mult);
        }

        public void Paralax(Parallax msg)
        {
            CanvasManager.Instance.AddVelocityParalax(msg.velocidade);
        }

        public void Balance(Balance msg)
        {
            CanvasManager.Instance.SetWalletNick(msg.msg);
            CanvasManager.Instance.SetWalletBalance(msg.valor);
            GameManager.Instance.credits = msg.valor;
            GameManager.Instance.clientName = msg.msg;
        }
        public void ButtonBet(ButtonBet msg)
        {
            if (msg.active)
            {
                CanvasManager.Instance.SetBetActive();
            }
            else
            {
                CanvasManager.Instance.SetBetDesactive();
            }
            //if (msg.txt != "") CanvasManager.Instance.SetBetButtonText(msg.txt);
        }
        void Box(Box msg)
        {
            Debug.Log(msg.bonus);
            CanvasManager.Instance.InstancieBox(msg.bonus);
        }

        //Funçao de Controle e coringa para funcoes de pouco uso
        // useValor controla  o uso da funçao, sendo -1 default, apenas debug mensagens do server
        // -1 = default, 0 = usa Valor (funçao) msg (parametro), 1 = usa msg(funçao) valor(parametro) , usa
        public void MensageControl(MensageControl msg)
        {
            //if (msg.useValor == -1)
            //{
            //    Debug.Log($"{msg.msg} : {msg.valor} ");
            //    return;
            //}
            //if (msg.useValor == 1)
            //{
            //    switch (msg.valor)
            //    {

            //    }
            //}
            //else if (msg.useValor == 0)
            //{
            //    switch (msg.msg)
            //    {
            //        case "Timer":
            //            if (logDebug) Debug.Log(CanvasManager.Instance == null ? "Canvas intance Vazio" : " Canvas Normal");
            //            CanvasManager.Instance.SetTimer((int)msg.valor);
            //            break;
            //        case "ResetBets":
            //            CanvasManager.Instance.ResetPlayersBet();
            //            break;
            //        case "AutoPlay":
            //            GameManager.Instance.CheckAutoPlay();
            //            break;
            //        case "BetOK":
            //            CanvasManager.Instance.AddCashBet(msg.valor);
            //            break;
            //        case "CashOutOK":
            //            CanvasManager.Instance.AddCashOut(msg.valor);
            //            break;
            //    }
            //}

        }

        public void BalanceCreditClient(BalanceCreditClient msg)
        {
            GameManager.Instance.credits = msg.valor;
        }


        //public void BetPlayers(BetPlayers msg)
        //{
        //    Debug.Log(msg.multply == 0 ? $"[Client] Jogador{msg.name} fez aposta pagando{msg.valor}" : $"[Cliente] O jogador {msg.name} Retirou e ganhou{msg.valor * msg.multply}");
        //    if (msg.multply == 0)
        //    {
        //        CanvasManager.Instance.SetPlayersBet(msg);
        //    }
        //    else
        //    {
        //        CanvasManager.Instance.SetPlayersWin(msg);
        //    }
        //}

        #endregion

        #region ClientToServer 
        public void SendBet(float bet, float stop)
        {
            //if (isRunning)
            //{
            //    //SendMsg(new StopBet { });

            //    Debug.Log("Stop bet [Client]");
            //}
            //else
            //{
            //    SendMsg(new BetServer { value = bet, stop = stop });
            //    Debug.Log(stop != 0 ? $"Start bet [Client] , Auto Bet ON. bet:{bet} Stop:{stop:0.00}" : "Start bet [Client] : " + bet);

            //}
            SendMsg((ushort)SendMsgIdc.PlayRequest, new PlayRequest());
        }

        public void GetBalance()
        {
            SendMsg((ushort)SendMsgIdc.GetBalance, new BalanceCreditServer());
        }

        public void SetBetValor(float bet)
        {
            //SendMsg(new SetBet { valor = bet });
            //CanvasManager.Instance.SetBetButtonText($"Bet {bet:0.00}");
        }
        public void AddBonus(float bonus)
        {
            //SendMsg(new AddBonus { valor = bonus });
        }
        #endregion


    }



}
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