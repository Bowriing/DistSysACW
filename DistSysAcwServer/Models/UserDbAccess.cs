
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
        
        public User GetUserByApiKey(string pApiKey)
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
        
        public User GetUserByUsername(string username)
        {
            return _userContext.Users.First(user => user.UserName == username);
        }

        public bool ApiKeyUsernameExists(string pApikey, string pUsername)
        {
            return _userContext.Users.Any(user => user.ApiKey == pApikey && user.UserName == pUsername);
        }

        public bool DeleteUser(string pApikey, string pUsername)
        {
            //create user object using method
            User user = GetUserByApiKey(pApikey);

            if(user == null)
            {
                return false;
            }

            //check parameter username exists/ is valid
            bool matchUser = UserUsernameExists(pUsername);

            //if no user is found
            if (!matchUser)
            {
                return false;
            }

            //if theyre not equal
            if (user.UserName != pUsername)
            {
                return false;
            }

            //remove user object from table
            _userContext.Users.Remove(user);
            _userContext.SaveChanges();

            return true;
        }

        public void ChangeUserRole(string username, string role)
        {
            //create object of user based off passed username
            User currentUser = GetUserByUsername(username);

            //parse role into enum to be able to update field in table
            var userRole = Enum.Parse<User.UserRole>(role);

            //set role to current user selected
            currentUser.Role = userRole;

            //update field and save
            _userContext.Users.Update(currentUser);
            _userContext.SaveChanges();
        }

        public void CreateLog(string pLogString)
        {
            DateTime dt = DateTime.Now;
            Log log = new Log(pLogString, dt);

            _userContext.Logs.Add(log);
            _userContext.SaveChanges();
        }
    }
}