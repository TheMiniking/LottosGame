using PrimeTween;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] GameObject explosion;
    [SerializeField] GameObject fire;
    [SerializeField] int tankNum;
    [SerializeField] bool lastMovingStatus = false;
    [SerializeField] RectTransform tankTrasform, canvasTrasform;
    [SerializeField] Vector2 inicialPos;
    [SerializeField] float velocity;
    [SerializeField] float movimentMaxDistance;
    [SerializeField] GameObject selectImage;

    [SerializeField] public bool selected;
    [SerializeField] Tween tween;

    [SerializeField] BetPlayersHud tankBet;
    [SerializeField] GameObject explosionFX , fireFX , concertarFX ; 
 
    void Start()
    {

    }

    private void Update()
    {
        switch (tankNum)
        {
            case 1:
                if(ClientCommands.Instance.tank1OnRunning != lastMovingStatus)
                {
                    lastMovingStatus = ClientCommands.Instance.tank1OnRunning;
                    Walking(lastMovingStatus);
                }
                break;
            case 2:
                if (ClientCommands.Instance.tank2OnRunning != lastMovingStatus)
                {
                    lastMovingStatus = ClientCommands.Instance.tank2OnRunning;
                    Walking(lastMovingStatus);
                }
                break;
            case 3:
                if (ClientCommands.Instance.tank3OnRunning != lastMovingStatus)
                {
                    lastMovingStatus = ClientCommands.Instance.tank3OnRunning;
                    Walking(lastMovingStatus);
                }
                break;
        }
        if (selected != selectImage.activeSelf) selectImage.SetActive(selected);

    }

    public void Walking(bool v)
    {
        anim.Play(v ? "Walking" : "Lost");
        if (!v)
        {
            StopCoroutine(GoCenter());
            StopCoroutine(MovingAuto());
            tween.Stop();
            StartCoroutine(GoBack(tankTrasform));
        }
        else
        {
            StopCoroutine(MovingAuto());
            StopCoroutine(GoBack(tankTrasform));
            tween.Stop();
            StartCoroutine(GoCenter());

        }
    }
    public void Crash(bool v)
    {
        Debug.Log("Crash");
        anim.Play("Lost");
    }

    public IEnumerator GoBack(RectTransform tank)
    {
        tween = Tween.UIAnchoredPositionX(tank, endValue: -(Screen.width/2) , duration: 3)
            .OnComplete(this, target => { fireFX.SetActive(false);  tween.Stop(); tween = Tween.UIAnchoredPositionX(tank, endValue: -(Screen.width / 2), duration: 0); });
        explosionFX.SetActive(true);
        fireFX.SetActive(true);
        yield return new WaitForSeconds(3);
        Debug.Log("End GoBack");
    }
    public IEnumerator GoCenter()
    {
        tween = Tween.UIAnchoredPositionX(tankTrasform, endValue: 0, duration: 2);
        fireFX.SetActive(false);
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(1);
            if (!lastMovingStatus) { 
                tween.Stop();
                Debug.Log("Stop GoCenter");
                break;
            }
        }
        StartCoroutine(MovingAuto());
        Debug.Log("End GoCenter");
    }
    public IEnumerator MovingAuto()
    {
            var n = Random.Range(0, 2);
        while (lastMovingStatus)
        {
            int d = Random.Range(1, 3);
            if (n == 0) 
            {
                tween = Tween.UIAnchoredPositionX(tankTrasform, endValue:Random.Range(50,75), duration:d);
                //yield return new WaitForSeconds(d);
                for (int i = 0; i < d + 1; i++)
                {
                    yield return new WaitForSeconds(1);
                    if (!lastMovingStatus)
                    {
                        Debug.Log($"Stop MovingAuto {i}");
                        break;
                    }
                }

            }
            else
            {
                tween = Tween.UIAnchoredPositionX(tankTrasform, endValue: Random.Range(-75, -50), duration: d+3);
                //yield return new WaitForSeconds(d);
                for (int i = 0; i < d + 4; i++)
                {
                    yield return new WaitForSeconds(1);
                    if (!lastMovingStatus)
                    {
                        Debug.Log($"Stop MovingAuto {i}");
                        break;
                    }
                }
            }
            n = n == 0 ? 1 : 0;
        }
        Debug.Log("End MovingAuto");
    }

    public void CreateTankStop(BetPlayers bet)
    {
        Quaternion q = new Quaternion();
        var tank = Instantiate(tankBet, tankTrasform.position ,q,canvasTrasform);
        tank.Set(bet);
        StartCoroutine(tank.GoBack(tank.GetComponent<RectTransform>()));
    }

   public void FakeExit()
    {
        CreateTankStop(new BetPlayers() { name = "FakeExit", value = Random.Range(1f, 100f), multiplier = Random.Range(1f, 100f)});
    }

    public void ButtonTestTank()
    {
        switch (tankNum)
        {
            case 1:
                ClientCommands.Instance.tank1OnRunning = !ClientCommands.Instance.tank1OnRunning;
                break;
            case 2:
                ClientCommands.Instance.tank2OnRunning = !ClientCommands.Instance.tank2OnRunning;
                break;
            case 3:
                ClientCommands.Instance.tank3OnRunning = !ClientCommands.Instance.tank3OnRunning;
                break;
        }
    }
}
