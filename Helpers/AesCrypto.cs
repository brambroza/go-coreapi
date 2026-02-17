using System.Security.Cryptography;
using System.Text;

public class AesCrypto
{
    private readonly byte[] _key; // 32 bytes

    public AesCrypto(string keyBase64)
    {
        _key = Convert.FromBase64String(keyBase64);
        if (_key.Length != 32) throw new ArgumentException("Key must be 32 bytes (Base64).");
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
