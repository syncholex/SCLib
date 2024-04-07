/*
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ProtoBuf;

[ProtoContract]
public class E
{
    [ProtoMember(1)]
    public string Field1 { get; set; }

    [ProtoMember(2)]
    public string Field2 { get; set; }
}

[ProtoContract]
public class D
{
    [ProtoMember(2)]
    public int Field1 { get; set; }

    [ProtoMember(4)]
    public E Field2 { get; set; }

    [ProtoMember(13)]
    public int Field3 { get; set; }
}


[ProtoContract]
public class C
{
    [ProtoMember(1)]
    public D Field1 { get; set; }

    [ProtoMember(3)]
    public string Field2 { get; set; }
}



[ProtoContract]
public class B
{
    [ProtoMember(1)]
    public C Field1 { get; set; }
}

[ProtoContract]
public class A
{
    [ProtoMember(6)]
    public B Field1 { get; set; }
}



[ProtoContract]
public class b
{
    [ProtoMember(2)]
    public string Field1 { get; set; }
}


[ProtoContract]
public class a
{
    [ProtoMember(2)]
    public b Field1 { get; set; }
}

public class Program
{
    public static async Task Main()
    {
        A messageContainer;
        using (var stream = new MemoryStream(Convert.FromBase64String("MlcKVQpPEAQiSAosd3c0TVh3ZnBKb0hJa3BzVXpienJZU0RtSnFyNXF1N3dFcE1QRUtYU3dlMD0SGHIzOUg3UHIrNC8wM0RJQlJRSWswQlE9PWiqDBoCZW4=")))
        {
            messageContainer = Serializer.Deserialize<A>(stream);
        }

        byte[] key = Convert.FromBase64String(messageContainer.Field1.Field1.Field1.Field2.Field1);
        byte[] iv = Convert.FromBase64String(messageContainer.Field1.Field1.Field1.Field2.Field2);


        a mc;
        using (var stream = new MemoryStream(Convert.FromBase64String("EiQSFW1ieEVSWHZEWnd2MmxRb2Jtc29LRRoAGgAyAQhIAlAEYAE=")))
        {
            mc = Serializer.Deserialize<a>(stream);
        }

        string url = "https://cf-st.sc-cdn.net/h/" + mc.Field1.Field1;

        byte[] decryptedStream = await DownloadAndDecryptFile(url, key, iv);

        File.WriteAllBytes("note.mp3", decryptedStream);
    }

    static async Task<byte[]> DownloadAndDecryptFile(string url, byte[] key, byte[] iv)
    {
        using (WebClient webClient = new WebClient())
        {
            byte[] encryptedData = webClient.DownloadData(url);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                using (MemoryStream decryptedStream = new MemoryStream())
                using (CryptoStream cryptoStream = new CryptoStream(decryptedStream, aesAlg.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(encryptedData, 0, encryptedData.Length);
                    cryptoStream.FlushFinalBlock();

                    return decryptedStream.ToArray();
                }
            }
        }
    }
}
*/