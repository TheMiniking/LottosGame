using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] bool debug = false;
    [SerializeField][Range(1f, 999f)] float autoCashOut = 1f;
    [SerializeField][Range(1, 100)] public int bet;
    [SerializeField][Range(-1, 100)] int betRounds = -1;
    [SerializeField] List<int> rounds = new();
    [SerializeField] public bool isJoin = false;
    public bool canBet, isMobile, isWalking = false;
    [SerializeField] Material fundo;
    [SerializeField] float fundoRealtimeAtualPosition;
    [SerializeField] public bool fundoOnMove;
    [SerializeField] float fundoRealtimeVelocity;
    //----------Mobile Teclado-----------
    [SerializeField] GameObject teclado, roundsFivityOBJ;
    [SerializeField] List<Button> tecladoButtons = new();   // 0-9 = num, 10 = ponto, 11 = backspace, 12 = enter,13 = cancel
    [SerializeField] TMP_Text tecladoText, tecladoShowMode;
    [SerializeField] string tecladoTextValue = string.Empty;
    [SerializeField] bool tecladoMode = false;              // false = bet, true = auto stop
    //------------Canvas-----------
    [SerializeField] public CanvasManager Desktop, Mobile;
    [SerializeField] public Camera cam;
    //------------Traduction-----------
    [SerializeField] public int traduction = 0;
    [SerializeField] public List<string> tradEnglish, tradPortugues = new();


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // disable screen sleep
        SetScreenMode();
        roundsFivityOBJ.SetActive(false);
        CanvasManager.Instance.betModButtons[0].onClick.AddListener(() => ModValorBet(true, 1));
        CanvasManager.Instance.betModButtons[1].onClick.AddListener(() => ModValorBet(false, 1));
        CanvasManager.Instance.betModButtons[2]?.onClick.AddListener(() => ModValorBet(true, 5));
        CanvasManager.Instance.betModButtons[3]?.onClick.AddListener(() => ModValorBet(true, 10));
        CanvasManager.Instance.betModButtons[4]?.onClick.AddListener(() => ModValorBet(true, 25));
        CanvasManager.Instance.betModButtons[5]?.onClick.AddListener(() => ModValorBet(true, 50));
        CanvasManager.Instance.betModButtons[6]?.onClick.AddListener(() => ModValorBet(true, 100));
        CanvasManager.Instance.betInput.onEndEdit.AddListener(x =>
        {
            int n = 0;
            int.TryParse(x, out n);
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
        CanvasManager.Instance.roundsButton[0]?.onClick.AddListener(() => SetRounds(-1));
        CanvasManager.Instance.roundsButton[1]?.onClick.AddListener(() => SetRounds(10));
        CanvasManager.Instance.roundsButton[2]?.onClick.AddListener(() => SetRounds(25));
        CanvasManager.Instance.roundsButton[3]?.onClick.AddListener(() => SetRounds(50));
        CanvasManager.Instance.roundsButton[4]?.onClick.AddListener(() => SetRounds(100));
        CanvasManager.Instance.autoCashOutToggle.onValueChanged.AddListener(x => AutoCashOut(x));
        CanvasManager.Instance.autoPlayToggle.onValueChanged.AddListener(x => AutoStop(x));
        SetRoundButtons(0);
    }

    void Update()
    {
        fundoRealtimeAtualPosition = fundoOnMove ? (fundoRealtimeAtualPosition + fundoRealtimeVelocity) : fundoRealtimeAtualPosition;
        fundo.SetFloat("_RealTimeUpdate", fundoRealtimeAtualPosition);
        if (isMobile && (Screen.height < Screen.width)) { SetScreenMode(); }
        if (!isMobile && (Screen.height > Screen.width)) { SetScreenMode(); }
    }

    public void SetScreenMode()
    {
        if (Screen.height > Screen.width)
        {
            Mobile.gameObject.SetActive(true);
            Desktop.gameObject.SetActive(false);
            cam.orthographicSize = 300;
            cam.transform.position = new Vector3(0, -50, -400);
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
        bet = bets;
        ClientCommands.Instance.NextBet(bet);
    }

    public void ModValorBet(bool up, int? valor)
    {
        if (isWalking)
        {
            return;
        }

        bet = up ? (bet + valor ?? 1) : (bet - valor ?? 1);
        bet = (bet <= 0) ? 1 : ((bet > 100) ? 100 : bet);
        ClientCommands.Instance.NextBet(bet);
        if (debug)
        {
            Debug.Log($"ModBet, valor atual: {bet}");
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

    public void ResetRoundsSprite()
    {
        CanvasManager.Instance.roundsButton.ForEach(x => x.GetComponent<Image>().sprite = CanvasManager.Instance.buttonsSpites[0]);
    }

    public void ModRound(bool up)
    {
        betRounds = up ? ((betRounds == -1) ? 1 : (betRounds + 1)) : (betRounds - 1);
        betRounds = (betRounds == 0) ? (-1) : ((betRounds < -1) ? 100 : ((betRounds > 100) ? (-1) : betRounds));
        CanvasManager.Instance.SetRoundsText(betRounds);
    }

    #endregion

    #region Automatic


    [SerializeField] bool activeAutoPlay;
    [SerializeField] bool activeAutoCashOut;
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
            CanvasManager.Instance.PlayMessage(traduction switch { 0 => "AutoPlay Bet", 1 => "Aposta Automatica" , _ => "AutoPlay Bet"});
            if (betRounds > 0)
            {
                betRounds--;
            }

            if (betRounds == 0)
            {
                CanvasManager.Instance.PlayMessage(traduction switch { 0 => "End of AutoPlay", 1 => "Fim da Aposta Automatica" , _ => "End of AutoPlay"});
                CanvasManager.Instance.autoPlayToggle.isOn = false;
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
        if (debug)
        {
            Debug.Log("MatchMultiplier :" + value + "autocash " + (activeAutoCashOut ? "ativo" : "desativo"));
        }

        if (activeAutoCashOut && (value >= autoCashOut))
        {
            Debug.Log($"Stop Auto DEBUG");
            ClientCommands.Instance.SendBet();      //envia o comando para o servidor para parar o auto
            CanvasManager.Instance.PlayMessage(traduction switch { 0 => $"CashOut x{value:0.00}", 1 => $"Saiu x{value:0.00}" , _ => $"CashOut x{value:0.00}"});
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
        switch (traduction)
        {
            case 0:
                CanvasManager.Instance.PlayMessage(x ? "AutoPlay Active" : "AutoPlay Desactive");
                break;
            case 1:
                CanvasManager.Instance.PlayMessage(x ? "Automatico Ativo" : "automatico Desativado");
                break;
            default:
                CanvasManager.Instance.PlayMessage(x ? "AutoPlay Active" : "AutoPlay Desactive");
                break;

        }
    }

    internal void AutoCashOut(bool x)
    {
        activeAutoCashOut = x;
        if (x)
        {
            CanvasManager.Instance.autoCashOutToggle.isOn = true;
        }
        switch (traduction)
        {
            case 0:
                CanvasManager.Instance.PlayMessage(x ? "CashOut Active" : "CashOut Desactive");
                break;
            case 1:
                CanvasManager.Instance.PlayMessage(x ? "Saida Auto Ativa" : "Saida Auto Desativo");
                break;
            default:
                CanvasManager.Instance.PlayMessage(x ? "CashOut Active" : "CashOut Desactive");
                break;

        }

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
    float MultiplierCalculator(float tempoDecorrido)
    {
        //Debug.Log("MultiplierCalculator " + tempoDecorrido);
        return 1.01f + (2 * Mathf.Pow(tempoDecorrido / 10, 1.5f));
    }

    public void ResetVelocityParalax()
    {
        if (debug)
        {
            Debug.Log($"ResetVelocityParalax: {fundoRealtimeVelocity}");
        }

        fundoRealtimeVelocity = 0.07f;
    }

    public string MoedaAtual() => traduction switch { 0 => "$", 1 => "R$" ,_ => "$"};

    public void SelectTeclado(int teclado)
    {
        this.teclado.SetActive(true);
        if (debug)
        {
            Debug.Log($"SelectTeclado: {((teclado == 0) ? "Bet" : "CashOut")}");
        }

        tecladoMode = (teclado == 0) ? true : false;
        tecladoButtons[10].interactable = teclado == 1;
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
