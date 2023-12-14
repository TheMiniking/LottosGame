using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTank : MonoBehaviour
{
    [SerializeField]Wallet wallet;
    [SerializeField]TextMeshProUGUI balance;
    [SerializeField]TextMeshProUGUI timer_multiply;
    [SerializeField]TextMeshProUGUI multiply2;
    [SerializeField] Player tank;
    [SerializeField] ElementMove map;
    void Start()
    {
        if (wallet.player.address == "") wallet.StartWalletplusCoin();
        balance.text = wallet.player.coinBalance.ToString();
        StartCoroutine(GameStart());
    }

    IEnumerator GameStart()
    {
        int num = 15;
        float crash = 0;
        float multiply = 1;
        while (true)
        {
            if (num >0)
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
                Debug.Log(crash+ " chance = " +c);
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
                    if (Random.Range(1, 20)==1)
                    {
                        map.InstantiateBox();
                    }
                }
            }

            yield return new WaitForSeconds(0.3f);
        }
    }
}
