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
                if (ClientCommands.Instance.tank1OnRunning != lastMovingStatus && !ClientCommands.Instance.tank2OnRunning && !ClientCommands.Instance.tank3OnRunning )
                {
                    lastMovingStatus = false;
                    explosionFX.SetActive(true);
                    fireFX.SetActive(true);
                }
                else if (ClientCommands.Instance.tank1OnRunning != lastMovingStatus)
                {
                    lastMovingStatus = ClientCommands.Instance.tank1OnRunning;
                    Walking(lastMovingStatus);
                }
                break;
            case 2:
                if (ClientCommands.Instance.tank2OnRunning != lastMovingStatus && !ClientCommands.Instance.tank1OnRunning && !ClientCommands.Instance.tank3OnRunning)
                {
                    lastMovingStatus = false;
                    explosionFX.SetActive(true);
                    fireFX.SetActive(true);
                }
                else if(ClientCommands.Instance.tank2OnRunning != lastMovingStatus)
                {
                    lastMovingStatus = ClientCommands.Instance.tank2OnRunning;
                    Walking(lastMovingStatus);
                }
                break;
            case 3:
                if (ClientCommands.Instance.tank3OnRunning != lastMovingStatus && !ClientCommands.Instance.tank2OnRunning && !ClientCommands.Instance.tank1OnRunning)
                {
                    lastMovingStatus = false;
                    explosionFX.SetActive(true);
                    fireFX.SetActive(true);
                }
                else if(ClientCommands.Instance.tank3OnRunning != lastMovingStatus)
                {
                    lastMovingStatus = ClientCommands.Instance.tank3OnRunning;
                    Walking(lastMovingStatus);
                }
                break;
        }
        if (selected != selectImage.activeSelf) selectImage.SetActive(selected);

    }

    public void Stop()
    {
        tween.Stop();
        fireFX.SetActive(false);
        Debug.Log($"Stop Obrigatorio, tweeeen: {tween.isAlive}");
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

    public void Reset()
    {
        tween = Tween.UIAnchoredPositionX(tankTrasform, endValue: 0, duration: 2);
        fireFX.SetActive(false);
    }

    public IEnumerator GoBack(RectTransform tank)
    {
        tween.Stop();
        tween = Tween.UIAnchoredPositionX(tank, endValue: -(Screen.width/2) , duration: 6)
            .OnComplete(this, target => { 
                fireFX.SetActive(false); 
                tween = Tween.UIAnchoredPositionX(tank, endValue: -(Screen.width / 2), duration: 0); });
        explosionFX.SetActive(true);
        fireFX.SetActive(true);
        yield return null;
        Debug.Log("End GoBack");
    }
    public IEnumerator GoCenter()
    {
        tween.Stop();
        fireFX.SetActive(false);
        tween = Tween.UIAnchoredPositionX(tankTrasform, endValue: 0, duration: 2)
                .OnComplete(this, target => {StartCoroutine(MovingAuto());});
        yield return null;
        Debug.Log("End GoCenter");
    }
    public IEnumerator MovingAuto()
    {
        var n = Random.Range(0, 2);
        while (lastMovingStatus)
        {
            tween.Stop();
            int d = Random.Range(2, 6);
            if (n == 0) 
            {
                tween = Tween.UIAnchoredPositionX(tankTrasform, endValue:Random.Range(50,75), duration:d);
                yield return new WaitForSeconds(d);
            }
            else
            {
                tween = Tween.UIAnchoredPositionX(tankTrasform, endValue: Random.Range(-75, -50), duration: d+3);
                yield return new WaitForSeconds(d+3);
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
