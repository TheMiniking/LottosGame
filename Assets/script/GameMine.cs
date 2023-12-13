using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameMine : MonoBehaviour
{
    [SerializeField] Wallet wallet;                     //Carteira de cripto
    [SerializeField] Bet atualBet;                      //Aposta Atual do usuario
    [SerializeField] float timeRoundWait;               //Timer Entre Rodadas
    [SerializeField] bool winBet = false;               //Marca se o jogador ganhou a aposta atual
    [SerializeField] float downFinal;                   //Valor de descida final, onde vai quebrar
    [SerializeField] float downStop;                    //Valor de descida atual,usada para marcar onde parou
    [SerializeField] float speedDownMultiply = 0.001f;  //Multiplicador de velocidade de descida max: 0.01f
    [SerializeField] MineState mineState=new MineState();// Status Atual da mina
    [SerializeField] bool doABet,autoStop;              //Marca se apostou nessa rodada e se esta usando Auto Stop
    [SerializeField] float stopValue , stopValBet;      //Valor usado para parar a aposta e guardar o valor dela. AutoStop para parar auto.

    [SerializeField] float valueToBet = 0.001f;         //Valor Base usaado para aposta Min:0.001f Max: Wallet Balance
    [SerializeField] string coin;                       //Moeda usada para aposta
    [SerializeField] Button bet;                        //Botao de aposta
    [SerializeField] TMP_Text betButtonTxt;             //texto do botao de aposta
    [SerializeField] TMP_Text valueCoin, addressWallet, betTxt, stopValBetTxt;//Textos Ui
    [SerializeField] Material fundo;                    //Material usado para simular a descida

    [SerializeField] List<float> lastBets;              //Ultimas apostas
    [SerializeField] List<GameObject> lastBetTxt = new List<GameObject>() ;
    [SerializeField] List<Button> stopBetListButton, betListButtonUpDown = new List<Button>();//Botoes para modificar apostas e autoStop
    [SerializeField] Animator coruja;                   // Animaçao curuja
    [SerializeField] GameObject pedrasPoeira,gem,explosion;       // Particulas Poeira e gemase explosion
    [SerializeField] float downVelocity;                // Velocidade da animaçao
    public List<Bet> roundBets = new List<Bet>();       // Apostas da rodada de outros jogadores
    [SerializeField] List<GameObject> roundListObj = new List<GameObject>();// Ui apostas de outros jogadores
    [SerializeField] int flamerate = 0;                 // contador Atualizaçao de flames , dedir a velocidade que vai multiplicar 
    [SerializeField] Toggle autoStopToggle;             //Toggle para usar autoStop


    [SerializeField] List<AudioSource> effectsSound = new List<AudioSource>();//Lista de sons
    [SerializeField] AudioSource music;                 //Musica de fundo
    [SerializeField] Toggle musicToggle,effectsToggle;  //Toggle para usar musica e efeitos

    private void Start()
    {
        Application.targetFrameRate = 60;
        if (wallet.player.address == "") wallet.StartWalletplusCoin();
        stopBetListButton.ForEach(x => x.onClick.RemoveAllListeners());
        betListButtonUpDown.ForEach(x => x.onClick.RemoveAllListeners());
        bet.onClick.AddListener(StopAndBet);
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

    }// Inicializa todos itens necesarios

    
    private void FixedUpdate()
    {
        flamerate++;
        valueCoin.text = $"{wallet.player.coinBalance.ToString("0.00000000")} {wallet.player.atualCoin}";
        fundo.SetFloat("_Velocity", downVelocity);
        betTxt.text = $"{valueToBet:0.0000} {coin}";
        if(terminouOTempo)uiTimer.text = $"x{downStop * 100:0.00}";
        stopValBetTxt.text = $"x{stopValBet:0.00}";
        gem.SetActive(downStop >= 0.1);
        if (mineState.active)
        {
            downStop = downStop + speedDownMultiply * Time.deltaTime;
            speedDownMultiply = speedDownMultiply <= 0.01 && flamerate % 60 == 0 ? speedDownMultiply + 0.0001f : speedDownMultiply;
            downVelocity = downVelocity >= 1? downVelocity: flamerate % 300 == 0 ? downVelocity + 0.2f : downVelocity;
            betButtonTxt.text = stopValue == 0 ? $"Stop Now : x {downStop * 100:0.00}" : $"You Get x{stopValue * 100:0.00}";
            roundBets.ForEach(x =>
            {
                var i = roundBets.IndexOf(x);
                if ((float)Math.Round(x.stop / 100, 2) <= downStop)
                {
                   x.winBet = true;
                }
                if (x.winBet &&roundListObj.Count - 1 >= i)
                {
                    roundListObj[i].GetComponent<Image>().color = new Color(0f, 1f, 0f, 0.1f);
                }
                
            
            });
            if (atualBet.autoStop && atualBet.stop/100 >= downStop ) { StopAndBet(); }
            if(downStop >= downFinal)
            {
                downStop = downFinal;
                CheckBetWin();
            }
        }else
        {
            betButtonTxt.text = doABet ? $"Wait Start Round" : $"Bet :{valueToBet} {coin}";
        }
        
    }// Checagem de novas apostas, atualizaçao de textos e valores

    private void OnDisable() => fundo.SetFloat("_Velocity", 0);

    public IEnumerator MineMachine()
    {
        PrepareMine();
        yield return new WaitForSeconds(timeRoundWait);
        bet.interactable = doABet;
        StarMine();
    }// Rotinia ciclica de inicio de cada rodada

    public void PrepareMine()
    {
        mineState.active = false;
        int r = UnityEngine.Random.Range(0, 100);
        mineState.nextStop = r<=10? UnityEngine.Random.Range(0.01f, 0.999f) : r<=20 ? UnityEngine.Random.Range(0.01f, 0.2f): UnityEngine.Random.Range(0.01f, 0.03f);
        downFinal = mineState.nextStop;
        downStop = 0;
        speedDownMultiply = 0.001f;
        stopValue = 0;
        atualBet = new Bet();
        winBet = false;
        bet.interactable = true;
        doABet = false;
        downVelocity = 0f;
        roundListObj.ForEach(x => {
            x.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);
            x.SetActive(false);
            });
        roundBets.Clear();
        gem.SetActive(false);
        Inicio((int)timeRoundWait);
    }// Inicia todas Variaveis para o estado inicial nessessario.

    public void StarMine()
    {
        mineState.active = true;
        coruja.Play("Down");
        downVelocity = 0.25f;
        pedrasPoeira.SetActive(true);
    }// Estado de inicio de rodada

    public void MakeABet()
    {
        autoStop = autoStopToggle.isOn;
        if (wallet.CheckBalance(valueToBet , coin)&& !doABet)
        {
            atualBet = autoStop? new Bet() { coin = coin, value = valueToBet, addressID = wallet.player.address , stop = stopValBet, autoStop =true} : new Bet() { coin = coin, value = valueToBet, addressID = wallet.player.address , autoStop =false}; 
            wallet.PayBet(atualBet);
            doABet = true;
        }
    }// Cria uma aposta se ouver saldo suficiente

    public void StopAndBet()
    {
        if (!mineState.active && !doABet)
        {
            MakeABet();
        }
        else
        {
            stopValue = downStop;
            winBet = stopValue <= downFinal; 
        }

    }// Botao de Aposta e parar Aposta

    public void UpDownAutoStop(float toUpVal, bool toUp)
    {
        stopValBet = toUp? stopValBet + toUpVal : stopValBet - toUpVal;
        stopValBet = stopValBet < 1.01 ? 1.01f : stopValBet;
    }// Controle de valores, para onde parar automaticamente

    public void UpDownBetAmount(float toUpVal, bool toUp, int sp)
    {

        if (toUp) valueToBet = sp == 1 ? valueToBet + toUpVal : sp== 2? valueToBet*2 : wallet.player.coinBalance;
        else valueToBet = sp == 1 ? valueToBet - toUpVal : sp == 2 ? valueToBet / 2 : 0.001f;
        valueToBet = valueToBet < 0.001 ? 0.001f : valueToBet > wallet.player.coinBalance ? wallet.player.coinBalance :  valueToBet;
    }//Controle de Valores da aposta

    public void CheckBetWin()
    {
        coruja.Play("Kabum");
        explosion.SetActive(true);
        pedrasPoeira.SetActive(false);
        if (winBet)
        {
            wallet.CalculateBetAddToBalance(atualBet,stopValue);
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
        
    }// Finaliza a rodada e checa se a aposta ganhou ou perdeu.Apos isso Reinicializa tudo novamente

    public void MakeFakeBets()
    {
        var d = UnityEngine.Random.Range(0, 2);
        if (d == 0)
        {
            roundBets.Add(new Bet() { coin = "BNB", value = UnityEngine.Random.Range(0.001f, 50f), addressID = $"0x{UnityEngine.Random.Range(10000000,int.MaxValue)}" , stop = (float)Math.Round(UnityEngine.Random.Range(1.01f,5f),2) });
            if (roundBets.Count <= roundListObj.Count) { 
                roundListObj[roundBets.Count - 1].SetActive(true); 
                roundListObj[roundBets.Count - 1].transform.Find("Bet").GetComponent<TMP_Text>().text = $"{roundBets[roundBets.Count - 1].value} {roundBets[roundBets.Count-1].coin}";
                roundListObj[roundBets.Count - 1].transform.Find("Address").GetComponent<TMP_Text>().text = $"{roundBets[roundBets.Count - 1].addressID}";
            }
        }
    }// Para proposito de testes : Simula apostas feitas por outro jogadores

    public void AudioAndEffects()
    {
        if (effectsToggle.isOn) effectsSound.ForEach(x => x.volume = 0.5f);
        else effectsSound.ForEach(x => x.volume = 0);
        if (musicToggle.isOn) music.volume = 0.5f;
        else music.volume = 0;
    }

    #region Timer
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
            MakeFakeBets();
            MakeFakeBets();
            MakeFakeBets();
        }
        EndOfTime();
    }

    public void EndOfTime()
    {
        terminouOTempo = true;
    }
    #endregion
}

public class MineState
{
    public bool active;
    public float nextStop = 0;
}
