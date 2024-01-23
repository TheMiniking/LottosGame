using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ServerHUD : MonoBehaviour
{
    [SerializeField] TMP_Text lastsResult, multiplier, atualFase, totalIn, totalOut;

    public void UpdateLastResult(List<string> results)
    {
        lastsResult.text = "";
        foreach (var result in results)
        {
            lastsResult.text +=$" [ {result} ] " ;
        }
    }

    public void UpdateMultiplier(float multiplier) => this.multiplier.text = $"x {multiplier:0.00}";

    public void UpdateTotalIn(float totalIn) => this.totalIn.text = $"{totalIn:0.00} C";

    public void UpdateTotalOut(float totalOut) => this.totalOut.text = $"{totalOut:0.00} C";

    public void UpdateAtualFase(int atualFase) => this.atualFase.text = $"Fase {atualFase}";

}
