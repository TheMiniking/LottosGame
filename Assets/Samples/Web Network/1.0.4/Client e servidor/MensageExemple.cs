
using Serializer;

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

