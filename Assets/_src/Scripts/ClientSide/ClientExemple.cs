using GameSpawner;
using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ClientExemple : WebClientBase
{
    [SerializeField] string url;
    [SerializeField] string token;
    [SerializeField] TextMeshProUGUI value;
    protected override void Start()
    {
        base.Start();

        RegisterHandler<GameLoginResponse>(GameLoginResponse);
        RegisterHandler<BalanceResponse>(BalanceResponse);
        RegisterHandler<PlayResponse<MathStatus>>(PlayResponse, 35559);
        CreateConnection(url, token);
        TryConnect();
    }
    protected override void OnOpen()
    {
        base.OnOpen();
        SendMsg(new GameLogin { token = token });
    }
    void GameLoginResponse(GameLoginResponse msg)
    {
        Debug.Log("LogResponse:" + msg.code);
    }

    void BalanceResponse(BalanceResponse msg)
    {
        Debug.Log("BalanceResponse:" + msg.balance);
    }

    void PlayResponse(PlayResponse<MathStatus> msg)
    {
        Debug.Log($"PlayResponse: id: {msg.data.id} value : {msg.data.value} ");
        if (msg.data.id == 0)
        {
            StartCoroutine(DisplayTimer(msg.data.value));
        }
        else
        if (msg.data.id == 1)
        {
            StopAllCoroutines();
            StartCoroutine(DisplayMulti(msg.data.value));
        }
        else if (msg.data.id == 2)
        {
            StopAllCoroutines();
            Debug.Log("Crash " + msg.data.value);
            value.text = msg.data.value.ToString("f2") + "x";
        }
    }
    IEnumerator DisplayTimer(float time)
    {
        while (time > 0)
        {
            time--;
            value.text = time.ToString("00:00");
            yield return new WaitForSeconds(1);
        }
    }
    IEnumerator DisplayMulti(float multiSum)
    {
        float multiplier = 1;
        float delay = 0.15f;
        float delayServer = 0.5f;
        float adjustmentFactor = delayServer / delay;
        while (true)
        {
            multiSum += multiSum *(.1f* delay* adjustmentFactor);
            multiplier += multiSum;

            value.text = multiplier.ToString("f2") + "x";
            yield return new WaitForSeconds(delay);
        }
    }
}
