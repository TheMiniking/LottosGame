using UnityEngine;
using NativeWebSocket;
using System.Collections;
using System.Collections.Generic;
using Serializer;
using System;
using UnityEngine.Events;

public delegate void WebNetworkMessage(byte[] data);
/// <summary>
/// Crie um script que herde deste script para usar como cliente e se conectar com servidore websocket, chame CreateConnection na primeira vez e use TryConnect para estabelecer uma conexao.
/// </summary>
public abstract class WebClientBase : MonoBehaviour
{
    [SerializeField] bool logs = true;
    WebSocket websocket;
    Dictionary<ushort, WebNetworkMessage> handlers = new Dictionary<ushort, WebNetworkMessage>();
    [SerializeField] UnityEvent BeguinTryConnect;
    [SerializeField] UnityEvent<bool> EndTryConnect;
    bool created;
    protected virtual void Start()
    {
        BeguinTryConnect.AddListener(OnBeginTryConnect);
        EndTryConnect.AddListener(OnEndTryConnect);
    }

    protected virtual void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
            websocket.DispatchMessageQueue();
#endif
    }
    /// <summary>
    /// Limpa a conexao e os eventos.
    /// </summary>
    protected void ClearConection()
    {
        websocket = null;
        websocket.OnOpen -= OnOpen;
        websocket.OnClose -= OnClose;
        websocket.OnError -= OnError;
        websocket.OnMessage -= OnMessage;
        created = false;
        if (logs) Debug.Log("[Client] ClearConection");
    }
    /// <summary>
    /// Cria e configura os eventos da conexao com url e token
    /// </summary>
    /// <param name="url">Url para xonexao, exemplo ws://localhost:8080</param>
    /// <param name="token">token é usado pro servidor confirmar a conexão</param>
    protected void CreateConnection(string url, string token)
    {
        if (created)
        {
            if (logs)
                Debug.LogError("[Client] A conexão ja foi criada, chame ClearConection se deseja criar uma nova conexão.");
            return;
        }

        websocket = new WebSocket(url + "?token=" + token);
        websocket.OnOpen += OnOpen;
        websocket.OnClose += OnClose;
        websocket.OnError += OnError;
        websocket.OnMessage += OnMessage;
        created = true;
        if (logs) Debug.Log("[Client] Connection created!");
    }
    /// <summary>
    /// Cria e configura os eventos da conexao com url simpes
    /// </summary>
    /// <param name="url">Url para xonexao, exemplo ws://localhost:8080</param>
    protected void CreateConnection(string url)
    {
        if (created)
        {
            if (logs)
            Debug.LogError("[Client] A conexão ja foi criada, chame ClearConection se deseja criar uma nova conexão.");
            return;
        }
        websocket = new WebSocket(url);
        websocket.OnOpen += OnOpen;
        websocket.OnClose += OnClose;
        websocket.OnError += OnError;
        websocket.OnMessage += OnMessage;
        created = true;
        if (logs) Debug.Log("[Client] Connection created!");
    }
    /// <summary>
    /// Tenta se conectar com o servidor
    /// </summary>
    /// <param name="trycount">numero de tentativas de conexao, -1 tentar ate conseguir.</param>
    public void TryConnect(int trycount = -1)
    {
        if (!created)
        {
            if (logs) 
            Debug.LogError("[Client] A conexão não foi criada, chame CreateConnection para criar.");
            return;
        }
        BeguinTryConnect.Invoke();
        StopAllCoroutines();
        StartCoroutine(ConnectAsync(trycount));
    }
    IEnumerator ConnectAsync(int trycount)
    {
        while (websocket.State != WebSocketState.Open)
        {
            if (websocket.State != WebSocketState.Connecting)
            {
                yield return websocket.Connect();
                yield return new WaitForSeconds(4);

                if (trycount != -1 && trycount == 0)
                    break;
                else trycount--;
            }
            else
            {
                yield return new WaitForSeconds(1);
            }
        }
        EndTryConnect.Invoke(websocket.State == WebSocketState.Open);
    }
    /// <summary>
    /// Registra um metodo que sera chamado de acordo com as msg que o servidor enviar
    /// </summary>
    /// <typeparam name="T">classe que sera montada quando a mensagem for recebida</typeparam>
    /// <param name="handler">Metodo que seja chamado</param>
    /// <param name="msgID">id da mensagem, se deixado em branco o ide sera criado apartir do nome da classe</param>
    public void RegisterHandler<T>(Action<T> handler, short msgID = -1) where T : INetSerializable, new()
    {
        ushort msgType = 0;
        if (msgID == -1)
            msgType = (ushort)(typeof(T).FullName.GetHashCode() & 0xFFFF);
        else msgType = (ushort)msgID;

        if (handlers.ContainsKey(msgType))
        {
            Debug.LogWarning($"[Client] replacing handler for {typeof(T).FullName}, id={msgType}. If replacement is intentional, use ReplaceHandler instead to avoid this warning.");
        }
        if(logs)
        Debug.Log($"[Client] handler registred {typeof(T)} id {msgType}");
        handlers[msgType] = (data) =>
        {
            //Debug.Log(" TryInvokeHandler handlers[msgType] ");
            if (Serializer.Serializer.NetDeserialize(data, 3, out T msg))
            {
                if (logs) Debug.Log(" TryInvokeHandler NetDeserialize " + msg.GetType());
                handler(msg);
            }
            else
            {
                Debug.LogError(" TryInvokeHandler NetDeserialize error ");
            }
        };
    }
    bool TryInvokeHandler(byte[] data)
    {
        ushort msgType = BitConverter.ToUInt16(data, 0);
        if(logs)
        Debug.LogError($"[Client] TryInvokeHandler message ID {msgType} Length {data.Length}" );
        if (handlers.TryGetValue(msgType, out WebNetworkMessage msgDelegate))
        {
            if (msgDelegate == null)
            {
                handlers.Remove(msgType);
                return false;
            }
            //Debug.LogError("TryInvokeHandler msgDelegate invoke " + msgType);
            msgDelegate(data);
            return true;
        }
        if (logs)
            Debug.Log("[Client] Unknown message ID " + msgType + " " + this + ". May be due to no existing RegisterHandler for this message.");
        return false;
    }
    protected void SendMsg<T>(T msg) where T: INetSerializable
    {
        var data = msg.GetData();
        if (logs)
        Debug.Log("SendMsg "+data.Length);
        websocket.Send(data);
    }
    protected async void Disconnect()
    {
        await websocket.Close();
    }
    protected void SendData(byte[] data)
    {
        websocket.Send(data);
    }
    protected virtual void OnMessage(byte[] data)
    {
        TryInvokeHandler(data);
    }
    protected virtual void OnError(string errorMsg)
    {
        if (logs)
            Debug.Log("[Client] OnError " + errorMsg);
    }
    protected virtual void OnClose(int closeCode)
    {
        if (logs)
            Debug.Log("[Client] OnClose " + closeCode);
    }
    protected virtual void OnOpen()
    {
        if (logs)
            Debug.Log("[Client] OnOpen");

    }
    /// <summary>
    /// Metodo chamado quando tentativa de conexao é iniciado.
    /// </summary>
    protected virtual void OnBeginTryConnect()
    {

    }/// <summary>
     /// Metodo chamado quando tentativa de conexao for concluido, retornado true se for bem sucedida.
     /// </summary>
    protected virtual void OnEndTryConnect(bool status)
    {

    }
}
