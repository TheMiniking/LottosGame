using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

public class GameMine : MonoBehaviour
{
    [SerializeField] Wallet wallet;
    [SerializeField] Slider mine;
    [SerializeField] float timePlays;
    [SerializeField] float downFinal;
    [SerializeField] float downStop;
    [SerializeField] float multResult;
    [SerializeField] float speed = 0.001f;
    [SerializeField] bool winBet = false;

    [SerializeField] Bet atualBet;
    [SerializeField] bool doABet;
    [SerializeField] float valueToBet = 0.001f;
    [SerializeField] string coin;
    [SerializeField] MineState mineState = new MineState();
    [SerializeField] Button bet;
    [SerializeField] TMP_Text betButtonTxt;
    [SerializeField] float stopValue;
    [SerializeField] TMP_Text valueCoin, addressWallet;
    [SerializeField] List<Image> gemPedregulho;
    [SerializeField] List<Sprite> pedras;
    [SerializeField] Material fundo;

    private void Start()
    {
        if (wallet.player.address == "") wallet.StartWalletplusCoin();
        addressWallet.text = wallet.player.address;
        valueToBet = 0.001f;
        coin = "BNB";
        bet.onClick.AddListener(StopAndBet);
        fundo.SetFloat("_Velocity", 0);
        StartCoroutine(MineMachine());

    }

    private void FixedUpdate()
    {
        valueCoin.text = $"{wallet.player.coinBalance.ToString("0.00000000")} {wallet.player.atualCoin}";
        fundo.SetFloat("_Velocity", speed != 0.001f ? 0.5f : 0);
        if (mineState.active)
        {
            downStop = downStop + speed * Time.deltaTime;
            //multResult = CalcularValorProporcional(downStop); // Calcula o valor proporcional a partir do slider
            speed = speed <= 0.01 ? speed+ 0.0005f : speed;
            mine.value = downStop;
            betButtonTxt.text = stopValue ==0 ? $"Stop on : x {downStop*100:0.00}": $"Get you bet X {stopValue * 100:0.00}%";
            if(downStop >= downFinal)
            {
                CheckBetWin();
            }
        }else
        {
            betButtonTxt.text = doABet ? $"Wait Start" : $"Bet on Next :{valueToBet} {coin}";
        }
    }

   
    public IEnumerator MineMachine()
    {
        PrepareMine();
        yield return new WaitForSeconds(timePlays);
        bet.interactable = doABet;
        StarMine();
    }

    public void MakeABet()
    {
        if (wallet.CheckBalance(valueToBet , coin))
        {
            atualBet = new Bet() { coin = coin, value = valueToBet, addressID = wallet.player.address };
            wallet.PayBet(atualBet);
            doABet = true;
        }
    }

    public void StopAndBet()
    {
        if (!mineState.active)
        {
            MakeABet();
        }
        else
        {
            stopValue = downStop;
            winBet = stopValue <= downFinal; 
        }

    }

    public void PrepareMine()
    {
        mineState.active = false;
        int r = Random.Range(0, 100);
        mineState.nextStop = r<=10? Random.Range(0.01f, 0.999f) : r<=20 ? Random.Range(0.01f, 0.2f): Random.Range(0.01f, 0.03f);
        downFinal = mineState.nextStop;
        downStop = 0;
        speed = 0.001f;
        stopValue = 0;
        winBet = false;
        mine.value = 0;
        bet.interactable = true;
        doABet = false;
        gemPedregulho.ForEach(x => { 
            x.sprite = pedras[Random.Range(0, pedras.Count)];
            x.gameObject.SetActive(Random.Range(0,2)==0? true : false);});

    }

    public void StarMine()
    {
        mineState.active = true;
    }

    public void CheckBetWin()
    {
        if (winBet)
        {
            wallet.CalculateBetAddToBalance(atualBet,downStop);
        }
        StartCoroutine(MineMachine());
        
    }
}

public class MineState
{
    public bool active;
    public float nextStop = 0;
}
