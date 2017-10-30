using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ReverseTCPClient
{
    class TransmissionEncryptor
    {
        private RSACryptoServiceProvider asymmetricForeign = new RSACryptoServiceProvider();
        private RSACryptoServiceProvider asymmetricLocal = new RSACryptoServiceProvider();
        public string localPublicString;
        

        public TransmissionEncryptor()
        {
            localPublicString = asymmetricLocal.ToXmlString(false);
        }
        
        /// <summary>
        /// Set foreign public key to be used for encryption later.
        /// </summary>
        /// <param name="xmlString">Foreign public key in the form of an XML string.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool setForeignKey(string xmlString)
        {
            try
            {
                asymmetricForeign.FromXmlString(xmlString);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Encrypts a byte[] object using the foreign party's public key.
        /// </summary>
        /// <param name="plainText">Byte[] object to be encrypted.</param>
        /// <returns>Encrypted byte[] object.</returns>
        public byte[] encryptAsymmetricForeign(byte[] plainText)
        {
            byte[] cipherText = asymmetricForeign.Encrypt(plainText, true);
            return cipherText;
        }

        /// <summary>
        /// Decrypts a byte[] object using the local private key.
        /// </summary>
        /// <param name="cipherText">Byte[] object encrypted with local public key.</param>
        /// <returns>Unencrypted byte[] object.</returns>
        public byte[] decryptAsymmetricLocal(byte[] cipherText)
        {
            byte[] plainText = asymmetricLocal.Decrypt(cipherText, true);
            return plainText;
        }

        /// <summary>
        /// Creates a signiture for the data it is given.
        /// </summary>
        /// <param name="dataToSign">Data that is to be signed.</param>
        /// <returns>The signiture for the given data.</returns>
        public byte[] sign(byte[] dataToSign)
        {
            byte[] signiture = asymmetricLocal.SignData(dataToSign, new SHA256CryptoServiceProvider());
            return signiture;
        }

        /// <summary>
        /// Checks that the signiture received with the data is valid.
        /// </summary>
        /// <param name="data">Data that has been received.</param>
        /// <param name="signiture">Signiture that has been received.</param>
        /// <returns>Whether the signiture is valid or not.</returns>
        public bool validateSigniture(byte[] data, byte[] signiture)
        {
            return asymmetricForeign.VerifyData(data, new SHA256CryptoServiceProvider(), signiture);
        }

        /// <summary>
        /// Appends a given signiture onto the data it is to validate.
        /// </summary>
        /// <param name="data">Data to be validated by signiture.</param>
        /// <param name="signiture">Signiture to validate data.</param>
        /// <returns>Signed data.</returns>
        public byte[] appendSigniture(byte[] data, byte[] signiture)
        {
            byte[] signedData = new byte[data.Length + signiture.Length];
            data.CopyTo(signedData, 0);
            signiture.CopyTo(signedData, data.Length - 1);
            return signedData;
        }

        /// <summary>
        /// Seperates the data in a byte[] object from the signiture sent with it.
        /// </summary>
        /// <param name="signedData">Signed data.</param>
        /// <returns>A tuple with seperate variables for the data and the signiture.</returns>
        public Tuple<byte[], byte[]> seperateSigniture(byte[] signedData)
        {
            byte[] data = new byte[signedData.Length - 128];
            byte[] signiture = new byte[128];
            Array.Copy(signedData, data, signedData.Length - 129);
            Array.Copy(signedData, signedData.Length - 129, signiture, 0, 128);
            return new Tuple<byte[], byte[]>(data, signiture);
        }
    }
}
