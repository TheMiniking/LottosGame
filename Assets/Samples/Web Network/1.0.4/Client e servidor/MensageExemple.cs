
using Serializer;
using System;

[Serializable]
public class MensageExemple : INetSerializable
{
    public string value1;
    public int value2;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref value1);
        reader.Get(ref value2);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(value1);
        write.Put(value2);
    }
}

public class StartRun : INetSerializable
{
    public void Deserialize(DataReader reader)
    {
    }

    public void Serialize(DataWriter write)
    {
    }
}
public class Crash : INetSerializable
{
    public float multply;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref multply);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(multply);
    }
}
public class TimerSync : INetSerializable
{
    public float time;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref time);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(time);
    }
}
public class MultSync : INetSerializable
{
    public float mult;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref mult);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(mult);
    }
}
public class BetServer : INetSerializable
{
    public float value;
    public float stop;
    public bool AutoStop => stop > 0;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref value);
        reader.Get(ref stop);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(value);
        write.Put(stop);
    }
}
public class StopBet : INetSerializable
{
    public byte a;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref a);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(a);
    }
}

public class MensageControl : INetSerializable
{
    public string msg;
    public float valor;
    public int useValor = -1;// -1 = Somente debug, 0 = false, 1 = true
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref msg);
        reader.Get(ref valor);
        reader.Get(ref useValor);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(msg);
        write.Put(valor);
        write.Put(useValor);
    }
}

public class Balance : INetSerializable
{
    public string msg;
    public float valor;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref msg);
        reader.Get(ref valor);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(msg);
        write.Put(valor);
    }
}

public class Parallax : INetSerializable
{
    public float velocidade;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref velocidade);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(velocidade);
    }
}
public class Box : INetSerializable
{
    public float bonus;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref bonus);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(bonus);
    }
}
public class ButtonBet : INetSerializable
{
    public string txt;
    public bool active;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref txt);
        reader.Get(ref active);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(txt);
        write.Put(active);
    }
}
public class BalanceCreditServer : INetSerializable
{
    public float valor;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref valor);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(valor);
    }
}
public class BalanceCreditClient : INetSerializable
{
    public float valor;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref valor);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(valor);
    }
}
public class SetBet : INetSerializable
{
    public float valor;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref valor);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(valor);
    }
}

public class Login : INetSerializable
{
    public string token;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref token);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(token);
    }
}

[Serializable]
public class BetPlayers : INetSerializable
{
    public string name;
    public double value;
    public float multiplier;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref name);
        reader.Get(ref value);
        reader.Get(ref multiplier);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(name);
        write.Put(value);
        write.Put(multiplier);
    }
}

[Serializable]
public class AddBonus : INetSerializable
{
    public float valor;
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref valor);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(valor);
    }
}
[Serializable]
public class Ranking : INetSerializable
{
    public Line[] rankValue;
    public Line[] rankMulti;


    public void Serialize(DataWriter writer)
    {
        writer.Put(rankValue);
        writer.Put(rankMulti);
    }
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref rankValue);
        reader.Get(ref rankMulti);
    }
}
[Serializable]
public class Line : INetSerializable
{
    public string name;
    public float multi;
    public byte bet;
    public void Serialize(DataWriter writer)
    {
        writer.Put(name);
        writer.Put(multi);
        writer.Put(bet);
    }
    public void Deserialize(DataReader reader)
    {
        reader.Get(ref name);
        reader.Get(ref multi);
        reader.Get(ref bet);
    }
}

[Serializable]
public class LastMulti : INetSerializable
{
    public float[] multis;

    public void Deserialize(DataReader reader)
    {
        reader.Get(ref multis);
    }

    public void Serialize(DataWriter write)
    {
        write.Put(multis);
    }
}
enum ReceiveMsgIdc
{
    GameLoginResponse = 1,
    BalanceResponse = 2,
    PlayResponse = 3,
    NextBetResponse = 4,
    ErrorResponse = 5,
    BetPlayers = 6,
    Ranking = 7,
    LastMulti = 8
}
enum SendMsgIdc
{
    PlayRequest = 1,
    NextBet = 2,
    GetBalance = 3,
    GetRanking = 4
}