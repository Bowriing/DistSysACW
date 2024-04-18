using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DistSysAcwServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistSysAcwServer.Controllers
{
    [Route("api/[controller]")]
    public class ProtectedController : BaseController
    {

        readonly RSA rsa;
        readonly UserDbAccess _userDbAccess;
        protected EncryptionService _encryptionService;

        public ProtectedController(Models.UserContext dbcontext, UserDbAccess pUserDbAccess, RSA pRSA, EncryptionService pEncryptionService) : base(dbcontext)
        {
            _userDbAccess = pUserDbAccess;
            rsa = pRSA;
            _encryptionService = pEncryptionService;
        }

        //Task 9 - /Protected/Hello
        [HttpGet("Hello")]
        [Authorize(Roles ="admin,user")]
        public IActionResult Hello([FromHeader]string ApiKey)
        {
            User user = _userDbAccess.GetUserByApiKey(ApiKey);
            return Ok("Hello " + user.UserName);
        }

        [HttpGet("sha1")]
        [Authorize(Roles ="admin,user")]
        public IActionResult ConvertSHA1(string? message)
        {
            if (message == null)
            {
                return BadRequest("Bad Request");
            }

            string returnMessage;

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(message));
                returnMessage = BitConverter.ToString(hashBytes).Replace("-", "").ToString();
            }

            return Ok(returnMessage);
        }

        [HttpGet("sha256")]
        [Authorize(Roles = "admin,user")]
        public IActionResult ConvertSHA256(string? message)
        {
            if (message == null)
            {
                return BadRequest("Bad Request");
            }

            string returnMessage;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(message));
                returnMessage = BitConverter.ToString(hashBytes).Replace("-", "").ToString();
            }

            return Ok(returnMessage);
        }

        //Task11
        [HttpGet("getpublickey")]
        [Authorize(Roles = "admin,user")]
        public IActionResult GetPublicKey([FromHeader] string apiKey)
        {
            if (!_userDbAccess.ApiKeyUserExists(apiKey))
            {
                return BadRequest("ApiKey is not in database");
            }

            RSAParameters publicKeyParams = rsa.ExportParameters(false);
            string pubKeyXML = ToXmlString(publicKeyParams);
            return Ok(pubKeyXML);
        }

        private string ToXmlString(RSAParameters rsaParams)
        {
            StringBuilder xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<RSAKeyValue>");
            xmlBuilder.Append("<Modulus>").Append(Convert.ToBase64String(rsaParams.Modulus)).Append("</Modulus>");
            xmlBuilder.Append("<Exponent>").Append(Convert.ToBase64String(rsaParams.Exponent)).Append("</Exponent>");
            xmlBuilder.Append("</RSAKeyValue>");
            return xmlBuilder.ToString();
        }

        [HttpGet("Sign")]
        [Authorize(Roles = "admin,user")]
        public IActionResult Sign([FromHeader]string ApiKey, string message)
        {
            if (!_userDbAccess.ApiKeyUserExists(ApiKey))
            {
                return BadRequest("ApiKey is not in database.");
            }

            else if(message == null || message == " ")
            {
                return BadRequest("Message is null or empty");
            }

            //Convert to ascii
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            //add signiture using private key and sha1
            byte[] signedBytes = rsa.SignData(messageBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

            //convert to hex for return
            string hexBytes = BitConverter.ToString(signedBytes);

            return Ok(hexBytes);
        }

        //Task14
        [HttpGet("mashify")]
        [Authorize(Roles = "admin")]
        public IActionResult Mashify([FromHeader]string ApiKey, string encryptedString, string encryptedsymkey, string encryptedIV)
        {
            if (_userDbAccess.ApiKeyUserExists(ApiKey) == false)
            {
                return BadRequest();
            }

            //decrypt data
            string decryptedString = _encryptionService.DecryptRsa(encryptedString);
            byte[] aesKey = _encryptionService.DecryptRsaToBytes(encryptedsymkey);
            byte[] aesIV = _encryptionService.DecryptRsaToBytes(encryptedIV);

            string mashifiedString = MashifyString(decryptedString);
            string encryptedResponse = _encryptionService.EncryptAes(mashifiedString, aesKey, aesIV);

            User user = _userDbAccess.GetUserByApiKey(ApiKey);
            _userDbAccess.CreateLog(user.UserName + " requested" + HttpContext.Request.Path.ToString(), user);

            return Ok(encryptedResponse);
        }

        private string MashifyString(string input)
        {
            char[] vowels = new char[] { 'a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U' };
            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (Array.IndexOf(vowels, c) >= 0)
                    sb.Append('X');
                else
                    sb.Append(c);
            }
            char[] charArray = sb.ToString().ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

    }

    public class EncryptionService
    {
        private RSA _privateKey;

        public EncryptionService(RSA privateKey)
        {
            _privateKey = privateKey;
        }

        public string DecryptRsa(string input)
        {
            byte[] bytesToDecrypt = Convert.FromHexString(input.Replace("-", ""));
            byte[] decryptedBytes = _privateKey.Decrypt(bytesToDecrypt, RSAEncryptionPadding.OaepSHA1);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public byte[] DecryptRsaToBytes(string input)
        {
            byte[] bytesToDecrypt = Convert.FromHexString(input.Replace("-", ""));
            return _privateKey.Decrypt(bytesToDecrypt, RSAEncryptionPadding.OaepSHA1);
        }

        public string EncryptAes(string plaintext, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plaintext);
                    }
                    var encrypted = msEncrypt.ToArray();
                    return BitConverter.ToString(encrypted).Replace("-", "");
                }
            }
        }
    }
}
