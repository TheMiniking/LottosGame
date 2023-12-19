using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTank : MonoBehaviour
{
    [SerializeField] Wallet wallet;                     //Carteira de cripto
    [SerializeField] Bet atualBet;                      //Aposta Atual do usuario
    [SerializeField] bool onMove;                       //Mostra se ainda esta no movimento
    [SerializeField] int betBest, betGreat, betNormal;  //Base de configuraçao para chance de usar      [betBest = 15%, betGreat = 35% , betNormal = 50%]

    [SerializeField] float timeDurationTank;            //Tempo de duraçao do tank antes de explodir    [ 0.1f = 1 seg ]
    [SerializeField] float multiplyBetUser;             //Multiplicador que o usuario ve na tela        [ add multiplyBetbyTime 2x a cada segundo ]
    [SerializeField] float multiplyBetByTimer;          //Aumenta o multiplicador de forma constante    [ default 0.1f ]

    [SerializeField] TMP_Text multiplyText;             //Texto que mostra o multiplicador do usuario   [ default 1x ]
    [SerializeField] Material fundo;


    [SerializeField] TextMeshProUGUI balance;
    [SerializeField] TextMeshProUGUI timer_multiply;
    [SerializeField] TextMeshProUGUI multiply2;
    [SerializeField] Player tank;
    [SerializeField] ElementMove map;
    private void Start()
    {
        if (wallet.player.address == "") wallet.StartWalletplusCoin();
        onMove = true;
        //StartCoroutine(TankInicialize());
        StartCoroutine(GameStart());
        Application.targetFrameRate = 60;
    }

    IEnumerator GameStart()
    {
        int num = 15;
        float crash = 0;
        float multiply = 1;
        while (true)
        {
            if (num > 0)
            {
                timer_multiply.text = num.ToString("00:00");
                num--;
            }
            else
            {
                if (multiply == 1)//init run
                {
                    map.Init(true);
                    tank.Walking(true);
                }
                var c = Random.Range(crash, 101);
                Debug.Log(crash + " chance = " + c);
                if (c >= 100)
                {
                    Debug.Log("Crash");
                    num = 15;
                    tank.Crash(true);
                    map.Stop();
                    yield return new WaitForSeconds(5);
                    tank.Crash(false);
                    tank.Walking(false);
                    map.Init(false);
                    multiply = 1;
                    crash = 0;
                }
                else
                {
                    timer_multiply.text = multiply.ToString("f2") + "x";
                    multiply2.text = multiply.ToString("f2") + "x";

                    crash += Random.Range(0.1f, 1f);
                    multiply += Random.Range(0.051f, 0.1f);
                    if (Random.Range(1, 20) == 1)
                    {
                        map.InstantiateBox();
                    }
                }
            }

            yield return new WaitForSeconds(0.3f);
        }
    }
    //private void OnDisable()
    //{
    //    fundo.SetFloat("_Velocity", 0f);
    //}

    //public int i = 0;
    //private void FixedUpdate()
    //{

    //    if (onMove)
    //    {
    //        i++;
    //        var flame = timeline  <= 0.20f? 20: timeline <= 0.1f ? 10 : timeline <= 0.5f ? 5 : 2;
    //        if (i >= flame )
    //        {
    //            timeline += 0.01f;
    //            timeline = float.Parse($"{timeline:0.00}");
    //            multiplyBetUser += multiplyBetByTimer;
    //            multiplyBetUser = float.Parse($"{multiplyBetUser:0.00}");
    //            multiplyText.text = $"x {multiplyBetUser:0.00}";
    //            if (timeline % 0.2f == 0)
    //            {
    //                multiplyBetByTimer += 0.01f;
    //                fundo.SetFloat("_Velocity", fundo.GetFloat("_Velocity")+0.5f);
    //            }
    //            i = 0;
    //        }
    //        if (timeline >= timeDurationTank)
    //        {
    //            Debug.Log("Explodir");
    //            onMove = false;
    //            fundo.SetFloat("_Velocity", 0f);
    //            StartCoroutine(TankInicialize());
    //        }
    //    }
    //}

    //float timeline = 0.01f;
    //IEnumerator TankInicialize()
    //{
    //    TankStart();
    //    var luck = Random.Range(0, 101);
    //    var range = luck <= betBest ? Random.Range(0.02f, 10f): luck <= betGreat? Random.Range(0.02f, 5f): Random.Range(0.02f,0.2f);
    //    range = float.Parse($"{range:0.00}");
    //    timeDurationTank = range;
    //    Debug.Log($"Sorte numero:{luck},Duraçao :{timeDurationTank}");
    //    yield return new WaitForSeconds(2f);
    //    onMove = true;
    //    fundo.SetFloat("_Velocity",0.5f);

    //}

    //void TankStart()
    //{
    //    multiplyBetByTimer = 0.01f;
    //    multiplyBetUser = 1.0f;
    //    timeline = 0.01f;
    //}
}
