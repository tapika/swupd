using System;
using System.Data;
using System.Security.Cryptography;
using System.IO;

/// <summary>
/// Simple class for encoding and decoding data using same algorithm.
/// 
/// https://stackoverflow.com/a/212707/2338477
/// 
/// Key/Vector used in this class are randomly generated.
/// 
/// Eariler version was using ProtectedData.Protect / ProtectedData.Unprotect
/// - but apparently making it portable is not so trivial from .net core perspective.
/// 
/// See https://github.com/dotnet/runtime/issues/22886
/// </summary>
public class SimpleAES
{
    // 
    // Keys generated randomly.

    private byte[] Key = new byte[]
    {
        114, 224, 32, 73, 89, 239, 177, 79, 126, 108, 207, 196, 148, 37, 132, 62,
        133, 107, 20, 210, 87, 200, 161, 200, 31, 108, 31, 245, 33, 84, 182, 231
    };

    private byte[] Vector = new byte[]
    {
        48, 41, 4, 253, 161, 19, 4, 213, 1, 102, 26, 165, 30, 95, 104, 170
    };

    private ICryptoTransform EncryptorTransform, DecryptorTransform;
    private System.Text.UTF8Encoding UTFEncoder;

    public SimpleAES()
    {
        //This is our encryption method
        RijndaelManaged rm = new RijndaelManaged();

        //Create an encryptor and a decryptor using our encryption method, key, and vector.
        EncryptorTransform = rm.CreateEncryptor(this.Key, this.Vector);
        DecryptorTransform = rm.CreateDecryptor(this.Key, this.Vector);

        //Used to translate bytes to text and vice versa
        UTFEncoder = new System.Text.UTF8Encoding();
    }

    /// -------------- Two Utility Methods (not used but may be useful) -----------
    /// Generates an encryption key.
    static public byte[] GenerateEncryptionKey()
    {
        //Generate a Key.
        RijndaelManaged rm = new RijndaelManaged();
        rm.GenerateKey();
        return rm.Key;
    }

    /// Generates a unique encryption vector
    static public byte[] GenerateEncryptionVector()
    {
        //Generate a Vector
        RijndaelManaged rm = new RijndaelManaged();
        rm.GenerateIV();
        return rm.IV;
    }


    /// ----------- The commonly used methods ------------------------------    
    /// Encrypt some text and return a string suitable for passing in a URL.
    public string EncryptToString(string TextValue)
    {
        return ByteArrToString(Encrypt(TextValue));
    }

    /// Encrypt some text and return an encrypted byte array.
    public byte[] Encrypt(Byte[] bytes)
    {
        //Used to stream the data in and out of the CryptoStream.
        MemoryStream memoryStream = new MemoryStream();

        /*
         * We will have to write the unencrypted bytes to the stream,
         * then read the encrypted result back from the stream.
         */
        #region Write the decrypted value to the encryption stream
        CryptoStream cs = new CryptoStream(memoryStream, EncryptorTransform, CryptoStreamMode.Write);
        cs.Write(bytes, 0, bytes.Length);
        cs.FlushFinalBlock();
        #endregion

        #region Read encrypted value back out of the stream
        memoryStream.Position = 0;
        byte[] encrypted = new byte[memoryStream.Length];
        memoryStream.Read(encrypted, 0, encrypted.Length);
        #endregion

        //Clean up.
        cs.Close();
        memoryStream.Close();

        return encrypted;
    }

    public byte[] Encrypt(string TextValue)
    {
        //Translates our text value into a byte array.
        Byte[] bytes = UTFEncoder.GetBytes(TextValue);
        return Encrypt(bytes);
    }

    /// The other side: Decryption methods
    public string DecryptString(string EncryptedString)
    {
        return Decrypt(StrToByteArray(EncryptedString));
    }

    /// Decryption when working with byte arrays.    
    public byte[] DecryptRaw(byte[] EncryptedValue)
    {
        #region Write the encrypted value to the decryption stream
        MemoryStream encryptedStream = new MemoryStream();
        CryptoStream decryptStream = new CryptoStream(encryptedStream, DecryptorTransform, CryptoStreamMode.Write);
        decryptStream.Write(EncryptedValue, 0, EncryptedValue.Length);
        decryptStream.FlushFinalBlock();
        #endregion

        #region Read the decrypted value from the stream.
        encryptedStream.Position = 0;
        Byte[] decryptedBytes = new Byte[encryptedStream.Length];
        encryptedStream.Read(decryptedBytes, 0, decryptedBytes.Length);
        encryptedStream.Close();
        #endregion
        return decryptedBytes;
    }

    public string Decrypt(byte[] EncryptedValue)
    {
        return UTFEncoder.GetString(DecryptRaw(EncryptedValue));
    }

    /// Convert a string to a byte array.  NOTE: Normally we'd create a Byte Array from a string using an ASCII encoding (like so).
    //      System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
    //      return encoding.GetBytes(str);
    // However, this results in character values that cannot be passed in a URL.  So, instead, I just
    // lay out all of the byte values in a long string of numbers (three per - must pad numbers less than 100).
    public byte[] StrToByteArray(string str)
    {
        if (str.Length == 0)
            throw new Exception("Invalid string value in StrToByteArray");

        byte val;
        byte[] byteArr = new byte[str.Length / 3];
        int i = 0;
        int j = 0;
        do
        {
            val = byte.Parse(str.Substring(i, 3));
            byteArr[j++] = val;
            i += 3;
        }
        while (i < str.Length);
        return byteArr;
    }

    // Same comment as above.  Normally the conversion would use an ASCII encoding in the other direction:
    //      System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
    //      return enc.GetString(byteArr);    
    public string ByteArrToString(byte[] byteArr)
    {
        byte val;
        string tempStr = "";
        for (int i = 0; i <= byteArr.GetUpperBound(0); i++)
        {
            val = byteArr[i];
            if (val < (byte)10)
                tempStr += "00" + val.ToString();
            else if (val < (byte)100)
                tempStr += "0" + val.ToString();
            else
                tempStr += val.ToString();
        }
        return tempStr;
    }
}


