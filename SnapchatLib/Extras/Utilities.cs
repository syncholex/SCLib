﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Asn1;
using SnapchatLib.Exceptions;

namespace SnapchatLib.Extras;

internal interface IUtilities
{
    long LongRandom(long min, long max, Random rand);
    ulong NextULong(ulong min, ulong max);
    string GenerateTemporaryRequestToken(string timestamp);
    string RandomString(int length);
    string RandomBlizzardSessionId(int length);
    T JsonDeserializeObject<T>(string data);
    string JsonSerializeObject(object obj);
    string NewGuid();
    Guid ParseGuid(string uuid);
    long UtcTimestamp();
    int GetAge(DateTime dateOfBirth);
    string UnwrapPublicKey(string publicKey);
    long GetInstallTimeStamp(long install_min);
    string ToHexWithoutPrefix(long number);
    byte[] GenerateRandomBytes(int length);
}

internal class Utilities : IUtilities
{
    private readonly Random m_Random = new();

    public byte[] GenerateRandomBytes(int length)
    {
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] randomBytes = new byte[length];
            rng.GetBytes(randomBytes);
            return randomBytes;
        }
    }

    public class AndroidIDConverter
    {
        public static ulong AndroidIDToLong(string androidID)
        {
            if (string.IsNullOrWhiteSpace(androidID) || androidID.Length != 16)
            {
                throw new ArgumentException("Invalid Android ID format");
            }

            if (!ulong.TryParse(androidID, System.Globalization.NumberStyles.HexNumber, null, out ulong value))
            {
                throw new ArgumentException("Failed to parse Android ID");
            }

            return value;
        }
    }
    public class AndroidIDGenerator
    {
        public static string GenerateAndroidID()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[8];
                rng.GetBytes(bytes);

                ulong value = BitConverter.ToUInt64(bytes, 0);
                return value.ToString("x16");
            }
        }
    }
    public string UnwrapPublicKey(string publicKey)
    {
        var publicKeyData = Convert.FromBase64String(publicKey);

        using (var memory = new MemoryStream(publicKeyData))
        using (var asn1 = new Asn1InputStream(memory))
        {
            var sequence = (DerSequence)asn1.ReadObject();
            var octetStringTwo = (DerBitString)sequence[1];

            return Convert.ToBase64String(octetStringTwo.GetBytes());
        }
    }
    public long LongRandom(long min, long max, Random rand)
    {
        byte[] buf = new byte[8];
        rand.NextBytes(buf);
        long longRand = BitConverter.ToInt64(buf, 0);
        return (Math.Abs(longRand % (max - min)) + min);
    }

    public ulong NextULong(ulong min, ulong max)
    {
        // Get a random 64 bit number.

        var buf = new byte[sizeof(ulong)];
        m_Random.NextBytes(buf);
        ulong n = BitConverter.ToUInt64(buf, 0);

        // Scale to between 0 inclusive and 1 exclusive; i.e. [0,1).

        double normalised = n / (ulong.MaxValue + 1.0);

        // Determine result by scaling range and adding minimum.

        double range = (double)max - min;

        return (ulong)(normalised * range) + min;
    }
    public string GenerateTemporaryRequestToken(string timestamp)
    {
        var s1 = "iEk21fuwZApXlz93750dmW22pw389dPwOk" + "m198sOkJEn37DjqZ32lpRu76xmw288xSQ9";
        var s2 = timestamp + "iEk21fuwZApXlz93750dmW22pw389dPwOk";

        var s3 = ComputeSha256Hash(s1);
        var s4 = ComputeSha256Hash(s2);

        var output = new StringBuilder();
        var pattern = "0001110111101110001111010101111011010001001110011000110001000110";

        for (var i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];
            output.Append(c == '0' ? s3[i] : s4[i]);
        }

        return output.ToString();
    }

    private string ComputeSha256Hash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }

    public int GetAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;

        var a = (today.Year * 100 + today.Month) * 100 + today.Day;
        var b = (dateOfBirth.Year * 100 + dateOfBirth.Month) * 100 + dateOfBirth.Day;

        return (a - b) / 10000;
    }
    public string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[m_Random.Next(s.Length)]).ToArray());
    }
    public string RandomBlizzardSessionId(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[m_Random.Next(s.Length)]).ToArray());
    }
    public T JsonDeserializeObject<T>(string data)
    {

        var result = JsonSerializer.Deserialize<T>(data, new JsonSerializerOptions
        {
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        if (result == null) throw new DeserializationException(nameof(T));
        return result;
    }

    public string JsonSerializeObject(object obj)
    {
        try
        {
            return JsonSerializer.Serialize(obj);
        }
        catch (Exception e)
        {
            throw new SerializationException("Failed to serialize object", e);
        }
    }

    public string ToHexWithoutPrefix(long number)
    {
        string hexRepresentation = number.ToString("X");
        hexRepresentation = hexRepresentation.TrimStart('0'); // Remove leading zeros
        return hexRepresentation;
    }

    public string NewGuid()
    {
        return Guid.NewGuid().ToString();
    }

    public Guid ParseGuid(string uuid)
    {
        return Guid.Parse(uuid);
    }
    public long GetInstallTimeStamp(long install_min)
    {
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long oneHourAgoTimestamp = currentTimestamp - (60 * 60 * 1000);
        return new Random().NextInt64(install_min, oneHourAgoTimestamp);
    }
    public long UtcTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}