/*
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;


[ProtoContract]
public class G
{
    [ProtoMember(1)]
    public string Field1 { get; set; }
    [ProtoMember(2)]
    public string Field2 { get; set; }
}





[ProtoContract]
public class F
{
    [ProtoMember(4)]
    public G Field1 { get; set; }
}






[ProtoContract]
public class E
{
    [ProtoMember(1)]
    public F Field1 { get; set; }
}

[ProtoContract]
public class D
{
    [ProtoMember(1)]
    public E Field1 { get; set; }
}


[ProtoContract]
public class C
{
    [ProtoMember(5)]
    public D Field1 { get; set; }
}



[ProtoContract]
public class B
{
    [ProtoMember(3)]
    public C Field1 { get; set; }
}

[ProtoContract]
public class A
{
    [ProtoMember(3)]
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
        using (var stream = new MemoryStream(Convert.FromBase64String("GqQBGqEBKpwBCpUBCpIBIkgKLDVLSjM1dTlBTENHMUoza0RYVXV3dzVWcDhPcjAyVVI4RE91OGdHK050Y3c9EhhoSElTQkR3R0thNjNSczVTTTY4dUJBPT0qBgjwBxDACnigH5IBAJoBNAog5KJ35u9ALCG1J3kDXUuww5Vp8Or02UR8DOu8gG+NtcwSEIRyEgQ8Bimut0bOUjOvLgSgAQUSAkAEagA=")))
        {
            messageContainer = Serializer.Deserialize<A>(stream);
        }

        byte[] key = Convert.FromBase64String(messageContainer.Field1.Field1.Field1.Field1.Field1.Field1.Field1);
        byte[] iv = Convert.FromBase64String(messageContainer.Field1.Field1.Field1.Field1.Field1.Field1.Field2);

        a mc;
        using (var stream = new MemoryStream(Convert.FromBase64String("EiQSFXNhOEYzUE0wRGJIdHlaY2hGN0NmQRoAGgAyAQhIAlAEYAE=")))
        {
            mc = Serializer.Deserialize<a>(stream);
        }

        string url = "https://cf-st.sc-cdn.net/h/" + mc.Field1.Field1;

        byte[] decryptedStream = await DownloadAndDecryptFile(url, key, iv);

        File.WriteAllBytes("image.png", decryptedStream);
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