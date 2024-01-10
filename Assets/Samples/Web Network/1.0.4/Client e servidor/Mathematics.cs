using Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


public static class Mathematics 
{
    public static TankRound TankCalculeRound(TankConfiguration tank)
    {
        var luck = UnityEngine.Random.Range(0, 101);
        var range = luck <= tank.bestChance ? UnityEngine.Random.Range(0.02f, tank.maxRange) : luck <= tank.greatChance+tank.bestChance ? UnityEngine.Random.Range(0.02f, tank.maxRange/2) : UnityEngine.Random.Range(0.02f, tank.maxRange/4);
        var multiplicador = 0.01f;
        var userMult = 1.0f;
        var listBonus = new List<float>();
        bool exitBomb = false;
        for (int i = 0; i < range*100; i++)
        {
            if (i % 20 == 0)
            {
                multiplicador = multiplicador < tank.maxMultiplicador ? multiplicador + 0.01f : multiplicador;
                if(tank.bombChance > 0)
                    listBonus.Add(tank.bonusList[UnityEngine.Random.Range(0,tank.bonusList.Count>=tank.bombChance?tank.bombChance:tank.bonusList.Count)]);
            }
            userMult += multiplicador;
            listBonus.ForEach(x => { if (x == 0) exitBomb = true; });
            if (exitBomb) break;
        }
        return new TankRound() { timeRound = userMult, bonus = listBonus , exitBomb = exitBomb};
    }

    
}

public class TankRound
{
    public float timeRound;
    public List<float> bonus;
    public bool exitBomb;
}
[Serializable]
public class TankConfiguration : INetSerializable
{
    public int bestChance;
    public int greatChance;
    public List<float> bonusList;
    public int bombChance;
    public float maxRange;
    public float maxMultiplicador;
    public int timeWait;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref bestChance);
        reader.Get(ref greatChance);
        reader.Get(ref maxRange);
        reader.Get(ref maxMultiplicador);
        reader.Get(ref bombChance);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(bestChance);
        write.Put(greatChance);
        write.Put(maxRange);
        write.Put(maxMultiplicador);
        write.Put(bombChance);
    }
}