using Christina.UI;
using PrimeTween;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;
    [SerializeField] public bool onTest = false;
    [SerializeField] Canvas Canvas;
    public TMP_Text betValueText, cashoutValueText,roundsTextII;
    public TMP_InputField betInput, autoCashOutInput;
    public TMP_Text betButtonText, roundsText, timerText, multiplierText,multiplierTextMensage, messageText, balanceText, inPlayersText, inPlayersWinnersText, totalWinAmountText;
    public Slider autoCashOutToggle, autoPlayToggle, roundsSlider;
    public List<Button> roundsButton = new();
    public List<Sprite> buttonsSpites = new();
    public List<Button> betModButtons = new(); // 0= up , 1 = down, 2-6 = +5/+10/+25/+50/Max 
    public Button betButton, crashOutButtonAdd, crashOutButtonSub, configButton;
    public Animator messageAnimator, player0Animator, player1Animator, player2Animator;
    public List<LastRoundHUD> multipliersSlots = new();
    public List<LastRoundHUD> multipliersSlotsFivity = new();
    public List<LastMulti> multipliers, multipliersFivity = new();
    public List<BetPlayersHud> betSlots = new();
    public int playerInBet, playerInBetWinners;
    public float totalWinAmount = 0f;
    public GameObject configObj;
    public float balanceVal,betVal;
    [SerializeField]int betButtonStatus;//0 = Bet, 1 = Cancel, 2 = Stop, 3 = Cant Bet

    //[Header("Rank")]
    ////-------------Rank-----------------
    //[SerializeField] GameObject rankCanvas, playerPanel;
    //[SerializeField] Button rankButton, playerButton;
    //[SerializeField] public List<BetPlayers> rankMultiplier = new();
    //[SerializeField] public List<BetPlayers> rankCash = new();
    //[SerializeField] public List<BetPlayersHud> rankSlotsMultiplier, rankSlotsCash = new();

    [Header("Tradution")]
    //-------------Tradutions-------------
    [SerializeField] public int traduction = 0;//0= english, 1 = Portugues
    [SerializeField] public TMP_Dropdown tradDropdown;
    [SerializeField] List<Sprite> bandeiras;
    [SerializeField] Image bandeira;
    //[SerializeField] public List<TMP_Text> tradTexts = new();
    //[SerializeField] public Button showTutorial;

    [SerializeField] public Action<int> OnTraductionChange;
    [SerializeField] int compTraduction;

    [SerializeField] WinExtra winExtraPrefab;
    [SerializeField] GameObject loadingPanel;

    //[SerializeField] Tutorial tutorial;

    [SerializeField] public List<Player> tankList = new ();
    [SerializeField] List<Sprite> tankSprites = new();
    [SerializeField] List<Image> selectedTankSprites = new ();
    [SerializeField] Image selTank,selTankBig;
    [SerializeField] GameObject canvasSelectTank;
    [SerializeField] TMP_Text timerTextSelectTank, selectTankText;
    public RectTransform aviao;
    public List<GameObject> bonus;
    public float aviaoDistance;
    public float aviaoaAntecipation;
    [SerializeField] TMP_Text bonusTxt;
    [SerializeField] float bonusTotal;
    [SerializeField] BigWin bigWin;

    [SerializeField] int trySelectTank = 0;

    void Awake()
    {
        Instance = this;
        Canvas = GetComponent<Canvas>();
    }

    void Start()
    {
        //ShowPlayers();
        ResetBets();
        tradDropdown.onValueChanged.AddListener(delegate { LanguageManager.instance.ChangeLanguage(tradDropdown.value); });
        tradDropdown.onValueChanged.AddListener(x => bandeira.sprite = bandeiras[x]);
        //rankButton.onClick.AddListener(ShowRank);
        configButton.onClick.AddListener(() => configObj.SetActive(!configObj.activeSelf));
        //playerButton.onClick.AddListener(ShowPlayers);
        betButton.onClick.AddListener(() => {
            BetMensages();
            ClientCommands.Instance.SendBet();
            }
        );
        if (!PlayerPrefs.HasKey("traduction")) PlayerPrefs.SetInt("traduction",0);
        SetTraduction(PlayerPrefs.GetInt("traduction"));
        if (!PlayerPrefs.HasKey("tutorial")) PlayerPrefs.SetInt("tutorial", 0);
    }

    void Update()
    {
        inPlayersText.text = $"{playerInBet}";
        inPlayersWinnersText.text = $"{playerInBetWinners}";
        totalWinAmountText.text = $"{GameManager.Instance.MoedaAtual(totalWinAmount)}";
        if (traduction != compTraduction) SetTraduction(traduction);
    }

    void OnEnable()
    {
        if (Instance != this) { Instance = this; }
    }

    public void ShowLoadingPanel(bool value)
    {
        loadingPanel.SetActive(value);
    }

    public void ShowCanvasSelectTank(bool value)
    {
        canvasSelectTank.SetActive(value);
    }

    public void TrySelectTank(int value)
    {
        if (ClientCommands.Instance.atualStatus == 0 && !GameManager.Instance.isJoin)
        {
            SelectTank(value);
        }
        else
        {
            selectedTankSprites.ForEach(x => 
                x.gameObject.SetActive(selectedTankSprites.IndexOf(x) == value));
            selTankBig.sprite = tankSprites[value];
            trySelectTank = value;
        }
    }

    public void ForceSelectTank()
    {
        if(GameManager.Instance.selectedTankNum == trySelectTank) return;
        SelectTank(trySelectTank);
    }

    public void SelectTank(int value)
    {
        GameManager.Instance.selectedTankNum = value;
        trySelectTank = value;
        tankList.ForEach(x => x.selected = tankList.IndexOf(x) == value);
        selectedTankSprites.ForEach(x => x.gameObject.SetActive( selectedTankSprites.IndexOf(x) == value));
        selTank.sprite = tankSprites[value];
        selTankBig.sprite = tankSprites[value];
    }

    public void BetMensages()
    {
        if (!ClientCommands.Instance.canBet) { 
            PlayMessage(LanguageManager.instance.TryTranslate("canceledbet", "Aposta Cancelada, Espere a Proxima Rodada")); }
        else
        {
            PlayMessage(LanguageManager.instance.TryTranslate( 
            betButtonStatus switch { 
                0 => "sendbet", 
                1 => "cancelbet", 
                2 => "finishbet", 
                3 => "cantbet", 
                _ => ""},
            betButtonStatus switch{ 
                0 => "Enviar Aposta", 
                1 => "Cancelar Aposta", 
                2 => "Finalizar Aposta", 
                3 => "Não Pode Apostar", 
                _ => "" 
            }));

        }
        
    }

    public void SetBetInput(int value)
    {
        betVal = value;
        betInput.text = $" {GameManager.Instance.MoedaAtual(betVal)}";
        betValueText.text = $" {GameManager.Instance.MoedaAtual(betVal)}";
    }

    public void SetAutoCashOutInput(float value)
    {
        autoCashOutInput.text = $"x {value:0.00}";
        cashoutValueText.text = $"x {value:0.00}";
    }

    public void SetRoundsText(int value)
    {
        value = (value == 0) ? (-1) : value;
        roundsText.text = (value == -1) ? "Inf" : ($"{value}");
        roundsTextII.text = (value == -1) ? "Inf" : ($"{value}");
    }

    public void SetTimerText(int value,bool? tutorial = false)
    {
        if(ClientCommands.Instance.onTutorial && tutorial == false) return;
        timerText.text = $"{value:00:00}";
        timerTextSelectTank.text = $"{value:00:00}";
        timerTextSelectTank.gameObject.SetActive(true);
        selectTankText.text = $"{LanguageManager.instance.TryTranslate("nextround","A Rodada Começa em:")}";
    }

    public void SetMultiplierText(float value, bool? tutorial = false)
    {
        if(ClientCommands.Instance.onTutorial && tutorial == false) return;
        timerTextSelectTank.gameObject.SetActive(false);
        selectTankText.text = $"{LanguageManager.instance.TryTranslate("configuretank", "Configurando Tanque")}";
        multiplierText.text = $"x {value:0.00}";
    }

    public void PlayMessage(string msg)
    {
        messageText.text = msg;
        messageAnimator.Play("PopUp");
    }

    public void ShowBonus(float value)
    {
        bonusTotal += value;
        bonusTxt.text = $"x {bonusTotal :0.00}";
    }

    public void ResetBonus()
    {
        bonusTotal = 0;
        bonusTxt.text = $"x {bonusTotal :0.00}";
    }

    public void PlayBonus(BonusDrop b)
    {
        Tween.UIAnchoredPositionX(aviao, endValue: Screen.width + aviaoDistance, duration: aviaoaAntecipation, ease: Ease.Linear)
            .OnComplete(() => {
                aviao.anchoredPosition = new Vector2(0, 0);
                if (!GameManager.Instance.fundoOnMove)
                {
                    return;
                }
                bonus.ForEach(x => x.SetActive(false));
                bonus.ForEach(x => x.SetActive(true));
            }); ;
    }

    public void SetBigWin(float value)
    {
        bigWin.value = value;
        bigWin.gameObject.SetActive(value > 0);
    }

    public void SetBetButtonBet()
    {
        betButtonText.text = LanguageManager.instance.TryTranslate("bet", "Apostar");
        betButtonStatus = 0;
    }

    public void SetBetButtonCancel()
    {
        betButtonText.text = LanguageManager.instance.TryTranslate("cancelbet", "Cancelar Aposta");
        betButtonStatus = 1;
    }

    public void SetBetButtonCantBet()
    {
        betButtonText.text = LanguageManager.instance.TryTranslate("waitround", "Espere a Rodada");
        betButtonStatus = 3;
    }

    public void SetBetButtonStop(float var)
    {
        betButtonText.text = $"{LanguageManager.instance.TryTranslate("stop","Parar")} {var:0.00}";
        //betButton.interactable = true;
        betButtonStatus = 2;
    }

    public void SetMultiplierTextMensage(bool timer, bool? tutorial = false)
    {
        if(ClientCommands.Instance.onTutorial && tutorial == false) return;
        multiplierTextMensage.text = LanguageManager.instance.TryTranslate(
            timer? "nextround" : "betmultiplier", 
            timer? "Proxima Rodada em :" : "Multiplicador de Aposta:");
    }

    public void SetPlayerStateX(string str,int tank, bool? all = false)
    {
        if (all == true)
        {
            player0Animator.Play(str);
            player1Animator.Play(str);
            player2Animator.Play(str);
        }
        else
        {
            switch (tank)
            {
            case 0:
                player0Animator.Play(str);
                break;
            case 1:
                player1Animator.Play(str);
                break;
            case 2:
                player2Animator.Play(str);
                break;
            case 3:
                break;
            }

        }
    }

    public void SetPlayerState( int tank,bool? running, bool? all = false)
    {
        if (all == true)
        {
            if(running == null)
                tankList.ForEach(x => x.Reset());
            else
                tankList.ForEach(x => x.Walking((bool)running));
        }
        else
        {
            tankList[tank].Walking((bool)running);
        }
    }

    public void SetLastPlays(LastMulti multply)
    {
        multipliers.Add(multply);
        multipliersFivity.Insert(0,multply);
        if (multipliers.Count > multipliersSlots.Count)
        {
            multipliers.RemoveAt(0);
        }

        if (multipliersFivity.Count > multipliersSlotsFivity.Count)
        {
            multipliersFivity.RemoveAt(multipliersFivity.Count - 1);
        }

        SetSlot(multply);
        SetSlotFivity(multply);
    }

    public void SetSlot(LastMulti multply)
    {
        LastRoundHUD slot = multipliersSlots.Last();
        slot.Set(multply);
        slot.transform.SetAsLastSibling();
        multipliersSlots.Remove(slot);
        multipliersSlots.Insert(0, slot);
    }

    public void SetSlotFivity(LastMulti multply)
    {
        LastRoundHUD slot = multipliersSlotsFivity.Last();
        slot.Set(multply);
        slot.transform.SetAsFirstSibling();
        multipliersSlotsFivity.Remove(slot);
        multipliersSlotsFivity.Insert(0, slot);
    }

    public void SetSlotFivity(bool? open = false)
    {
        GameManager.Instance.roundsFivityOBJ.SetActive(true);
        multipliersSlotsFivity.ForEach(x =>
        {
            if (multipliersSlotsFivity.IndexOf(x) <= multipliersFivity.Count - 1)
                x.Set(multipliersFivity[multipliersSlotsFivity.IndexOf(x)]);
            else x.Clear();
        });
        GameManager.Instance.roundsFivityOBJ.SetActive((bool)open);
        //multipliersSlotsFivity.ForEach(x => x.Set(multipliersFivity[multipliersSlotsFivity.IndexOf(x)]));
    }

    internal void SetBalanceTxt(double balance)
    {
        balanceVal = (float)balance;
        balanceText.text = $"{GameManager.Instance.MoedaAtual(balance)}";
    }

    public void ResetBets(bool? tutorial = false)
    {
        if(ClientCommands.Instance.onTutorial && tutorial == false) return;
        betSlots.ForEach(x => x.Clear());
        playerInBet = 0;
        playerInBetWinners = 0;
    }

    public void SetBetSlot(BetPlayers bet, bool tutorial = false)
    {
        //if(ClientCommands.Instance.onTutorial && tutorial == false) return;
        //int index = betSlots.FindIndex(x => x.name.text == bet.name);
        //if (index == -1)
        //{
        //    index = betSlots.Count - 1;
        //}
        //BetPlayersHud p = betSlots[index];
        //p.Set(bet);
        //p.transform.SetAsFirstSibling();
        //betSlots.Remove(p);
        //betSlots.Insert(0, p);
        //betSlots[0].Set(bet);
        if (bet.multiplier > 0)
        {
            playerInBetWinners++;
            tankList[bet.tankid].CreateTankStop(bet);
            if (bet.name == ClientCommands.Instance.playerName) { 
                totalWinAmount += ((float)bet.value) * bet.multiplier;
                SetBigWin(((float)bet.value) * bet.multiplier);
                GameManager.Instance.isJoin = false;
            }
            //if (!playerShow)
            //{
            //    WinExtra winExtra = Instantiate(winExtraPrefab, transform);
            //    winExtra.SetText($"{bet.name} {GameManager.Instance.MoedaAtual()} {bet.value * bet.multiplier:#,0.00}");
            //}

        }
        else
        {
            playerInBet++;
            if (bet.name == ClientCommands.Instance.playerName)
            {
                SetBetButtonCancel();
            }
        }
    }

    //public void SetRank(Line[] rankMult, Line[] rankCashh)
    //{
    //    List<BetPlayers> mult = new List<BetPlayers>();
    //    List<BetPlayers> cash = new List<BetPlayers>();
    //    foreach (Line item in rankMult)
    //    {
    //        mult.Add(new BetPlayers { name = item.name, value = item.bet, multiplier = item.multi });
    //    }
    //    foreach (Line item in rankCashh)
    //    {
    //        cash.Add(new BetPlayers { name = item.name, value = item.bet, multiplier = item.multi });
    //    }
    //    mult.Sort((x, y) => y.multiplier.CompareTo(x.multiplier));
    //    cash.Sort((x, y) => (y.value * y.multiplier).CompareTo(x.value * x.multiplier));
    //    rankMultiplier = mult;
    //    rankCash = cash;
    //    Debug.Log($"rankMultiplier {rankMultiplier.Count} rankCash {rankCash.Count} rankSlotsMultiplier {rankSlotsMultiplier.Count} rankSlotsCash {rankSlotsCash.Count}");
    //    if (rankMultiplier.Count >= rankSlotsMultiplier.Count)
    //    {
    //        rankSlotsMultiplier.ForEach(x => x.Set(rankMultiplier[rankSlotsMultiplier.IndexOf(x)], true));
    //    }
    //    else
    //    {
    //        rankMultiplier.ForEach(x => rankSlotsMultiplier[rankMultiplier.IndexOf(x)].Set(x, true));
    //    }
    //    if (cash.Count >= rankSlotsCash.Count)
    //    {
    //        rankSlotsCash.ForEach(x => x.Set(cash[rankSlotsCash.IndexOf(x)], true));
    //    }
    //    else
    //    {
    //        rankCash.ForEach(x => rankSlotsCash[rankCash.IndexOf(x)].Set(x, true));
    //    }
    //}
    //bool rankShow = false;

    //public void ShowRank()
    //{
    //    if (playerShow)
    //    {
    //        ShowPlayers();
    //    }
    //    rankCanvas.GetComponent<Animator>().Play(rankShow ? "Hide" : "Show");
    //    rankShow = !rankShow;
    //    rankMultiplier.Sort((x, y) => y.multiplier.CompareTo(x.multiplier));
    //    rankCash.Sort((x, y) => (y.value * y.multiplier).CompareTo(x.value * x.multiplier));
    //    if (rankMultiplier.Count >= rankSlotsMultiplier.Count)
    //    {
    //        rankSlotsMultiplier.ForEach(x => x.Set(rankMultiplier[rankSlotsMultiplier.IndexOf(x)], true));
    //    }
    //    else
    //    {
    //        rankMultiplier.ForEach(x => rankSlotsMultiplier[rankMultiplier.IndexOf(x)].Set(x, true));
    //    }
    //    if (rankCash.Count >= rankSlotsCash.Count)
    //    {
    //        rankSlotsCash.ForEach(x => x.Set(rankCash[rankSlotsCash.IndexOf(x)], true));
    //    }
    //    else
    //    {
    //        rankCash.ForEach(x => rankSlotsCash[rankCash.IndexOf(x)].Set(x, true));
    //    }

    //}

    //bool playerShow = false;
    //public void ShowPlayers()
    //{
    //    if (rankShow)
    //    {
    //        ShowRank();
    //    }
    //    playerPanel.GetComponent<Animator>().Play(playerShow ? "Hide" : "Show");
    //    playerShow = !playerShow;
    //}

    public void SetTraduction(int trad)
    {
        traduction = trad;
        tradDropdown.value = trad;
        GameManager.Instance.traduction = trad;
        LanguageManager.instance.ChangeLanguage(trad switch {  0 =>"ss" ,1 => "rr" , 2 => "zz", _ => "ss" });
        balanceText.text = $"{GameManager.Instance.MoedaAtual(balanceVal)}";
        betInput.text = $" {GameManager.Instance.MoedaAtual(betVal)}";
        betValueText.text = $" {GameManager.Instance.MoedaAtual(betVal)}";
        OnTraductionChange?.Invoke(traduction);
        compTraduction = trad;
    }

    //public void ShowTutorial()
    //{
    //    tutorial.gameObject.SetActive(true);
    //}
}
