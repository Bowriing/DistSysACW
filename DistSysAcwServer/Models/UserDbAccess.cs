
using System.Linq;

namespace DistSysAcwServer.Models
{
    public class UserDbAccess
    {
        //THIS VARIABLE IS THE DATABASE IT JUST HAS A WEIRD NAME
        private readonly UserContext _userContext;

        public UserDbAccess(UserContext pContext)
        {
            _userContext = pContext;
        } 

        //US

        public void CreateUser(User user)
        {
            _userContext.Users.Add(user);
            _userContext.SaveChanges();
        }

        public bool ApiKeyUserExists(string pApiKey)
        {
            return _userContext.Users.Any(user => user.ApiKey == pApiKey);
        }
        
        public User ApiKeyUserExistsUser(string pApiKey)
        {
            return _userContext.Users.FirstOrDefault(user => user.ApiKey == pApiKey);
        }

        public bool AnyUserExists()
        {
            return _userContext.Users.Any();
        }

        public bool UserUsernameExists(string pUsername)
        {
            return _userContext.Users.Any(user => user.UserName == pUsername);
        }
    

        public bool ApiKeyUsernameExists(string pApikey, string pUsername)
        {
            return _userContext.Users.Any(user => user.ApiKey == pApikey && user.UserName == pUsername);
        }

        public bool DeleteUserApiKey(string pApikey)
        {
            User user = ApiKeyUserExistsUser(pApikey);
            if(user != null)
            {
                _userContext.Users.Remove(user);
                _userContext.SaveChanges();
                return true;
            }

            return false;
        }
    }
}