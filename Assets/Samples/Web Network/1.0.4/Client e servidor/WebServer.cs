using UnityEngine;

public class WebServer : WebServerBase
{
    [SerializeField] string validToken;
    WebSession session;
    private void Awake()
    {
        StartServer();
    }
    protected override void Start()
    {
        base.Start();
        RegisterHandler<MensageExemple>(Exemple, false, true);
    }
    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.C))
            Close();
        if (Input.GetKeyDown(KeyCode.S))
            StartServer();
        if (Input.GetKeyDown(KeyCode.M))
            session.SendMsg(new MensageExemple { value1 = "oi cliente", value2 = 2});

    }

    public override void OnConnectd(WebSession session)
    {
        this.session = session;
        base.OnConnectd(session);
    }
    public override bool ValideToken(string token)
    {
        return true;
    }

    void Exemple(WebSession session, MensageExemple msg)
    {
        Debug.Log($"mensagem recebida do cliente: {session.ID} v1= {msg.value1}, v2= {msg.value2}");
    }
}
