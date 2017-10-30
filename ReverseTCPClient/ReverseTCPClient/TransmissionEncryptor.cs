using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;

namespace ReverseTCPClient
{
    class TransmissionEncryptor
    {
        /// <summary>
        /// Setting up variables for asymmetric encryption.
        /// </summary>
        private RSACryptoServiceProvider asymmetricForeign = new RSACryptoServiceProvider();
        private RSACryptoServiceProvider asymmetricLocal = new RSACryptoServiceProvider();
        public string localPublicString;

        /// <summary>
        /// Setting up variables for symmetric encryption.
        /// </summary>
        private Aes aes = Aes.Create();
        private List<byte[]> oldIVs = new List<byte[]>();

        public TransmissionEncryptor()
        {
            localPublicString = asymmetricLocal.ToXmlString(false);
            aes.GenerateKey();
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

        /// <summary>
        /// Sets the key to be used by AES for encryption.
        /// </summary>
        /// <param name="key">The Key for AES encryption</param>
        public void setAESKey(byte[] key)
        {
            aes.Key = key;
        }

        /// <summary>
        /// Encrypts a byte[] object using the set key and a IV that is appended to the start of the output.
        /// </summary>
        /// <param name="plainText">The byte[] object to be encrypted.</param>
        /// <returns>The IV that has been used appended by the cipher text.</returns>
        public byte[] encryptAES(byte[] plainText)
        {
            //Generating random IV key for one time use.
            generateUniqueIV();
            byte[] encryptedMessage;
            //Creating encryptor.
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memStream = new MemoryStream())
            {
                using(CryptoStream cryptStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptStream.Write(plainText, 0, plainText.Length);
                }
                encryptedMessage = memStream.ToArray();
            }

            //Adding IV to the start of the cipher text.
            byte[] datagram = new byte[aes.IV.Length+encryptedMessage.Length];
            Array.Copy(aes.IV, datagram, aes.IV.Length);
            Array.Copy(encryptedMessage, 0, datagram, aes.IV.Length, encryptedMessage.Length);

            return datagram;
        }

        /// <summary>
        /// Decrypts a cipher text using AES with the given key of the instance and the IV that is appended to the start of the datagram. 
        /// </summary>
        /// <param name="datagram">IV appended with the cipher text.</param>
        /// <returns>Byte[] object of plain text.</returns>
        public byte[] decryptAES(byte[] datagram)
        {
            byte[] plainText;
            
            //Seperating the IV from the cipher text.
            byte[] IV = new byte[aes.IV.Length];
            byte[] cipherText = new byte[datagram.Length - aes.IV.Length];
            Array.Copy(datagram, 0, IV, 0, aes.IV.Length);
            Array.Copy(datagram, aes.IV.Length, cipherText, 0, datagram.Length - aes.IV.Length);

            //Creating the decryptor.
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, IV);

            using(MemoryStream memStream = new MemoryStream())
            {
                using(CryptoStream cryptStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Write))
                {
                    cryptStream.Write(cipherText, 0, cipherText.Length);
                }
                plainText = memStream.ToArray();
            }

            return plainText;
        }

        /// <summary>
        /// Generates new IVs until one that has not been used is found.
        /// </summary>
        private void generateUniqueIV()
        {
            bool isNotUnique = true;

            while (isNotUnique)
            {
                aes.GenerateIV();
                if (!oldIVs.Contains(aes.IV))
                {
                    isNotUnique = false;
                }
                
            }
        }
    }
}
