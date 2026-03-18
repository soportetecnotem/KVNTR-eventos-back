using System.Security.Cryptography;
using System.Text;

namespace EventosBack.Servicios
{
    public class CoDeRnd
    {
        private const string DEFAULT_HASH_ALGORITHM = "SHA512";
        private const int DEFAULT_KEY_SIZE = 256;
        private const int MAX_ALLOWED_SALT_LEN = 255;
        private const int MIN_ALLOWED_SALT_LEN = 4;
        private const int DEFAULT_MIN_SALT_LEN = MIN_ALLOWED_SALT_LEN;
        private const int DEFAULT_MAX_SALT_LEN = 8;
        private int minSaltLen = -1;
        private int maxSaltLen = -1;
        private ICryptoTransform? encryptor = null;
        private ICryptoTransform? decryptor = null;

        public CoDeRnd()
        {
        }

        public CoDeRnd(string passPhrase)
            : this(passPhrase, "@1B2c3D4e5F6g7H8") { }


        public CoDeRnd(string passPhrase, string initVector)
            : this(passPhrase, initVector, -1) { }

        public CoDeRnd(string passPhrase, string initVector, int minSaltLen)
            : this(passPhrase, initVector, minSaltLen, -1) { }

        public CoDeRnd(string passPhrase, string initVector, int minSaltLen, int maxSaltLen)
            : this(passPhrase, initVector, minSaltLen, maxSaltLen, -1) { }

        public CoDeRnd(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize)
            : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, null) { }

        public CoDeRnd(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm)
            : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, hashAlgorithm, null) { }

        public CoDeRnd(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm, string saltValue)
            : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, hashAlgorithm, saltValue, 1) { }

        public CoDeRnd(string passPhrase, string initVector, int minSaltLenP, int maxSaltLenP, int keySize, string hashAlgorithm, string saltValue, int passwordIterations)
        {
            if (string.IsNullOrEmpty(initVector))
            {
                initVector = "@1B2c3D4e5F6g7H8";
            }

            minSaltLen = minSaltLenP < MIN_ALLOWED_SALT_LEN ? DEFAULT_MIN_SALT_LEN : minSaltLenP;

            if (maxSaltLenP < 0 || maxSaltLen > MAX_ALLOWED_SALT_LEN)
            {
                maxSaltLen = DEFAULT_MAX_SALT_LEN;
            }
            else
            {
                maxSaltLen = maxSaltLenP;
            }

            keySize = keySize <= 0 ? DEFAULT_KEY_SIZE : keySize;

            if (hashAlgorithm == null)
            {
                hashAlgorithm = DEFAULT_HASH_ALGORITHM;
            }
            else
            {
                hashAlgorithm = hashAlgorithm.ToUpper().Replace("-", "");
            }

            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] saltValueBytes = saltValue == null ? new byte[] { } : Encoding.ASCII.GetBytes(saltValue);

            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);
            byte[] keyBytes = password.GetBytes(keySize / 8);

            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            symmetricKey.Padding = PaddingMode.PKCS7;

            encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
            {
                return string.Empty;
            }

            string encryptedText = EncryptToBase64String(Encoding.UTF8.GetBytes(plainText));

            while (encryptedText.IndexOf("+") != -1 || encryptedText.IndexOf("&") != -1)
            {
                encryptedText = EncryptToBase64String(Encoding.UTF8.GetBytes(plainText));
            }

            return encryptedText;
        }

        private string EncryptToBase64String(byte[] plainTextBytes)
        {
            byte[] plainTextBytesWithSalt = AddSalt(plainTextBytes);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytesWithSalt, 0, plainTextBytesWithSalt.Length);
                    cryptoStream.FlushFinalBlock();
                    byte[] cipherTextBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(cipherTextBytes);
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                return string.Empty;
            }
            try
            {
                return DecryptFromBase64String(cipherText);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        private string DecryptFromBase64String(string cipherText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            byte[] decryptedBytes = DecryptToBytes(cipherTextBytes);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        private byte[] DecryptToBytes(byte[] cipherTextBytes)
        {
            byte[] decryptedBytes = null;
            byte[] plainTextBytes = null;
            int decryptedByteCount = 0, saltLen = 0;

            using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
            {
                Array.Resize(ref decryptedBytes, cipherTextBytes.Length - 1);

                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    decryptedByteCount = cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);
                }
            }

            if (maxSaltLen > 0 && maxSaltLen >= minSaltLen)
            {
                saltLen = (decryptedBytes[0] & 0x3) | (decryptedBytes[1] & 0xc) | (decryptedBytes[2] & 0x30) | (decryptedBytes[3] & 0xc0);
            }

            Array.Resize(ref plainTextBytes, decryptedByteCount - saltLen);
            Array.Copy(decryptedBytes, saltLen, plainTextBytes, 0, decryptedByteCount - saltLen);

            return plainTextBytes;
        }

        private byte[] AddSalt(byte[] plainTextBytes)
        {
            if (maxSaltLen == 0 || maxSaltLen < minSaltLen)
            {
                return plainTextBytes;
            }

            byte[] saltBytes = GenerateSalt();
            byte[] plainTextBytesWithSalt = new byte[plainTextBytes.Length + saltBytes.Length];
            Array.Copy(saltBytes, plainTextBytesWithSalt, saltBytes.Length);
            Array.Copy(plainTextBytes, 0, plainTextBytesWithSalt, saltBytes.Length, plainTextBytes.Length);
            return plainTextBytesWithSalt;
        }

        private byte[] GenerateSalt()
        {
            int saltLen = minSaltLen == maxSaltLen ? minSaltLen : GenerateRandomNumber(minSaltLen, maxSaltLen);
            byte[] salt = new byte[saltLen];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(salt);
            }

            salt[0] = (byte)((salt[0] & 0xfc) | (saltLen & 0x3));
            salt[1] = (byte)((salt[1] & 0xf3) | (saltLen & 0xc));
            salt[2] = (byte)((salt[2] & 0xcf) | (saltLen & 0x30));
            salt[3] = (byte)((salt[3] & 0x3f) | (saltLen & 0xc0));

            return salt;
        }

        private int GenerateRandomNumber(int minValue, int maxValue)
        {
            byte[] randomBytes = new byte[4];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            int seed = ((randomBytes[0] & 0x7f) << 24) | (randomBytes[1] << 16) | (randomBytes[2] << 8) | (randomBytes[3]);
            Random random = new Random(seed);
            return random.Next(minValue, maxValue + 1);
        }
    }
}
