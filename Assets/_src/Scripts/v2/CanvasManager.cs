using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;
    [SerializeField] Canvas Canvas;
    [SerializeField] public TMP_InputField betInput, autoCashOutInput;
    [SerializeField] public TMP_Text betButtonText, roundsText, timerText, multiplierText,multiplierTextMensage, messageText, balanceText, inPlayersText, inPlayersWinnersText, totalWinAmountText;
    [SerializeField] public Toggle autoCashOutToggle, autoPlayToggle;
    [SerializeField] public List<Button> roundsButton = new();
    [SerializeField] public List<Sprite> buttonsSpites = new();
    [SerializeField] public List<Button> betModButtons = new(); // 0= up , 1 = down, 2-6 = +5/+10/+25/+50/Max 
    [SerializeField] public Button betButton, crashOutButtonAdd, crashOutButtonSub, configButton;
    [SerializeField] public Animator messageAnimator, playerAnimator;
    [SerializeField] public List<LastSlot> multipliersSlots, multipliersSlotsFivity = new();
    [SerializeField] public List<float> multipliers, multipliersFivity = new();
    [SerializeField] public List<BetPlayersHud> betSlots = new();
    [SerializeField] public int playerInBet, playerInBetWinners;
    [SerializeField] public float totalWinAmount = 0f;
    [SerializeField] public GameObject configObj;
    [SerializeField] public float balanceVal,betVal;
    [SerializeField] int betButtonStatus;//0 = Bet, 1 = Cancel, 2 = Stop, 3 = Cant Bet
    //-------------Rank-----------------
    [SerializeField] GameObject rankCanvas, playerPanel;
    [SerializeField] Button rankButton, playerButton;
    [SerializeField] public List<BetPlayers> rankMultiplier = new();
    [SerializeField] public List<BetPlayers> rankCash = new();
    [SerializeField] public List<BetPlayersHud> rankSlotsMultiplier, rankSlotsCash = new();
    //-------------Tradutions-------------
    [SerializeField] public int traduction = 0;//0= english, 1 = Portugues
    [SerializeField] public List<TMP_Text> tradTexts = new();
    [SerializeField] public TMP_Dropdown tradDropdown;

    [SerializeField] WinExtra winExtraPrefab;
    [SerializeField] GameObject loadingPanel;

    void Awake()
    {
        Instance = this;
        Canvas = GetComponent<Canvas>();
    }

    void Start()
    {
        ShowPlayers();
        ResetBets();
        tradDropdown.onValueChanged.AddListener(delegate { SetTraduction(tradDropdown.value); });
        tradDropdown.value = traduction;
        rankButton.onClick.AddListener(ShowRank);
        configButton.onClick.AddListener(() => configObj.SetActive(!configObj.activeSelf));
        playerButton.onClick.AddListener(ShowPlayers);
        betButton.onClick.AddListener(() => BetMensages());
    }

    void Update()
    {
        inPlayersText.text = $"{playerInBet}";
        inPlayersWinnersText.text = $"{playerInBetWinners}";
        totalWinAmountText.text = $"{GameManager.Instance.MoedaAtual()} {totalWinAmount:#,0.00}";
    }

    void OnEnable()
    {
        if (Instance != this) { Instance = this; }
    }

    public void ShowLoadingPanel(bool value)
    {
        loadingPanel.SetActive(value);
    }

    public void BetMensages()
    {
        switch (betButtonStatus)
        {
            case 0:
                PlayMessage(traduction switch { 0 => "Send Bet", 1 => "Aposta Enviada", _ => "Send Bet" });
                break;
            case 1:
                PlayMessage(traduction switch { 0 => "Cancel Bet", 1 => "Aposta Cancelada", _ => "Cancel Bet" });
                break;
            case 2:
                PlayMessage(traduction switch { 0 => "Finish Bet", 1 => "Aposta Finalizada", _ => "Finish Bet" });
                break;
            case 3:
                PlayMessage(traduction switch { 0 => "Cant Bet, Wait Next Round", 1 => "Não Pode Aposta, Espere a Proxima Rodada", _ => "Cant Bet, Wait Next Round" });
                break;
        }
    }

    public void SetBetInput(int value)
    {
        betVal = value;
        betInput.text = $" {GameManager.Instance.MoedaAtual()} {betVal}";
    }

    public void SetAutoCashOutInput(float value)
    {
        autoCashOutInput.text = $"x {value:0.00}";
    }

    public void SetRoundsText(int value)
    {
        value = (value == 0) ? (-1) : value;
        roundsText.text = (value == -1) ? "Inf" : ($"{value}");
    }

    public void SetTimerText(int value,bool? tutorial = false)
    {
        if(ClientCommands.Instance.onTutorial && tutorial == false) return;
        timerText.text = $"{value:00:00}";
    }

    public void SetMultiplierText(float value, bool? tutorial = false)
    {
        if(ClientCommands.Instance.onTutorial && tutorial == false) return;
        multiplierText.text = $"x {value:0.00}";
    }

    public void PlayMessage(string msg)
    {
        messageText.text = msg;
        messageAnimator.Play("PopUp");
    }

    public void SetBetButtonBet()
    {
        betButtonText.text = traduction switch
        {
            0 => "Bet",
            1 => "Apostar",
            _ => "Bet"
        };
        //betButton.interactable = true;
        betButtonStatus = 0;
    }

    public void SetBetButtonCancel()
    {
        betButtonText.text = traduction switch
        {
            0 => "Cancel Bet",
            1 => "Cancelar Aposta",
            _ => "Cancel Bet"
        };
        //betButton.interactable = true;
        betButtonStatus = 1;
    }

    public void SetBetButtonCantBet(bool? tutorial = false)
    {
        if (ClientCommands.Instance.onTutorial && tutorial == false) return;
        betButtonText.text = traduction switch
        {
            0 => "Wait Round",
            1 => "Espere a Rodada",
            _ => "Wait Round"
        };
        //betButton.interactable = false;
        betButtonStatus = 3;
    }

    public void SetBetButtonStop(float var)
    {
        betButtonText.text = traduction switch
        {
            0 => $"Stop {var:0.00}",
            1 => $"Parar {var:0.00}",
            _ => $"Stop {var:0.00}"
        };
        //betButton.interactable = true;
        betButtonStatus = 2;
    }

    public void SetMultiplierTextMensage(bool timer)
    {
        multiplierTextMensage.text = (timer) ? 
            traduction switch { 0 => "Next Round in :", 1 => "Proxima Rodada em :", _ => "Next Round in :" } :
            traduction switch { 0 => "Bet Multiplier :", 1 => "Multiplicador de Aposta :", _ => "Bet Multiplier :" };
    }

    public void SetPlayerState(string str)
    {
        playerAnimator.Play(str);
    }

    public void SetLastPlays(float multply)
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
        SetSlotFivity();
    }

    public void SetSlot(float multply)
    {
        LastSlot slot = multipliersSlots.Last();
        slot.SetBet(multply);
        slot.transform.SetAsLastSibling();
        multipliersSlots.Remove(slot);
        multipliersSlots.Insert(0, slot);
    }

    public void SetSlotFivity(float multply)
    {
        LastSlot slot = multipliersSlotsFivity.Last();
        slot.SetBet(multply);
        slot.transform.SetAsLastSibling();
        multipliersSlotsFivity.Remove(slot);
        multipliersSlotsFivity.Insert(0, slot);
    }

    public void SetSlotFivity()
    {
        multipliersSlotsFivity.ForEach(x => {
            if (multipliersSlotsFivity.IndexOf(x) <= multipliersFivity.Count-1)
                x.SetBet(multipliersFivity[multipliersSlotsFivity.IndexOf(x)]); 
            else  x.SetBet(0); 
        });
    }

    internal void SetBalanceTxt(double balance)
    {
        balanceVal = (float)balance;
        balanceText.text = $"{GameManager.Instance.MoedaAtual()} {balanceVal:#,0.00}";
    }

    public void ResetBets()
    {
        betSlots.ForEach(x => x.Clear());
        playerInBet = 0;
        playerInBetWinners = 0;
    }

    public void SetBetSlot(BetPlayers bet)
    {
        int index = betSlots.FindIndex(x => x.name.text == bet.name);
        if (index == -1)
        {
            index = betSlots.Count - 1;
        }
        BetPlayersHud p = betSlots[index];
        p.Set(bet);
        p.transform.SetAsFirstSibling();
        betSlots.Remove(p);
        betSlots.Insert(0, p);
        betSlots[0].Set(bet);
        if (bet.multiplier > 0)
        {
            playerInBetWinners++;
            if (bet.name == ClientCommands.Instance.playerName) { totalWinAmount += ((float)bet.value) * bet.multiplier; }
            if (!playerShow)
            {
                WinExtra winExtra = Instantiate(winExtraPrefab, transform);
                winExtra.SetText($"{bet.name} {GameManager.Instance.MoedaAtual()} {bet.value * bet.multiplier:#,0.00}");
            }

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

    public void SetRank(Line[] rankMult, Line[] rankCashh)
    {
        List<BetPlayers> mult = new List<BetPlayers>();
        List<BetPlayers> cash = new List<BetPlayers>();
        foreach (Line item in rankMult)
        {
            mult.Add(new BetPlayers { name = item.name, value = item.bet, multiplier = item.multi });
        }
        foreach (Line item in rankCashh)
        {
            cash.Add(new BetPlayers { name = item.name, value = item.bet, multiplier = item.multi });
        }
        mult.Sort((x, y) => y.multiplier.CompareTo(x.multiplier));
        cash.Sort((x, y) => (y.value * y.multiplier).CompareTo(x.value * x.multiplier));
        rankMultiplier = mult;
        rankCash = cash;
        Debug.Log($"rankMultiplier {rankMultiplier.Count} rankCash {rankCash.Count} rankSlotsMultiplier {rankSlotsMultiplier.Count} rankSlotsCash {rankSlotsCash.Count}");
        if (rankMultiplier.Count >= rankSlotsMultiplier.Count)
        {
            rankSlotsMultiplier.ForEach(x => x.Set(rankMultiplier[rankSlotsMultiplier.IndexOf(x)], true));
        }
        else
        {
            rankMultiplier.ForEach(x => rankSlotsMultiplier[rankMultiplier.IndexOf(x)].Set(x, true));
        }
        if (cash.Count >= rankSlotsCash.Count)
        {
            rankSlotsCash.ForEach(x => x.Set(cash[rankSlotsCash.IndexOf(x)], true));
        }
        else
        {
            rankCash.ForEach(x => rankSlotsCash[rankCash.IndexOf(x)].Set(x, true));
        }
    }
    bool rankShow = false;

    public void ShowRank()
    {
        if (playerShow)
        {
            ShowPlayers();
        }
        rankCanvas.GetComponent<Animator>().Play(rankShow ? "Hide" : "Show");
        rankShow = !rankShow;
        rankMultiplier.Sort((x, y) => y.multiplier.CompareTo(x.multiplier));
        rankCash.Sort((x, y) => (y.value * y.multiplier).CompareTo(x.value * x.multiplier));
        if (rankMultiplier.Count >= rankSlotsMultiplier.Count)
        {
            rankSlotsMultiplier.ForEach(x => x.Set(rankMultiplier[rankSlotsMultiplier.IndexOf(x)], true));
        }
        else
        {
            rankMultiplier.ForEach(x => rankSlotsMultiplier[rankMultiplier.IndexOf(x)].Set(x, true));
        }
        if (rankCash.Count >= rankSlotsCash.Count)
        {
            rankSlotsCash.ForEach(x => x.Set(rankCash[rankSlotsCash.IndexOf(x)], true));
        }
        else
        {
            rankCash.ForEach(x => rankSlotsCash[rankCash.IndexOf(x)].Set(x, true));
        }

    }

    bool playerShow = false;
    public void ShowPlayers()
    {
        if (rankShow)
        {
            ShowRank();
        }
        playerPanel.GetComponent<Animator>().Play(playerShow ? "Hide" : "Show");
        playerShow = !playerShow;
    }

    public void SetTraduction(int trad)
    {
        traduction = trad;
        GameManager.Instance.traduction = trad;
        switch (trad)
        {
            case 0:
                tradTexts.ForEach(x => x.text = GameManager.Instance.tradEnglish[tradTexts.IndexOf(x)]);
                break;
            case 1:
                tradTexts.ForEach(x => x.text = GameManager.Instance.tradPortugues[tradTexts.IndexOf(x)]);
                break;
        }
        betSlots.ForEach(x => x.Reload());
        balanceText.text = $"{GameManager.Instance.MoedaAtual()} {balanceVal:#,0.00}";
        betInput.text = $" {GameManager.Instance.MoedaAtual()} {betVal}";
    }
}
