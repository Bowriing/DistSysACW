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
        public ProtectedController(Models.UserContext dbcontext, UserDbAccess pUserDbAccess, RSA pRSA) : base(dbcontext)
        {
            _userDbAccess = pUserDbAccess;
            rsa = pRSA;
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
        public IActionResult Sign([FromHeader]string apiKey, string message)
        {
            if (!_userDbAccess.ApiKeyUserExists(apiKey))
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

    }
}
