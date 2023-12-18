using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wallet", menuName = "Zyan Asset/Wallet", order = 1)]
public class Wallet : ScriptableObject
{
    public PlayerWallet player;

    public void StartWalletplusCoin()
    {
        player = new PlayerWallet()
        {
            address = $"0x{UnityEngine.Random.Range(0, 999999):X6}",
            coins = new List<Coin>()
            {
                { new Coin() { name = "Bitcoin", symbol = "BTC", contract = "0x0000000000000000000000000000000000000000", balance = 10 } },
                { new Coin() { name = "Ethereum", symbol = "ETH", contract = "0x0000000000000000000000000000000000000000", balance = 10 } },
                { new Coin() { name = "Litecoin", symbol = "LTC", contract = "0x0000000000000000000000000000000000000000", balance = 10 } },
                { new Coin() { name = "Binance Coin", symbol = "BNB", contract = "0x0000000000000000000000000000000000000000", balance = 10 } }
            }
        };
        UpdateAtualCoin("BNB");
    }

    public void UpdateAtualCoin(string coin)
    {
        player.atualCoin = coin;
        player.coinBalance = player.coins.Find(x => x.symbol == coin).balance;
        Debug.Log("Update Coin to " + coin);
    }
    public void PayBet(Bet bet)
    {
        player.coins.Find(x => x.symbol == bet.coin).balance -= bet.value;
        UpdateAtualCoin(bet.coin);
        Debug.Log("Pay bet " + bet.value);
    }

    public void WinBet(Bet bet)
    {
        player.coins.Find(x => x.symbol == bet.coin).balance += bet.value;
        UpdateAtualCoin(bet.coin);
        Debug.Log("Win Bet " + bet.value);
    }

    public float CalculateBetAddToBalance(Bet bet , float multiplier)
    {
        player.coins.Find(x => x.symbol == bet.coin).balance += bet.value *(multiplier*100);
        UpdateAtualCoin(bet.coin);
        Debug.Log($"Add to Balance x{ multiplier * 100:0.00} Reward : {bet.value * multiplier * 100:0.0000}" );
        return bet.value * multiplier;
    }

    public bool CheckBalance(float value ,string coin)
    {
        if (player.coins.Find(x => x.symbol == coin).balance >= value)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
[Serializable]
public class PlayerWallet
{
    public string address;
    public string atualCoin;
    public float coinBalance;
    public List<Coin> coins;

}

[Serializable]
public class Coin
{
    public string name;
    public string symbol;
    public string contract;
    public float balance;
}
[Serializable]
public class Bet
{
    public string addressID;
    public string coin;
    public float value;
    public float stop;
    public bool autoStop = false;
    public bool winBet = false;
}