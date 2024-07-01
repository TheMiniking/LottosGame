using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RematchTankUI : MonoBehaviour
{
    public TMP_Text txtTank;
    public Image imgTank;
    public Button btnRematch;

    byte tank = 0;
    float valor = 0;

    private void Awake()
    {
        btnRematch = GetComponent<Button>();
    }

    private void Start()
    {
        btnRematch.onClick.AddListener(BetRemach);
    }

    void OnDisable()
    {
        btnRematch.onClick.RemoveListener(BetRemach);
    }

    public void SetRematch(byte tank, float valor)
    {
        imgTank.sprite = CanvasManager.Instance.tankSprites[tank];
        txtTank.text = $"ExtraBet :{GameManager.Instance.MoedaAtual(valor):#,0.00}";
        this.tank = tank;
        this.valor = valor;
    }

    public void BetRemach()
    {
        ClientCommands.Instance.SendBet(tank);
        CanvasManager.Instance.rematchTanks.ForEach(x => x.gameObject.SetActive(false));
        CanvasManager.Instance.canRematch = false;
    }
}
