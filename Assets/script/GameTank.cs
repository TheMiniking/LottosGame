using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameTank : MonoBehaviour
{
    [SerializeField] Wallet wallet;                     //Carteira de cripto
    [SerializeField] Bet atualBet;                      //Aposta Atual do usuario
    [SerializeField] bool onMove;                       //Mostra se ainda esta no movimento
    [SerializeField] int betBest, betGreat, betNormal;  //Base de configuraçao para chance de usar      [betBest = 15%, betGreat = 35% , betNormal = 50%]

    [SerializeField] float timeDurationTank;            //Tempo de duraçao do tank antes de explodir    [ 0.1f = 1 seg ]
    [SerializeField] float multiplyBetUser;             //Multiplicador que o usuario ve na tela        [ add multiplyBetbyTime 2x a cada segundo ]
    [SerializeField] float multiplyBetByTimer;          //Aumenta o multiplicador de forma constante    [ default 0.1f ]
    [SerializeField] int countdown;                     //Contador de tempo

    [SerializeField] TMP_Text multiplyText;             //Texto que mostra o multiplicador do usuario   [ default 1x ]
    [SerializeField] Material fundo;
    [SerializeField] float fundoRealtimeVelocity;
    [SerializeField] float fundoRealtimeAtualPosition;
    [SerializeField] bool fundoOnMove;
    [SerializeField] float timeline = 0.01f;

    [SerializeField] TextMeshProUGUI balance;
    [SerializeField] TextMeshProUGUI timer_multiply;
    [SerializeField] TextMeshProUGUI multiply2;
    [SerializeField] Player tank;
    [SerializeField] ElementMove map;

    [SerializeField] List<float> lastRounds = new List<float>();
    [SerializeField] List<GameObject> lastRoundsOBJ = new List<GameObject>();

    [SerializeField] GameObject boxPrefab;
    [SerializeField] int initBox;
    List<Transform> currentBox = new List<Transform>();
    List<bool> boxOpening = new List<bool>();

    private void Start()
    {
        if (wallet.player.address == "") wallet.StartWalletplusCoin();
        onMove = true;
        fundo.SetInt("_UseScriptTime", 1);
        StartCoroutine(CountDownStart());
        StartCoroutine(TankInicialize());
        StartCoroutine(GameStart());
        Application.targetFrameRate = 60;
    }
    int flameCount = 0;
    void FixedUpdate()
    {
        flameCount++;
        fundoRealtimeAtualPosition = fundoOnMove ? fundoRealtimeAtualPosition + fundoRealtimeVelocity : fundoRealtimeAtualPosition;
        fundo.SetFloat("_RealTimeUpdate", fundoRealtimeAtualPosition);
        
    }

    //IEnumerator GameStart()
    //{
    //    int num = 15;
    //    float crash = 0;
    //    float multiply = 1;
    //    while (true)
    //    {
    //        
    //        else
    //        {
    //            if (multiply == 1)//init run
    //            {
    //                map.Init(true);
    //                tank.Walking(true);
    //            }
    //            var c = Random.Range(crash, 101);
    //            Debug.Log(crash + " chance = " + c);
    //            if (c >= 100)
    //            {
    //                Debug.Log("Crash");
    //                num = 15;
    //                tank.Crash(true);
    //                map.Stop();
    //                yield return new WaitForSeconds(5);
    //                tank.Crash(false);
    //                tank.Walking(false);
    //                map.Init(false);
    //                multiply = 1;
    //                crash = 0;
    //            }
    //            else
    //            {
    //                timer_multiply.text = multiply.ToString("f2") + "x";
    //                multiply2.text = multiply.ToString("f2") + "x";

    //                crash += Random.Range(0.1f, 1f);
    //                multiply += Random.Range(0.051f, 0.1f);
    //                if (Random.Range(1, 20) == 1)
    //                {
    //                    map.InstantiateBox();
    //                }
    //            }
    //        }

    //        yield return new WaitForSeconds(0.3f);
    //    }
    //}

    void TankStart()
    {
        multiplyBetByTimer = 0.01f;
        multiplyBetUser = 1.0f;
        timeline = 0.01f;
        fundoRealtimeVelocity = 0.05f;
        countdown = 10;
    }

    IEnumerator TankInicialize()
    {
        TankStart();
        var luck = Random.Range(0, 101);
        var range = luck <= betBest ? Random.Range(0.02f, 5f) : luck <= betGreat ? Random.Range(0.02f, 2f) : Random.Range(0.02f, 0.2f);
        range = float.Parse($"{range:0.00}");
        timeDurationTank = range;
        Debug.Log($"Sorte numero:{luck}, Duraçao :{timeDurationTank}");
        lastRoundsOBJ.ForEach(x => x.SetActive(false));
        lastRounds.ForEach(x => {
            lastRoundsOBJ[lastRounds.IndexOf(x)].gameObject.SetActive(true);
            lastRoundsOBJ[lastRounds.IndexOf(x)].GetComponentInChildren<TextMeshProUGUI>().text = $"x {x:0.00}";
            lastRoundsOBJ[lastRounds.IndexOf(x)].GetComponent<Image>().color = x <= 2? new Color(1f, 0f, 0f, 0.5f) : x<=10? new Color(0f, 1f, 0f, 0.5f) : new Color(0f,0f,1f, 0.5f);
        });
        yield return new WaitForSecondsRealtime(countdown);
        onMove = true;
        tank.Walking(true);

    }

    IEnumerator GameStart()
    {
        while (true)
        {   
            if (onMove && countdown<0)
            {
                fundoOnMove = true;
                timeline += 0.01f;
                timeline = float.Parse($"{timeline:0.00}");
                multiplyBetUser += multiplyBetByTimer;
                multiplyBetUser = float.Parse($"{multiplyBetUser:0.00}");
                multiplyText.text = $"x {multiplyBetUser:0.00}";
                if (flameCount % 20 == 0)
                {
                    multiplyBetByTimer += 0.01f;
                    fundoRealtimeVelocity += 0.01f;
                }
                if (timeline >= timeDurationTank)
                {
                    lastRounds.Add(multiplyBetUser);
                    if (lastRounds.Count >= 6) lastRounds.RemoveAt(0);
                    tank.Crash(true);
                    tank.Walking(false);
                    onMove = false;
                    fundoOnMove = false;
                    StartCoroutine(TankInicialize());
                }
            }
            //else { Debug.Log("Nao esta em movimento"); }
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator CountDownStart()
    {
        while (true)
        {
            if (countdown > -1)
            {
                timer_multiply.text = countdown.ToString("00:00");
                countdown--;
            }
            yield return new WaitForSecondsRealtime(1f);
        }
    }
    IEnumerator Open(Transform box)
    {
        var b = box.GetComponent<Animator>();
        b.SetBool("open", true);
        yield return new WaitForSecondsRealtime(1);
        int id = currentBox.FindIndex(b => b == box);
        Destroy(box.gameObject);
        boxOpening.RemoveAt(id);
        currentBox.RemoveAt(id);
    }

    public void InstantiateBox()
    {
        var b = Instantiate(boxPrefab).transform;
        b.gameObject.SetActive(true);
        b.position = new Vector3(initBox, -149);
        currentBox.Add(b);
        boxOpening.Add(false);
    }

}
