using System.Security.Cryptography;
using System.Text;

public class AesCrypto
{
    private readonly byte[] _key; // 32 bytes

    public AesCrypto(string keyBase64)
    {
        if (string.IsNullOrWhiteSpace(keyBase64))
            throw new InvalidOperationException("EmailCrypto:KeyBase64 is missing.");

        keyBase64 = keyBase64.Trim();

        byte[] key;
        try
        {
            key = Convert.FromBase64String(keyBase64);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("EmailCrypto:KeyBase64 must be a valid Base64 string.");
        }

        if (key.Length != 32)
            throw new InvalidOperationException($"EmailCrypto:KeyBase64 must decode to 32 bytes, got {key.Length} bytes.");

        _key = key;
    }

    public (byte[] cipher, byte[] iv) Encrypt(string plain)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var enc = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plain);
        var cipher = enc.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return (cipher, aes.IV);
    }

    public string Decrypt(byte[] cipher, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var dec = aes.CreateDecryptor(aes.Key, aes.IV);
        var plainBytes = dec.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
