using UnityEngine;

public class WebClient : WebClientBase
{
    [SerializeField] string url;
    [SerializeField] string token;
    protected override void Start()
    {
        base.Start();
        RegisterHandler<MensageExemple>(Exemple);
        CreateConnection(url, token);
        TryConnect();
    }
    void Exemple(MensageExemple msg)
    {
        Debug.Log($"mensagem recebida do servidor: v1= {msg.value1}, v2= {msg.value2}");
    }
    protected override void OnOpen()
    {
        base.OnOpen();
        SendMsg(new MensageExemple { value1 = "a", value2 = 111 });
    }
    protected override void OnClose(int closeCode)
    {
        base.OnClose(closeCode);
        TryConnect();
    }
}
