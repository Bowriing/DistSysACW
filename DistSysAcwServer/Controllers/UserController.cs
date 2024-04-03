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
        [Authorize(Roles = "Admin, User")]
        public IActionResult CreateUser([FromBody]string pUsername)
        {
            //check if any data for username given from body
            if (pUsername == null)
            {
                return BadRequest("Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json");
            }

            //check if the username exists in the database
            else if (_userDbAccess.UserUsernameExists(pUsername))
            {
                return Forbid("Oops. This username is already in use. Please try again with a new username.");
            }

            string username = pUsername;
            string ApiKey = Guid.NewGuid().ToString(); //randomly generated GUID String for API key
            DistSysAcwServer.Models.User.UserRole role = Models.User.UserRole.user;

            if (!_userDbAccess.AnyUserExists())
            {
                role = Models.User.UserRole.admin;
            }

            User user = new User
            {
                ApiKey = ApiKey,
                Role = role,
                UserName = username
            };

            _userDbAccess.CreateUser(user);

            return Ok(ApiKey);
        }

        //api/user/New?username=" "
        [HttpGet("New")]
        public IActionResult CreateUserGET(string username)
        {
            if(username == null)
            {
                return Ok("False - User Does Not Exist! Did you mean to do a POST to create a new user?");
            }

            bool exists = _userDbAccess.UserUsernameExists(username);

            if (exists)
            {
                return Ok("True! - User Does Exist! Did you mean to do a POST to create a new user?");
            }

            return Ok("False - User Does Not Exist! Did you mean to do a POST to create a new user?");

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
