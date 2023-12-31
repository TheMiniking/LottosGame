﻿
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
        write.Put(value2) ;
    }
}

public class StartRun: INetSerializable
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
    public bool  AutoStop { get => stop > 0;}

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