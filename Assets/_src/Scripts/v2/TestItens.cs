using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using Unity.VisualScripting;

public class TestItens : MonoBehaviour
{
    public List<Player> tanks;
    public RectTransform aviao;
    public List<GameObject> bonus;
    public float aviaoDistance;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            SelectTankAleatorio();
        }

        if (Input.GetKeyDown(KeyCode.KeypadPeriod))
        {
            MoveAllTank();
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            MoveAviao();
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            SetNewLastRound();
        }

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            MoveTank(0);
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            MoveTank(1);
        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            MoveTank(2);
        }

        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            FakeExit(0);
        }

        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            FakeExit(1);
        }

        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            FakeExit(2);
        }
    }

    public void FakeExit(int tank)
    {
        tanks[tank].FakeExit();
    }

    public void MoveTank(int tank)
    {
        tanks[tank].ButtonTestTank();
    }

    public void MoveAllTank()
    {
        tanks.ForEach(x => x.ButtonTestTank());
    }

    public void MoveAviao()
    {
        Tween.UIAnchoredPositionX(aviao, endValue: Screen.width + aviaoDistance, duration:5, ease: Ease.Linear)
            .OnComplete(() => { 
                aviao.anchoredPosition = new Vector2(0, 0);
                bonus.ForEach(x => x.SetActive(false));
                bonus.ForEach(x => x.SetActive(true));
            }); ;
    }

    public void SetNewLastRound()
    {
        LastMulti f = new() { multis = new multiplier[3] { new multiplier { multiply = Random.Range(1f, 6f), tankid= 0}, new multiplier { multiply = Random.Range(1f, 6f), tankid = 1 }, new multiplier { multiply = Random.Range(1f, 6f), tankid = 2 } } };
        CanvasManager.Instance.SetLastPlays(f);
    }

    public void SelectTankAleatorio ()
    {
        var i = Random.Range(0, tanks.Count);
        CanvasManager.Instance.TrySelectTank(i);
    }


}
