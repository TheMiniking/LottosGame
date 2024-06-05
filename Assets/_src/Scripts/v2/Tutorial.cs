using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [SerializeField] bool onTutorial;
    [SerializeField] int parts = 0;
    [SerializeField] List<GameObject> partList = new ();
    [SerializeField] List<GameObject> partTutorial = new ();
    [SerializeField] Button betValOk;
    [SerializeField] List<Button> fakeBetControl = new ();

    private void OnEnable()
    {
        onTutorial = true;
        ClientCommands.Instance.OnTutorial(onTutorial);
        fakeBetControl[0].onClick.AddListener(() => FakeNextBet(1, true));
        fakeBetControl[1].onClick.AddListener(() => FakeNextBet(1, false));
        fakeBetControl[2].onClick.AddListener(() => FakeNextBet(5, true));
        fakeBetControl[3].onClick.AddListener(() => FakeNextBet(10, true));
        fakeBetControl[4].onClick.AddListener(() => FakeNextBet(25, true));
        fakeBetControl[5].onClick.AddListener(() => FakeNextBet(50, true));
    }

    private void OnDisable()
    {
        onTutorial = false;
        ClientCommands.Instance.OnTutorial(onTutorial);
    }

    void SetTutorialPart(int part)
    {
        if (part == 1)
        {
            partList[0].SetActive(true);
        }
        else
        {
            if(part < partList.Count)
            {
                partList[part-2].SetActive(false);
                partList[part-1].SetActive(true);
            }
            else
            {
                Simulation(part-partList.Count);
            }
        }
    }

    public void NextTutorialPart()
    {
        parts++;
        SetTutorialPart(parts);
    }

    int fakeBet = 0;

    void Simulation(int part)
    {
        switch (part)
        {
            case 0:
                Debug.Log("Simulaçao pt. 0");
                partTutorial[0].SetActive(true);
                CanvasManager.Instance.SetBalanceTxt(500);
                GameManager.Instance.bet = 10;
                CanvasManager.Instance.SetBetInput(10);
                fakeBet = 10;
                CanvasManager.Instance.SetMultiplierTextMensage(true,true);
                break;
            case 1:
                Debug.Log("Simulaçao pt. 1");
                partTutorial[0].SetActive(false);
                onTimerI = true;
                StartCoroutine(FakeTimerI());
                break;
            case 2:
                Debug.Log("Simulaçao pt. 2");
                partTutorial[1].SetActive(false);
                StopCoroutine(FakeTimerI());
                CanvasManager.Instance.SetBetSlot(new BetPlayers() { name = ClientCommands.Instance.playerName, value = 100, multiplier = 0 },true);
                CanvasManager.Instance.SetBalanceTxt(400);
                NextTutorialPart();
                break;
            case 3:
                Debug.Log("Simulaçao pt. 3");
                partTutorial[2].SetActive(false);
                StartCoroutine(FakeMultply());
                break;
            case 4:
                Debug.Log("Simulaçao pt. 4");
                partTutorial[3].SetActive(false);
                CanvasManager.Instance.SetBetSlot(new BetPlayers() { name = ClientCommands.Instance.playerName, value = 100, multiplier = 2f }, true);
                CanvasManager.Instance.SetBalanceTxt(600);
                CanvasManager.Instance.SetMultiplierText(2f);
                ClientCommands.Instance.Crash(new Crash { multply = 2f },0,true);
                GameManager.Instance.isJoin = false;
                GameManager.Instance.EndMatchStart();
                StopAllCoroutines();;
                NextTutorialPart();
                break;
            case 5:
                Debug.Log("Simulaçao pt. 5 ");
                partTutorial[4].SetActive(true);
                break;
            case 6:
                Debug.Log("Simulaçao pt. 6 - Final");
                partTutorial[4].SetActive(false);
                this.gameObject.SetActive(false);
                CanvasManager.Instance.ResetBets();
                switch (ClientCommands.Instance.atualStatus) 
                {
                    case 0:
                        CanvasManager.Instance.SetBetButtonBet();
                        break;
                    case 1:
                        CanvasManager.Instance.SetBetButtonCantBet();
                        ClientCommands.Instance.StartRun(new StartRun());
                        break;
                    case 2:
                        CanvasManager.Instance.SetBetButtonBet();
                        break;
                }
                parts = 0; // reset
                PlayerPrefs.SetInt("tutorial", 1);
                break;
        }
    }

    public void FakeNextBet(int? value, bool up)
    {
        fakeBet = up ? (fakeBet + value ?? 1) : (fakeBet - value ?? 1);
        fakeBet = fakeBet < 1 ? 1 : fakeBet > 100 ? 100 : fakeBet;
        GameManager.Instance.bet = fakeBet;
        CanvasManager.Instance.SetBetInput(fakeBet);
        if (fakeBet == 100) betValOk.interactable = true;
    }

    bool onTimerI = false;
    bool onMultply = false;
     IEnumerator FakeTimerI()
    {
        var time = 10;
        while (onTimerI)
        {
            CanvasManager.Instance.SetTimerText(time-- , true);
            if(time >= 5)
                yield return new WaitForSeconds(1f);
            else
                onTimerI = false;
        }
        partTutorial[1].SetActive(true);
    }

    IEnumerator FakeMultply()
    {
        Debug.Log("FakeMultply Start");
        var time = 5;
        onTimerI = true;
        while (onTimerI)
        {
            CanvasManager.Instance.SetTimerText(time-- , true);
            yield return new WaitForSeconds(1f);
            onTimerI = time != 0;
        }
        onMultply = true;
        var multShow = 0f;
        ClientCommands.Instance.StartRun(new StartRun {},true);
        CanvasManager.Instance.SetMultiplierTextMensage(false,true);
        while (onMultply)
        {
            CanvasManager.Instance.SetMultiplierText(multShow += 0.01f,true);
            CanvasManager.Instance.SetBetButtonStop(multShow);
            yield return new WaitForSeconds(0.02f);
            onMultply = multShow < 2;
        }
        partTutorial[3].SetActive(true);
    }

    public void StopInteration()
    {
        CanvasManager.Instance.PlayMessage(GameManager.Instance.traduction switch {
            0 => "Follow tutorial steps", 
            1 => "Siga as etapas do tutorial",
            _ => "Follow tutorial steps" });
    } 
}
