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

    private void OnEnable()
    {
        onTutorial = true;
        ClientCommands.Instance.onTutorial = onTutorial;
    }

    private void OnDisable()
    {
        onTutorial = false;
        ClientCommands.Instance.onTutorial = onTutorial;
    }

    void SetTutorialPart(int part)
    {
        if (part == 1)
        {
            partList[0].SetActive(true);
        }
        else
        {
            if(part <= partList.Count)
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

    void NextTutorialPart()
    {
        parts++;
        SetTutorialPart(parts);
    }

    int fakeBet = 0;

    void Simulation(int part)
    {
        switch (part)
        {
            case 1:
                partTutorial[0].SetActive(true);
                CanvasManager.Instance.SetBalanceTxt(500);
                GameManager.Instance.bet = 10;
                CanvasManager.Instance.SetBetInput(10);
                fakeBet = 10;
                break;
            case 2:
                partTutorial[0].SetActive(false);
                onTimerI = true;
                StartCoroutine(FakeTimerI());
                break;
            case 3:
                partTutorial[1].SetActive(false);
                StopCoroutine(FakeTimerI());
                CanvasManager.Instance.SetBetSlot(new BetPlayers() { name = ClientCommands.Instance.playerName, value = 100, multiplier = 0 });
                break;
            case 4:
                partTutorial[1].SetActive(false);
                break;
        }
    }

    void FakeNextBet(int? value, bool up)
    {
        fakeBet = up ? (fakeBet + value ?? 1) : (fakeBet - value ?? 1);
        fakeBet = fakeBet < 1 ? 1 : fakeBet > 100 ? 100 : fakeBet;
        GameManager.Instance.bet = fakeBet;
        CanvasManager.Instance.SetBetInput(fakeBet);
        if (fakeBet == 100) betValOk.interactable = true;
    }

    bool onTimerI = false;
     IEnumerator FakeTimerI()
    {
        var time = 16;
        while (onTimerI)
        {
            CanvasManager.Instance.SetTimerText(time--);
            if(time >= 5)
                yield return new WaitForSeconds(1f);
            else
                onTimerI = false;
        }
        partTutorial[1].SetActive(true);
    }
}
