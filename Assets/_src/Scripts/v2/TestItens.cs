using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

public class TestItens : MonoBehaviour
{
    public List<Player> tanks;
    public RectTransform aviao;
    public List<GameObject> bonus;
    public float aviaoDistance;

    public void SetNewLastRound()
    {
        float[] f = new float[] { Random.Range(0f, 6f), Random.Range(0f, 6f), Random.Range(0f, 6f) };
        CanvasManager.Instance.SetLastPlays(new LastMultiTriple { multis = f });
    }

    public void SelectTankAleatorio ()
    {
        var i = Random.Range(0, tanks.Count);
        CanvasManager.Instance.SelectTank(i);
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
}
