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
        readonly UserDbAccess _userDbAccess;
        public ProtectedController(Models.UserContext dbcontext, UserDbAccess pUserDbAccess) : base(dbcontext)
        {
            _userDbAccess = pUserDbAccess;
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
    }
}
