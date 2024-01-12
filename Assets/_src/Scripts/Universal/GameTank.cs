using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameTank : MonoBehaviour
{
    [SerializeField] Wallet wallet;                     //Carteira de cripto
    [SerializeField] Bet atualBet;                      //Aposta Atual do usuario
    [SerializeField] string coin;                       //Moeda usada para aposta
    [SerializeField] bool onMove;                       //Mostra se ainda esta no movimento
    [SerializeField] int betBest, betGreat, betNormal;  //Base de configura�ao para chance de usar      [betBest = 15%, betGreat = 35% , betNormal = 50%]
    [SerializeField] Player tank;
    [SerializeField] TMP_Text walletBalanceTxt, walletAddressTxt;

    [SerializeField] List<float> lastRounds = new List<float>();
    [SerializeField] List<GameObject> lastRoundsOBJ = new List<GameObject>();

    [SerializeField] float timeDurationTank;            //Tempo de dura�ao do tank antes de explodir    [ 0.1f = 1 seg ]
    [SerializeField] float multiplyBetUser;             //Multiplicador que o usuario ve na tela        [ add multiplyBetbyTime 2x a cada segundo ]
    [SerializeField] float bonusOpenPlayer;
    [SerializeField] float multiplyBetByTimer;          //Aumenta o multiplicador de forma constante    [ default 0.1f ]
    [SerializeField] int countdown;                     //Contador de tempo
    [SerializeField] GameObject boxPrefab;
    [SerializeField] int initBox;
    [SerializeField] List<BoxTank> boxT = new List<BoxTank>();

    [SerializeField] TMP_Text multiplyText;             //Texto que mostra o multiplicador do usuario   [ default 1x ]
    [SerializeField] TextMeshProUGUI balance;
    [SerializeField] TextMeshProUGUI timer_multiply;
    [SerializeField] TextMeshProUGUI multiply2;

    [SerializeField] float timeline = 0.01f;
    [SerializeField] Material fundo;
    [SerializeField] float fundoRealtimeVelocity;
    [SerializeField] float fundoRealtimeAtualPosition;
    [SerializeField] bool fundoOnMove;

    [SerializeField] Toggle autoStopToggle;             //Toggle para usar autoStop
    [SerializeField] bool doABet, autoStop;              //Marca se apostou nessa rodada e se esta usando Auto Stop
    [SerializeField] float stopValue, stopValBet;      //Valor usado para parar a aposta e guardar o valor dela. AutoStop para parar auto.
    [SerializeField] float valueToBet = 1f;             //Valor Base usaado para aposta Min:1f Max: Wallet Balance
    [SerializeField] Button bet;                        //Botao de aposta
    [SerializeField] TMP_Text betButtonTxt, betValTex, autoStopTxt, winnersFromRoundTxt, roundTxt, valBonusTxt;             //texto do botao de aposta, total a apostar e onde parar
    [SerializeField] bool winBet = false;               //Marca se o jogador ganhou a aposta atual
    [SerializeField] float stopBet;
    [SerializeField] List<Button> stopBetListButton, betListButtonUpDown = new List<Button>();//Botoes para modificar apostas e autoStop
    [SerializeField] int flameCount = 0;
    [SerializeField] public List<Bet> roundBets = new List<Bet>();       // Apostas da rodada de outros jogadores
    [SerializeField] public List<Bet> roundWinnerBets = new List<Bet>();       // Apostas da rodada de outros jogadores
    [SerializeField] List<GameObject> roundListObj = new List<GameObject>();// Ui apostas de outros jogadores
    [SerializeField] float velBonus = 0.1f;
    [SerializeField] Vector3 direcaoBonus = new Vector3(-1, 0, 0);
    [SerializeField] float bonusPosInicial;
    [SerializeField] int bonusDitancia;
    [SerializeField] List<float> bonusList = new List<float>();
    [SerializeField] int ind = 0;

    [SerializeField] RTPTank rtp = new RTPTank();

    private void Start()
    {
        if (wallet.player.address == "") wallet.StartWalletplusCoin();
        else wallet.UpdateAtualCoin("C");
        coin = wallet.player.atualCoin;
        stopBetListButton.ForEach(x => x.onClick.RemoveAllListeners());
        betListButtonUpDown.ForEach(x => x.onClick.RemoveAllListeners());
        bet.onClick.AddListener(StopAndBet);
        betListButtonUpDown[0].onClick.AddListener(() => UpDownBetAmount(1f, true, 1));
        betListButtonUpDown[1].onClick.AddListener(() => UpDownBetAmount(1f, false, 1));
        betListButtonUpDown[2].onClick.AddListener(() => UpDownBetAmount(0f, true, 2));
        betListButtonUpDown[3].onClick.AddListener(() => UpDownBetAmount(0f, false, 2));
        betListButtonUpDown[4].onClick.AddListener(() => UpDownBetAmount(0f, true, 3));
        betListButtonUpDown[5].onClick.AddListener(() => UpDownBetAmount(0f, false, 3));
        stopBetListButton[0].onClick.AddListener(() => UpDownAutoStop(0.1f, true));
        stopBetListButton[1].onClick.AddListener(() => UpDownAutoStop(0.1f, false));
        stopBetListButton[2].onClick.AddListener(() => UpDownAutoStop(1f, true));
        stopBetListButton[3].onClick.AddListener(() => UpDownAutoStop(1f, false));
        fundo.SetInt("_UseScriptTime", 1);
        stopValBet = 1.01f;
        StartCoroutine(CountDownStart());
        StartCoroutine(TankInicialize());
        StartCoroutine(GameStart());
        Application.targetFrameRate = 60;
    }
    void FixedUpdate()
    {
        flameCount++;
        fundoRealtimeAtualPosition = fundoOnMove ? fundoRealtimeAtualPosition + fundoRealtimeVelocity : fundoRealtimeAtualPosition;
        fundo.SetFloat("_RealTimeUpdate", fundoRealtimeAtualPosition);
        bet.interactable = onMove ? winBet ? false : doABet : true;
        betButtonTxt.text = onMove ? doABet ? winBet ? $"You win: x{stopValue:0.00}" : $"Stop : x{multiplyBetUser:0.00}" : "Wait Next Round" : doABet ? "Wait Start" : $"Bet : {valueToBet:0.00}";
        betValTex.text = $"{valueToBet:0.00}";
        autoStopTxt.text = $"{stopValBet:0.00}";
        walletAddressTxt.text = wallet.player.address;
        walletBalanceTxt.text = $" {wallet.player.coinBalance:0.00} {wallet.player.atualCoin}";
        winnersFromRoundTxt.text = countdown < 0 ? "Winners :" : "Players Bets";
        roundTxt.text = countdown < 0 ? "Multiply your Bet :" : "Next Round in:";
        if(boxT.Count > 0 )boxT.ForEach(x =>{x.currentBox.gameObject.transform.position += ((direcaoBonus * velBonus) * (fundoRealtimeVelocity * (countdown < 0 ? 1 : 0)));});
        valBonusTxt.text = $"x {bonusOpenPlayer:0.0}";
    }

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
        if(multiplyBetUser != 1f) Debug.Log("Final x" + multiplyBetUser);
        TankStart();
        var luck = UnityEngine.Random.Range(0, 101);
        var range = luck <= betBest ? UnityEngine.Random.Range(0.02f, 5f) : luck <= betGreat ? UnityEngine.Random.Range(0.02f, 2f) : UnityEngine.Random.Range(0.02f, 0.2f);
        range = float.Parse($"{range:0.00}");
        timeDurationTank = range;
        Debug.Log($"Sorte numero:{luck}, Duracao :{timeDurationTank}, Final X{rtp.RoundPlay(timeDurationTank, true , 10)}");
        lastRoundsOBJ.ForEach(x => x.SetActive(false));
        lastRounds.ForEach(x =>
        {
            lastRoundsOBJ[lastRounds.IndexOf(x)].gameObject.SetActive(true);
            lastRoundsOBJ[lastRounds.IndexOf(x)].GetComponentInChildren<TextMeshProUGUI>().text = $"x {x:0.00}";
            lastRoundsOBJ[lastRounds.IndexOf(x)].GetComponent<Image>().color = x <= 2 ? new Color(1f, 0f, 0f, 0.5f) : x <= 10 ? new Color(0f, 1f, 0f, 0.5f) : new Color(0f, 0f, 1f, 0.5f);
        });
        doABet = false;
        bet.interactable = true;
        atualBet = new Bet();
        winBet = false;
        roundListObj.ForEach(x =>
        {
            x.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.4f);
            x.SetActive(false);
        });
        roundBets.Clear();
        roundWinnerBets.Clear();
        boxT.ForEach(x => GameObject.Destroy(x.currentBox.gameObject));
        boxT.Clear();
        bonusOpenPlayer = 0;
        yield return new WaitForSecondsRealtime(countdown);
        onMove = true;
        tank.Walking(true);

    }

    IEnumerator GameStart()
    {
        int d = 0;
        while (true)
        {
            if (onMove && countdown < 0)
            {
                fundoOnMove = true;
                AddtoUserX();
                multiplyText.text = $"x {multiplyBetUser:0.00}";
                roundBets.ForEach(x =>
                {
                    if (multiplyBetUser >= x.stop && !x.winBet)
                    {
                        x.winBet = true;
                        if (roundWinnerBets.Count < roundListObj.Count) roundWinnerBets.Add(x);
                    }
                });
                roundListObj.ForEach(x => x.gameObject.SetActive(false));
                roundWinnerBets.ForEach(x =>{
                    var y = roundWinnerBets.IndexOf(x);
                    roundListObj[y].gameObject.SetActive(true);
                    roundListObj[y].transform.Find("Bet").GetComponent<TMP_Text>().text = $"{x.value * x.stop:0.00} {x.coin}";
                    roundListObj[y].transform.Find("Address").GetComponent<TMP_Text>().text = $"{x.addressID}";
                    roundListObj[y].GetComponent<Image>().color = new Color(0f, 1f, 0f, 0.4f);
                });
                if(d != bonusDitancia) { d++; }
                else{d = 0; InstantiateBox();}
                boxT.ForEach(x =>{ if (x.currentBox.position.x <= tank.transform.position.x) StartCoroutine(Open(x.currentBox));});
                if (flameCount % 20 == 0)
                {
                    //multiplyBetByTimer += 0.01f;
                    fundoRealtimeVelocity = fundoRealtimeVelocity == 0.2f? fundoRealtimeVelocity : fundoRealtimeVelocity + 0.01f;
                }
                if (doABet && atualBet.autoStop && !winBet)
                {
                    if (multiplyBetUser >= atualBet.stop)
                    {
                        stopValue = multiplyBetUser;
                        winBet = onMove;
                    }
                }
                if (timeline >= timeDurationTank)
                {
                    lastRounds.Add(multiplyBetUser);
                    if (lastRounds.Count >= 1 + lastRoundsOBJ.Count) lastRounds.RemoveAt(0);
                    CheckBetWin();
                    yield return new WaitForSeconds(3);
                    wallet.UpdateAtualCoin(coin);
                }
            }
            else
            {
                MakeFakeBets();
                MakeFakeBets();
            }
            //else { Debug.Log("Nao esta em movimento"); }
            yield return new WaitForSeconds(multiplyBetUser <= 2 ? 0.3f : multiplyBetUser <= 5 ? 0.2f : 0.1f);
        }
    }

    void AddtoUserX() {
        timeline += 0.01f;
        timeline = float.Parse($"{timeline:0.00}");
        multiplyBetByTimer = ind == 20 ? multiplyBetByTimer + 0.01f : multiplyBetByTimer;
        ind = ind == 20 ? 0 : ind + 1;
        multiplyBetUser += multiplyBetByTimer;
        multiplyBetUser = float.Parse($"{multiplyBetUser:0.00}");
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
            timer_multiply.color = countdown == -1 ? Color.white : countdown <= 4 ? Color.red : Color.white;
            yield return new WaitForSecondsRealtime(1f);
        }
    }
    IEnumerator Open(Transform box)
    {
        var b = box.GetComponent<Animator>();
        b.SetBool("open", true);
        int id = boxT.FindIndex(b => b.currentBox == box);
        yield return new WaitForSecondsRealtime(boxT[id].bonus != 0? 0.5f :0.1f);
        if (boxT[id].bonus != 0)
        {
            bonusOpenPlayer += boxT[id].bonus;
            Destroy(boxT[id].currentBox.gameObject);
            boxT.RemoveAt(id);
        }
        else
        {
            Debug.Log("Box Explosiva");
            lastRounds.Add(multiplyBetUser);
            if (lastRounds.Count >= 1 + lastRoundsOBJ.Count) lastRounds.RemoveAt(0);
            Destroy(boxT[id].currentBox.gameObject);
            CheckBetWin();
            wallet.UpdateAtualCoin(coin);
        }
    }

    public void InstantiateBox()
    {
        var b = Instantiate(boxPrefab).transform;
        b.gameObject.SetActive(true);
        b.position = new Vector3(initBox + bonusPosInicial, -149);
        var bb = bonusList[UnityEngine.Random.Range(0, bonusList.Count)];
        boxT.Add(new BoxTank { currentBox = b, boxOpening = false, bonus = bb });
        b.Find("Text (TMP)").GetComponent<TMP_Text>().text =bb==0?"BOMB": $"x {bb}";
    }
    public void MakeABet()
    {
        autoStop = autoStopToggle.isOn;
        if (wallet.CheckBalance(valueToBet, coin) && !doABet)
        {
            atualBet = autoStop ? new Bet() { coin = coin, value = valueToBet, addressID = wallet.player.address, stop = stopValBet, autoStop = true } :
                new Bet() { coin = coin, value = valueToBet, addressID = wallet.player.address, autoStop = false };
            wallet.PayBet(atualBet);
            doABet = true;
        }
    }// Cria uma aposta se ouver saldo suficiente

    public void StopAndBet()
    {
        if (!onMove && !doABet)
        {
            MakeABet();
        }
        else
        {
            stopValue = multiplyBetUser + bonusOpenPlayer;
            winBet = onMove;
        }

    }// Botao de Aposta e parar Aposta

    public void UpDownAutoStop(float toUpVal, bool toUp)
    {
        stopValBet = toUp ? stopValBet + toUpVal : stopValBet - toUpVal;
        stopValBet = stopValBet < 1.01 ? 1.01f : stopValBet;
    }// Controle de valores, para onde parar automaticamente

    public void UpDownBetAmount(float toUpVal, bool toUp, int sp)
    {

        if (toUp) valueToBet = sp == 1 ? valueToBet + toUpVal : sp == 2 ? valueToBet * 2 : wallet.player.coinBalance;
        else valueToBet = sp == 1 ? valueToBet - toUpVal : sp == 2 ? valueToBet / 2 : 1f;
        valueToBet = valueToBet < 1 ? 1f : valueToBet > wallet.player.coinBalance ? wallet.player.coinBalance : valueToBet;
    }//Controle de Valores da aposta

    public void CheckBetWin()
    {

        tank.Crash(true);
        tank.Walking(false);
        onMove = false;
        fundoOnMove = false;
        if (winBet)
        {
            wallet.CalculateBetAddToBalance(atualBet, stopValue);
        }
        StartCoroutine(TankInicialize());


    }// Finaliza a rodada e checa se a aposta ganhou ou perdeu.Apos isso Reinicializa tudo novamente


    public void MakeFakeBets()
    {
        var d = UnityEngine.Random.Range(0, 2);
        if (d == 0)
        {
            roundBets.Add(new Bet() { coin = "C", value = UnityEngine.Random.Range(1f, 100f), addressID = $"Player{UnityEngine.Random.Range(0, 99999)}", stop = (float)Math.Round(UnityEngine.Random.Range(1.01f, 20f), 2) });
            if (roundBets.Count <= roundListObj.Count)
            {
                roundListObj[roundBets.Count - 1].SetActive(true);
                roundListObj[roundBets.Count - 1].transform.Find("Bet").GetComponent<TMP_Text>().text = $"{roundBets[roundBets.Count - 1].value:0.00} {roundBets[roundBets.Count - 1].coin}";
                roundListObj[roundBets.Count - 1].transform.Find("Address").GetComponent<TMP_Text>().text = $"{roundBets[roundBets.Count - 1].addressID}";
            }
        }
    }// Para proposito de testes : Simula apostas feitas por outro jogadores

}
[Serializable]
public class BoxTank
{
    public Transform currentBox;
    public bool boxOpening;
    public float bonus;
}

