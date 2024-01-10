using Serializer;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public delegate void WebMessageDelegate(WebSession session, byte[] data);
/// <summary>
/// Cre um script que herde WebServerBase para se conectar com clientes, use StartServer para começar a receber as conexões
/// </summary>
public abstract class WebServerBase : MonoBehaviour
{
    [SerializeField] int port = 8080;
    [SerializeField] bool logs = true;
    WebSocketServer server;
    Dictionary<ushort, WebMessageDelegate> handlers = new Dictionary<ushort, WebMessageDelegate>();
    protected virtual void Start()
    {
    }
    protected virtual void Update()
    {
    }

    /// <summary>
    /// Fecha o servidor
    /// </summary>
    protected void Close()
    {
        server.Stop(); 
        if (logs)
            Debug.Log("[server] server stoped");
    }
    /// <summary>
    /// Inicia o servidor
    /// </summary>
    protected void StartServer()
    {
        server = new WebSocketServer(IPAddress.Any, port); // Define o endereço do servidor
        server.AddWebSocketService<WebSession>("/", s => s.Server = this); // Adiciona um comportamento para lidar com mensagens
        server.Start();
        if (logs)
            Debug.Log("[server] server initialized");
    }
    /// <summary>
    /// Registra um metodo que sera chamada quando um cliente enviar uma mensagem.
    /// </summary>
    /// <typeparam name="T">Classe que sera montada com a mensagem</typeparam>
    /// <param name="handler">Metodo que sera chamado</param>
    /// <param name="needAuthenticated">se o cliente precisa se autenticar primeiro</param>
    /// <param name="mainThreadForce">se o metodo deve rodar na main thread, no webgl sempre vai rodar no main tread</param>
    /// <param name="msgID">id da mensagem, se deixado em branco, usara o nome da classe com id</param>
    protected void RegisterHandler<T>(Action<WebSession, T> handler, bool needAuthenticated = true, bool mainThreadForce = false,int msgID =-1) where T : INetSerializable, new()
    {
        ushort msgType = 0;
        if (msgID == -1)
            msgType = (ushort)(typeof(T).FullName.GetHashCode() & 0xFFFF);
        else msgType = (ushort)msgID;

        if (handlers.ContainsKey(msgType))
        {
            if (logs)
                Debug.LogWarning($"[server] replacing handler for {typeof(T).FullName}, id={msgType}. If replacement is intentional, use ReplaceHandler instead to avoid this warning.");
        }
        if (logs)
            Debug.Log($"[server] handler registred {typeof(T)} id {msgType}");
        handlers[msgType] = (con, data) =>
        {
            if (needAuthenticated && !con.IsAuthenticated)
                return;
            if (Serializer.Serializer.NetDeserialize(data, 3, out T msg))
            {
                try
                {
                    if (mainThreadForce)
                    {
                        Serializer.Dispatcher.Instance.ExecuteInMainThread(() => handler(con, msg));
                    }
                    else
                        handler(con, msg);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[server] Invoke {msg.GetType().Name} Exception: " + e.Message);
                }
            }
        };
    }
    internal bool TryInvokeHandler(WebSession con, byte[] data)
    {
        //Debug.Log("TryInvokeHandler data " + data.Length);
        ushort msgType = BitConverter.ToUInt16(data, 0);
        if (handlers.TryGetValue(msgType, out WebMessageDelegate msgDelegate))
        {
            if (msgDelegate == null)
            {
                handlers.Remove(msgType);
                return false;
            }
            msgDelegate(con, data);
            return true;
        }
        if (logs)
            Debug.Log("[server] Unknown message ID " + msgType + " " + this + ". May be due to no existing RegisterHandler for this message.");
        return false;
    }
    public virtual void OnConnectd(WebSession session)
    {
        if (logs)
            Debug.Log("[server] OnConnectd base");
    }
    public virtual void OnDisconnectd(WebSession session)
    {
        if (logs)
            Debug.Log("[server] OnDisconnectd base");
    }
    /// <summary>
    /// Use para receber e validar um token antes de concluir a conexão.
    /// </summary>
    /// <param name="token">Se retornar true a conexão sera efetuada.</param>
    /// <returns></returns>
    public virtual bool ValideToken(string token)
    {
        return true;
    }
}
public interface IClientInfos { }
[Serializable]
public class WebSession : WebSocketBehavior
{
    public WebServerBase Server { get; set; }
    public bool IsAuthenticated { get; internal set; }
    IClientInfos client;
    public void SetClientInfos(IClientInfos client)
    {
        this.client = client;
    }
    public T GetClient<T>() where T : IClientInfos
    {
        return (T)this.client;
    }
    public void SendMsg<T>(T msg) where T : INetSerializable
    {
        var data = msg.GetData();
        Send(data);
    }
    public void SendData(byte[] data)
    {
        Send(data);
    }
    protected override void OnMessage(MessageEventArgs e)
    {
        Server.TryInvokeHandler(this, e.RawData);
    }
    protected override void OnClose(CloseEventArgs e)
    {
        base.OnClose(e);
        Server.OnDisconnectd(this);
    }
    protected override void OnError(ErrorEventArgs e)
    {
        base.OnError(e);
    }
    protected override void OnOpen()
    {
        var token = this.QueryString["token"];
        if (!Server.ValideToken(token))
        {
            Close(1008, "Invalid token");
            return;
        }
        base.OnOpen();
        Server.OnConnectd(this);
    }
}
