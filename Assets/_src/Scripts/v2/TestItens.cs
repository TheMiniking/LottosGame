using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestItens : MonoBehaviour
{
    public List<Player> tanks;

    public void SetNewLastRound()
    {
        float[] f = new float[] { Random.Range(1f, 999f), Random.Range(1f, 999f), Random.Range(1f, 999f) };
        CanvasManager.Instance.SetLastPlays(new LastMultiTriple { multis = f });
    }

    public void SelectTankAleatorio ()
    {
        Player tank = tanks[Random.Range(0, tanks.Count)];
        tanks.ForEach(x => x.selected = x == tank );
    }
}
