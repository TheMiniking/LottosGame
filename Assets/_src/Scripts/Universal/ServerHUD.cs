using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerHUD : MonoBehaviour
{
    [SerializeField] TMP_Text lastsResult, multiplier, bonus, atualFase , totalIn, totalOut, serverStatus, totalPlayers;
    [SerializeField] Image serverStatusBG;
    [SerializeField] List<float> results = new();
    [SerializeField] float creditsIn, creditsOut;

    private void Start()
    {
        UpdateLastResult(0);
    }

    public void UpdateLastResult(float resultAdd)
    {
        if (resultAdd == 0) { lastsResult.text = ""; results.Clear(); return; }
        results.Add(resultAdd);
        if (results.Count > 9) results.RemoveAt(0);
        lastsResult.text = "";
        foreach (var result in results)
        {
            lastsResult.text +=$" [ {result:0.00} ] " ;
        }
    }

    public void UpdateMultiplier(float multiplier, float? bonus)
    {
        this.multiplier.text = $"x {multiplier:0.00}";
        if(bonus != null) this.bonus.text = $"x {bonus:0.00}";
    }

    public void UpdateTotalIn(float totalIn)
    {
        creditsIn += totalIn;
        this.totalIn.text = $"{creditsIn:0.00} C";
    }

    public void UpdateTotalOut(float totalOut)
    {
        creditsOut += totalOut;
        this.totalOut.text = $"{creditsOut:0.00} C";
    }

    public void UpdateAtualFase(string atualFase) => this.atualFase.text = $"--- {atualFase} ---";

    public void ServerStatus(bool status)
    {
        serverStatus.text = status ? "Server On" : "Server OFF";
        serverStatusBG.color = status ?new Color(0.5f,1f,0.5f,0.5f) : new Color(1, 0.5f, 0.5f, 0.5f);
    }

    public void TotalPlayers(int p) => this.totalPlayers.text = $"Total Connected Players : {p}"; 

}
