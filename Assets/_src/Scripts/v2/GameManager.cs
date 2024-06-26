using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] bool debug = false;

    [SerializeField][Range(1, 100)] public int bet;
    [SerializeField][Range(-1, 1000)] int betRounds = -1;
    [SerializeField] List<int> rounds = new();
    [SerializeField][Range(1f, 999f)] float autoCashOut = 1f;

    //------------- Tanks Gameplay ------------
    [SerializeField] public int selectedTankNum = -1;
    [SerializeField] public bool isJoin = false;
    public bool canBet, isMobile, isWalking = false;

    //-----------Fundo-----------
    [SerializeField] Material fundo;
    [SerializeField] float fundoRealtimeAtualPosition;
    [SerializeField] public bool fundoOnMove;
    [SerializeField] float fundoRealtimeVelocity;
    [SerializeField, Tooltip("Velocidade do Fundo : Base = 1f")] public float paralaxVelocity = 1f;

    //----------Mobile Teclado-----------
    [SerializeField] GameObject teclado;
    [SerializeField] List<Button> tecladoButtons = new();   // 0-9 = num, 10 = ponto, 11 = backspace, 12 = enter,13 = cancel
    [SerializeField] TMP_Text tecladoText, tecladoShowMode;
    [SerializeField] string tecladoTextValue = string.Empty;
    [SerializeField] bool tecladoMode = false;              // false = bet, true = auto stop
    //------------Canvas-----------
    [SerializeField] public CanvasManager Desktop, Mobile;
    [SerializeField] public Camera cam;
    [SerializeField] public GameObject roundsFivityOBJ,roundsFivityOBJMobile;

    //------------Traduction-----------
    [SerializeField] public int traduction = 0;
    [SerializeField] public CultureInfo Culture;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // disable screen sleep
        Application.targetFrameRate = 120;
        SetScreenMode(Application.isMobilePlatform);
        CanvasManager.Instance.betModButtons[0].onClick.AddListener(() => ModValorBet(true, 1));
        CanvasManager.Instance.betModButtons[1].onClick.AddListener(() => ModValorBet(false, 1));
        CanvasManager.Instance.betModButtons[2]?.onClick.AddListener(() => ModValorBet(true, 5));
        CanvasManager.Instance.betModButtons[3]?.onClick.AddListener(() => ModValorBet(true, 10));
        CanvasManager.Instance.betModButtons[4]?.onClick.AddListener(() => ModValorBet(true, 25));
        CanvasManager.Instance.betModButtons[5]?.onClick.AddListener(() => ModValorBet(true, 50));
        CanvasManager.Instance.betModButtons[6]?.onClick.AddListener(() => ModValorBet(true, 100));
        CanvasManager.Instance.betInput.onEndEdit.AddListener(x =>
        {
            int.TryParse(x, out int n);
            n = (n < 1) ? 1 : ((n > 100) ? 100 : n);
            SetBet(n);
        });
        if (isMobile)
        {
            CanvasManager.Instance.betInput.onSelect.AddListener((x) => SelectTeclado(0));
            CanvasManager.Instance.autoCashOutInput.onSelect.AddListener((x) => SelectTeclado(1));
        }
        CanvasManager.Instance.crashOutButtonAdd.onClick.AddListener(() => ModValorAutoCashOut(true));
        CanvasManager.Instance.crashOutButtonSub.onClick.AddListener(() => ModValorAutoCashOut(false));
        CanvasManager.Instance.autoCashOutInput.onEndEdit.AddListener(x =>
        {
            string resultado = Regex.Replace(x.Replace(".", ","), "[^0-9,]", string.Empty);
            float n = 0;
            float.TryParse(resultado, out n);
            n = (n < 1.0f) ? 1.0f : ((n > 999f) ? 999f : n);
            SetValorAutoCashOut(n);
        });
        CanvasManager.Instance.roundsButton[0]?.onClick.AddListener(() => SetRoundButtons(0));
        CanvasManager.Instance.roundsButton[1]?.onClick.AddListener(() => SetRoundButtons(1));
        CanvasManager.Instance.roundsButton[2]?.onClick.AddListener(() => SetRoundButtons(2));
        CanvasManager.Instance.roundsButton[3]?.onClick.AddListener(() => SetRoundButtons(3));
        CanvasManager.Instance.roundsButton[4]?.onClick.AddListener(() => SetRoundButtons(4));
        CanvasManager.Instance.roundsSlider.onValueChanged.AddListener(x => SetRoundSlider((int)x));
        CanvasManager.Instance.autoCashOutToggle.onValueChanged.AddListener(x => 
        { 
            if (x == 0) AutoCashOut(false);
            if (x == 1) AutoCashOut(true);
        });
        CanvasManager.Instance.autoPlayToggle.onValueChanged.AddListener(x =>
        {
            if (x == 0) AutoStop(false);
            if (x == 1) AutoStop(true);
        });
        SetRoundButtons(0);
    }

    void Update()
    {
        fundoRealtimeAtualPosition = fundoOnMove ? (fundoRealtimeAtualPosition + fundoRealtimeVelocity) : fundoRealtimeAtualPosition;
        fundo.SetFloat("_RealTimeUpdate", fundoRealtimeAtualPosition);
        //if (isMobile && (Screen.height < Screen.width)) { SetScreenMode(); }
        //if (!isMobile && (Screen.height > Screen.width)) { SetScreenMode(); }
    }

    public void SetScreenMode(bool? mobile = null)
    {
        if (Screen.height > Screen.width || (bool)mobile)
        {
            Mobile.gameObject.SetActive(true);
            Desktop.gameObject.SetActive(false);
            cam.orthographicSize = 400;
            cam.transform.position = new Vector3(-70,0, -400);
            isMobile = true;
        }
        else
        {
            Mobile.gameObject.SetActive(false);
            Desktop.gameObject.SetActive(true);
            cam.orthographicSize = 200;
            cam.transform.position = new Vector3(0, 0, -400);
            isMobile = false;
        }
    }

    #region BetPainel Funcoes

    public void SendBet()
    {
        if (debug)
        {
            Debug.Log($"SendBet, valor atual: {bet}");
        }
        ClientCommands.Instance.SendBet();
    }

    public void SetBet(int bets)
    {
        if (debug) Debug.Log($"SetBet, valor atual: {bets}");
        bet = bets;
        ClientCommands.Instance.NextBet(bet);
    }

    public void ModValorBet(bool up, int? valor)
    {
        if (isJoin) return;
        var betI = bet;
        betI = up ? (betI + valor ?? 1) : (betI - valor ?? 1);
        betI = (betI <= 0) ? 1 : ((betI > 100) ? 100 : betI);
        if(CanvasManager.Instance.onTest) CanvasManager.Instance.SetBetInput(betI);
        ClientCommands.Instance.NextBet((int)betI);
        if (debug)
        {
            Debug.Log($"ModBet, valor atual: {betI}");
        }
    }

    public void SetValorAutoCashOut(float valor)
    {
        autoCashOut = valor;
        CanvasManager.Instance.SetAutoCashOutInput(autoCashOut);
        if (debug)
        {
            Debug.Log($"AutoCashOut set valor: {autoCashOut}");
        }
    }

    public void ModValorAutoCashOut(bool up)
    {
        autoCashOut = up ? (autoCashOut + 0.1f) : (autoCashOut - 0.1f);
        autoCashOut = (autoCashOut < 1.0f) ? 1.0f : ((autoCashOut > 999f) ? 999f : autoCashOut);
        CanvasManager.Instance.SetAutoCashOutInput(autoCashOut);
        if (debug)
        {
            Debug.Log($"ModAutoCashOut, valor atual: {autoCashOut}");
        }
    }

    public void SetRounds(int rounds)
    {
        betRounds = rounds;
        CanvasManager.Instance.SetRoundsText(betRounds);
        if (debug)
        {
            Debug.Log($"SetRounds, valor atual: {betRounds}");
        }
    }

    public void SetRoundButtons(int i)
    {
        betRounds = rounds[i];
        Button escolhido = CanvasManager.Instance.roundsButton[i];
        CanvasManager.Instance.roundsButton.ForEach(x => x.GetComponent<Image>().sprite = CanvasManager.Instance.buttonsSpites[0]);
        escolhido.GetComponent<Image>().sprite = CanvasManager.Instance.buttonsSpites[1];
        CanvasManager.Instance.SetRoundsText(betRounds);
        if (debug)
        {
            Debug.Log($"SetRoundButtons, valor atual: {betRounds}");
        }
    }

    public void SetRoundSlider(int i)
    {
        betRounds = i;
        betRounds = betRounds == 1000? -1 : betRounds;
        CanvasManager.Instance.SetRoundsText(i);
        if (debug)
        {
            Debug.Log($"SetRounds, valor atual: {betRounds}");
        }
    }

    public void ResetRoundsSprite()
    {
        CanvasManager.Instance.roundsButton.ForEach(x => x.GetComponent<Image>().sprite = CanvasManager.Instance.buttonsSpites[0]);
    }

    public void ModRound(bool up)
    {
        betRounds = up ? ((betRounds == -1) ? 1 : (betRounds + 1)) : (betRounds - 1);
        betRounds = (betRounds == 0) ? (-1) : ((betRounds < -1) ? 999 : ((betRounds > 999) ? (-1) : betRounds));
        CanvasManager.Instance.SetRoundsText(betRounds);
    }

    #endregion

    #region Automatic


    [SerializeField] public bool activeAutoPlay;
    [SerializeField] public bool activeAutoCashOut;
    //quando inicia o tempo pra entrar na partida
    public void NewMatchInit()
    {
        if (debug)
        {
            Debug.Log(activeAutoPlay + " NewMatchInit " + betRounds);
        }

        if (activeAutoPlay && ((betRounds == -1) || (betRounds > 0)))
        {
            ClientCommands.Instance.SendBet();
            CanvasManager.Instance.PlayMessage(LanguageManager.instance.TryTranslate("msg_autoplayactive", "AutoPlay Active"));
            if (betRounds > 0)
            {
                betRounds--;
            }
            if (betRounds == 0)
            {
                CanvasManager.Instance.PlayMessage(LanguageManager.instance.TryTranslate("msg_autoplaydesactive", "AutoPlay Desactive"));
                CanvasManager.Instance.autoPlayToggle.value = 0;
            }
            CanvasManager.Instance.SetRoundsText(betRounds);
        }
    }
    //quando começa a somar o multiplicador.
    public void NewMathStart()
    {

    }
    public void MatchMultiplier(float value)
    {
        if (activeAutoCashOut && (value >= autoCashOut) && isJoin)
        {
            ClientCommands.Instance.SendBet();      //envia o comando para o servidor para parar o auto
            isJoin = false;
            CanvasManager.Instance.PlayMessage($"{LanguageManager.instance.TryTranslate("cashout_long", "CashOut")} x {value:0.00}");
        }
    }

    internal void EndMatchStart()
    {

    }

    internal void SetAutoStop(float v)
    {
        autoCashOut = v;
    }

    internal void AutoStop(bool x)
    {
        activeAutoPlay = x;
        CanvasManager.Instance.PlayMessage(x ? 
            LanguageManager.instance.TryTranslate("msg_autoplayactive", "AutoPlay Active") :
            LanguageManager.instance.TryTranslate("msg_autoplaydesactive", "AutoPlay Desativado"));
    }

    internal void AutoCashOut(bool x)
    {
        activeAutoCashOut = x;
        if (x)
        {
            CanvasManager.Instance.autoCashOutToggle.value = 1;
        }
        CanvasManager.Instance.PlayMessage(x ? 
            LanguageManager.instance.TryTranslate("msg_cashoutactive", "CashOut ativo") : 
            LanguageManager.instance.TryTranslate("msg_cashoutdesactive", "CashOut Desativado"));
    }

    #endregion

    #region Game

    public IEnumerator DisplayTimer(float time)
    {
        while (time > 0)
        {
            time--;
            CanvasManager.Instance.SetTimerText((int)time);
            yield return new WaitForSeconds(1);
        }

    }

    public IEnumerator DisplayMulti(float multiSum)
    {
        if (debug)
        {
            Debug.Log("DisplayMulti " + multiSum);
        }

        float multiplier = 1;
        float timer = Time.time - multiSum;
        while (true)
        {
            
            multiplier = MultiplierCalculator(Time.time - timer);
            if (isJoin)
            {
                CanvasManager.Instance.SetBetButtonStop(multiplier);
                MatchMultiplier(multiplier);// Auto CashOut
            }
            CanvasManager.Instance.SetMultiplierText(multiplier);
            //fundoRealtimeVelocity = (multiplier < 2) ? 0.1f : ((multiplier < 10) ? 0.12f : 0.15f);
            yield return new WaitForSeconds(0.03f);
        }
    }

    //float MultiplierCalculator(float tempoDecorrido)
    //{
    //    //Debug.Log("MultiplierCalculator " + tempoDecorrido);
    //    return 1.01f + (2 * Mathf.Pow(tempoDecorrido / 10, 1.5f));
    //}

    public float MultiplierCalculator(float tempoDecorrido)
    {
        float Mf = 998.99f;
        float Mi = 1.01f;
        float Tf = 120f;
        float C = 2.2f;
        float Ta = tempoDecorrido;

        float Ma = Mi + Mathf.Pow(1 / ((1 / Ta) * Tf), C) * Mf;

        return Ma;
    }

    public void ResetVelocityParalax()
    {
        if (debug)
        {
            Debug.Log($"ResetVelocityParalax: {fundoRealtimeVelocity}");
        }

        fundoRealtimeVelocity = 0.07f;
    }

    public string MoedaAtual() => traduction switch { 0 => "$", 1 => "R$", _ => "$" };

    public string MoedaAtual(double valor)
    {
        return valor.ToString("C" , Culture);
    }

    public void SelectTeclado(int teclado)
    {
        this.teclado.SetActive(true);
        if (debug)
        {
            Debug.Log($"SelectTeclado: {((teclado == 0) ? "Bet" : "CashOut")}");
        }

        tecladoMode = (teclado == 0) ? true : false;
        tecladoButtons[10].gameObject.SetActive(teclado == 1);
        tecladoText.text = (teclado == 0) ? ($"{tecladoTextValue}") : ($"x {tecladoTextValue:0.00}");
        tecladoShowMode.text = (teclado == 0) ? "Bet" : "CashOut";

    }

    public void SelectTecladoNumber(int number) // 10 = ponto
    {
        if (debug)
        {
            Debug.Log($"SelectTecladoNumber: {number}");
        }
        if (tecladoTextValue.Length >= 8)
        {
            return;
        }

        tecladoTextValue = (number == 10) ? (tecladoTextValue + ".") : (tecladoTextValue + number);
        tecladoText.text = tecladoMode ? ($"{tecladoTextValue}") : ($"x {tecladoTextValue:0.00}");
    }

    public void SelectTecladoBackspace()
    {
        if (debug)
        {
            Debug.Log($"SelectTecladoBackspace");
        }

        if (tecladoTextValue.Length > 0)
        {
            tecladoTextValue = tecladoTextValue.Substring(0, tecladoTextValue.Length - 1);
        }

        tecladoText.text = tecladoMode ? ($"{tecladoTextValue}") : ($"x {tecladoTextValue:0.00}");
    }

    public void SelectTecladoEnter()
    {
        float v = float.Parse(tecladoTextValue.Replace(".", ","));
        if (debug)
        {
            Debug.Log($"SelectTecladoEnter");
        }

        if (tecladoMode)
        {
            int bet = int.Parse(tecladoTextValue);
            bet = (bet <= 0) ? 1 : ((bet > 100) ? 100 : bet);
            ClientCommands.Instance.NextBet(bet);
        }
        else
        {
            v = (v <= 1) ? 1 : ((v > 999) ? 999 : v);
            SetValorAutoCashOut(v);
        }
        tecladoTextValue = string.Empty;
        teclado.SetActive(false);
    }

    public void TecladoCancel()
    {
        teclado.SetActive(false);
    }

    #endregion

}
