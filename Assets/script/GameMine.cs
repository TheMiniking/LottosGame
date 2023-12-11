using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;
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
    [SerializeField] float stopValue , stopValBet;
    [SerializeField] TMP_Text valueCoin, addressWallet, betTxt, stopValBetTxt;
    [SerializeField] List<Image> gemPedregulho;
    [SerializeField] List<Sprite> pedras;
    [SerializeField] Material fundo;

    [SerializeField] List<float> lastBets;
    [SerializeField] List<GameObject> lastBetTxt;
    [SerializeField] List<Button> stopBetListButton, betListButtonUpDown;
    [SerializeField] Animator coruja;
    [SerializeField] GameObject pedrasPoeira;

    private void Start()
    {
        if (wallet.player.address == "") wallet.StartWalletplusCoin();
        //addressWallet.text = wallet.player.address;
        stopBetListButton.ForEach(x => x.onClick.RemoveAllListeners());
        betListButtonUpDown.ForEach(x => x.onClick.RemoveAllListeners());
        betListButtonUpDown[0].onClick.AddListener(() => UpDownBetAmount(0.001f, true,1));
        betListButtonUpDown[1].onClick.AddListener(() => UpDownBetAmount(0.001f, false,1));
        betListButtonUpDown[2].onClick.AddListener(() => UpDownBetAmount(0f, true, 2));
        betListButtonUpDown[3].onClick.AddListener(() => UpDownBetAmount(0f, false,2));
        betListButtonUpDown[4].onClick.AddListener(() => UpDownBetAmount(0f, true, 3));
        betListButtonUpDown[5].onClick.AddListener(() => UpDownBetAmount(0f, false, 3));
        stopBetListButton[0].onClick.AddListener(()=> UpDownAutoStop(0.1f,true));
        stopBetListButton[1].onClick.AddListener(() => UpDownAutoStop(0.1f, false));
        stopBetListButton[2].onClick.AddListener(() => UpDownAutoStop(1f, true));
        stopBetListButton[3].onClick.AddListener(() => UpDownAutoStop(1f, false));
        valueToBet = 0.001f;
        coin = "BNB";
        bet.onClick.AddListener(StopAndBet);
        pedrasPoeira.SetActive(false);
        lastBetTxt.ForEach(x => {
            if (lastBetTxt.IndexOf(x) <= lastBets.Count - 1)
            {
                x.gameObject.SetActive(true);
                x.GetComponentInChildren<TMP_Text>().text = $"x{lastBets[lastBetTxt.IndexOf(x)]}";
            }
            else
                x.gameObject.SetActive(false);
        });
        StartCoroutine(MineMachine());

    }

    private void FixedUpdate()
    {
        valueCoin.text = $"{wallet.player.coinBalance.ToString("0.00000000")} {wallet.player.atualCoin}";
        fundo.SetFloat("_Velocity", speed != 0.001f ? 0.5f : 0);
        betTxt.text = $"{valueToBet:0.0000} {coin}";
        if(terminouOTempo)uiTimer.text = $"x{downStop * 100:0.00}";
        stopValBetTxt.text = $"x {stopValBet:0.00}";
        if (mineState.active)
        {
            downStop = downStop + speed * Time.deltaTime;
            //multResult = CalcularValorProporcional(downStop); // Calcula o valor proporcional a partir do slider
            speed = speed <= 0.01 ? speed+ 0.0005f : speed;
            mine.value = downStop;
            betButtonTxt.text = stopValue ==0 ? $"Stop Now : x {downStop*100:0.00}": $"Get you bet X {stopValue * 100:0.00}%";
            if(downStop >= downFinal)
            {
                CheckBetWin();
            }
        }else
        {
            betButtonTxt.text = doABet ? $"Wait Start" : $"Bet on Next :{valueToBet} {coin}";
        }
    }

    public void UpDownAutoStop(float toUpVal, bool toUp)
    {
        stopValBet = toUp? stopValBet + toUpVal : stopValBet - toUpVal;
        stopValBet = stopValBet < 1.01 ? 1.01f : stopValBet;
    }

    public void UpDownBetAmount(float toUpVal, bool toUp, int sp)
    {

        if (toUp) valueToBet = sp == 1 ? valueToBet + toUpVal : sp== 2? valueToBet*2 : wallet.player.coinBalance;
        else valueToBet = sp == 1 ? valueToBet - toUpVal : sp == 2 ? valueToBet / 2 : 0.001f;
        valueToBet = valueToBet < 0.001 ? 0.001f : valueToBet > wallet.player.coinBalance ? wallet.player.coinBalance :  valueToBet;
    }

    public IEnumerator MineMachine()
    {
        PrepareMine();
        yield return new WaitForSeconds(timePlays);
        bet.interactable = doABet;
        StarMine();
    }

    private void OnDisable() => fundo.SetFloat("_Velocity", 0);
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
        Inicio((int)timePlays);
    }

    public void StarMine()
    {
        mineState.active = true;
        coruja.Play("Down");
        pedrasPoeira.SetActive(true);
    }

    public void CheckBetWin()
    {
        coruja.Play("Kabum");
        pedrasPoeira.SetActive(false);
        if (winBet)
        {
            wallet.CalculateBetAddToBalance(atualBet,downStop);
        }
        if (lastBets.Count >= 5) lastBets.RemoveAt(0);
        lastBets.Add(float.Parse($"{downFinal*100:0.00}"));
        lastBetTxt.ForEach(x => {
            if (lastBetTxt.IndexOf(x) <= lastBets.Count - 1)
            {
                x.gameObject.SetActive(true);
                x.GetComponentInChildren<TMP_Text>().text = $"x{lastBets[lastBetTxt.IndexOf(x)]}";
                x.GetComponent<Image>().color = lastBets[lastBetTxt.IndexOf(x)] <= 2 ? new Color(1f,0f,0f,0.15f) : lastBets[lastBetTxt.IndexOf(x)] <= 10 ? new Color(0f, 1f, 0f, 0.15f) : new Color(1f, 0.5f, 0f, 0.15f);
            }
            else
                x.gameObject.SetActive(false);
        });
        StartCoroutine(MineMachine());
        
    }

    [SerializeField] Image uiFill;
    [SerializeField] TMP_Text uiTimer;
    public int duration;
    int remaningDuration;
    public bool terminouOTempo;

    public void Inicio(int second)
    {
        terminouOTempo = false;
        remaningDuration = second;
        StartCoroutine(UpdateTime());
    }

    private IEnumerator UpdateTime()
    {
        while (remaningDuration >= 0)
        {
            uiTimer.text = $"{remaningDuration / 60:00} : {remaningDuration % 60:00}";
            uiFill.fillAmount = Mathf.InverseLerp(0, duration, remaningDuration);
            remaningDuration--;
            yield return new WaitForSeconds(1f);
        }
        EndOfTime();
    }

    public void EndOfTime()
    {
        terminouOTempo = true;
    }
}

public class MineState
{
    public bool active;
    public float nextStop = 0;
}
