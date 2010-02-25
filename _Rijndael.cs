// YCC_encoder-decoder V 1.3.1 build 128
// (c) S. Manzhulovsky KIT-24B NTU "KPI" 2010
// create   24.11.2009
// modified 22.01.2010

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;


namespace YCC_encoder
{
     public class _Rijndael
    {

        private string mKey = string.Empty;
        private string mSalt = string.Empty;
        private SymmetricAlgorithm mCryptoService;


        public _Rijndael()
        {
            mCryptoService = new RijndaelManaged();
            mCryptoService.Mode = CipherMode.CBC;
        }

        public virtual byte[] GetLegalKey()
        {
            // Adjust key if necessary, and return a valid key

            if (mCryptoService.LegalKeySizes.Length > 0)
            {
                // Key sizes in bits

                int keySize = mKey.Length * 8;
                int minSize = mCryptoService.LegalKeySizes[0].MinSize;
                int maxSize = mCryptoService.LegalKeySizes[0].MaxSize;
                int skipSize = mCryptoService.LegalKeySizes[0].SkipSize;

                if (keySize > maxSize)
                {
                    // Extract maximum size allowed

                    mKey = mKey.Substring(0, maxSize / 8);
                }
                else if (keySize < maxSize)
                {
                    // Set valid size

                    int validSize = (keySize <= minSize) ? minSize :
                         (keySize - keySize % skipSize) + skipSize;
                    if (keySize < validSize)
                    {
                        
                        mKey = mKey.PadRight(validSize / 8, '*');
                    }
                }
            }
            PasswordDeriveBytes key = new PasswordDeriveBytes(mKey,
                 ASCIIEncoding.ASCII.GetBytes(mSalt));
            return key.GetBytes(mKey.Length);
        }

        public virtual byte[] Encrypt(byte[] plainByte)
        {
            byte[] keyByte = GetLegalKey();

            mCryptoService.Key = keyByte;
            mCryptoService.IV = new byte[] {0xe, 0x6f, 0x44, 0x2e, 0x87, 
                             0xc2, 0xff, 0xfd, 0x54, 0x24, 0x84, 0xea, 0xa8, 0x4b, 0x27,0xcc};

            ICryptoTransform cryptoTransform = mCryptoService.CreateEncryptor();

            MemoryStream ms = new MemoryStream();

            CryptoStream cs = new CryptoStream(ms, cryptoTransform,
                 CryptoStreamMode.Write);

            cs.Write(plainByte, 0, plainByte.Length);
            cs.FlushFinalBlock();

            byte[] cryptoByte = ms.ToArray();

            return cryptoByte;
        }

        public virtual byte[] Decrypt(byte[] cryptoByte)
        {
            byte[] keyByte = GetLegalKey();

            mCryptoService.Key = keyByte;
            mCryptoService.IV = new byte[] {0xe, 0x6f, 0x44, 0x2e, 0x87, 
                             0xc2, 0xff, 0xfd, 0x54, 0x24, 0x84, 0xea, 0xa8, 0x4b, 0x27,0xcc};

            ICryptoTransform cryptoTransform = mCryptoService.CreateDecryptor();
            try
            {
                MemoryStream ms = new MemoryStream(cryptoByte, 0, cryptoByte.Length);

                CryptoStream cs = new CryptoStream(ms, cryptoTransform,
                    CryptoStreamMode.Read);

                MemoryStream output = new MemoryStream();
                byte[] buff = new byte[128];
                int read = -1;
                read = cs.Read(buff, 0, buff.Length);
                while (read > 0)
                {
                    output.Write(buff, 0, read);
                    read = cs.Read(buff, 0, buff.Length);
                }
                cs.Close();

                return output.ToArray();
            }
            catch
            {
                return null;
            }
        }

        public string Key
        {
            get
            {
                return mKey;
            }
            set
            {
                mKey = value;
            }
        }
    }


}
