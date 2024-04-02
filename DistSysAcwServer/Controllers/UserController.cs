using System.Collections.Generic;
using DistSysAcwServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistSysAcwServer.Controllers
{
    //MUST DEFINE ROUTE ON EACH CONTROLLER!!!!!!
    [Route ("api/[controller]")]
    public class UserController : BaseController
    {
        //.              /api/usercontroller/
        readonly UserDbAccess _userDbAccess;

        public UserController(Models.UserContext dbcontext, UserDbAccess pUserDbAccess) : base(dbcontext) 
        {
            _userDbAccess = pUserDbAccess;
        }

        //  api/user/new
        [HttpPost("New")]
        public IActionResult CreateUser([FromBody]string pUsername)
        {
            if (pUsername == null)
            {
                return BadRequest("No Username Data");
            }

            string username = pUsername;
            string ApiKey = Guid.NewGuid().ToString(); //randomly generated GUID String for API key
            DistSysAcwServer.Models.User.UserRole role = Models.User.UserRole.admin;

            User user = new User
            {
                ApiKey = ApiKey,
                Role = role,
                UserName = username
            };

            _userDbAccess.CreateUser(user);

            return Ok(user);
        }

        [HttpGet("CheckApiKeyExist/{apiKey}")]
        public bool CheckApiKeyUserExist(string apiKey)
        {
            return _userDbAccess.ApiKeyUserExists(apiKey);
        }

        [HttpGet("CheckApiKeyExistUser/{apiKey}")]
        public User CheckApiKeyUserExistUserReturn(string apiKey)
        {
            return _userDbAccess.ApiKeyUserExistsUser(apiKey);
        }

        [HttpGet("CheckApiKeyUsernameExist/{apiKey}/{username}")]
        public bool CheckApiKeyUserExist(string apiKey, string username)
        {
            return _userDbAccess.ApiKeyUsernameExists(apiKey,username);
        }

        [HttpGet("DeleteUserByApiKey/{apiKey}")]
        public bool DeleteUserApiKey(string apiKey)
        {
            return _userDbAccess.DeleteUserApiKey(apiKey);
        }
    }
}
