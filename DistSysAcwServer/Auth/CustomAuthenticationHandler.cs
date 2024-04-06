using System;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DistSysAcwServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace DistSysAcwServer.Auth
{
    /// <summary>
    /// Authenticates clients by API Key
    /// </summary>
    public class CustomAuthenticationHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>, IAuthenticationHandler
    {
        private Models.UserContext DbContext { get; set; }
        private IHttpContextAccessor HttpContextAccessor { get; set; }

        private readonly UserDbAccess _userDbAccess;

        public CustomAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, Models.UserContext dbContext, IHttpContextAccessor httpContextAccessor, UserDbAccess userDBaccess ) : base(options, logger, encoder, clock) 
        {
            _userDbAccess = userDBaccess;
            DbContext = dbContext;
            HttpContextAccessor = httpContextAccessor;
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            #region Task5
            // TODO:  Find if a header ‘ApiKey’ exists, and if it does, check the database to determine if the given API Key is valid
            //        Then create the correct Claims, add these to a ClaimsIdentity, create a ClaimsPrincipal from the identity 
            //        Then use the Principal to generate a new AuthenticationTicket to return a Success AuthenticateResult
            #endregion

            string apiKey;
            string authenticationType = "ApiKey";

            //check if the request has the matching apikey as a header
            if (!Request.Headers.TryGetValue("ApiKey", out var apiKeyHeader))
            {
                return AuthenticateResult.Fail("Not Authenticated");
            }

            apiKey = apiKeyHeader;

            //if a user based off the given apikey does not exist
            if (!_userDbAccess.ApiKeyUserExists(apiKey))
            {
                return  AuthenticateResult.Fail("Unauthorized. Check ApiKey in Header is correct.");
            }

            User verifiedUser = _userDbAccess.ApiKeyUserExistsUser(apiKey);

            Claim[] claimsArray = new Claim[]
            {
                new Claim(ClaimTypes.Name, verifiedUser.UserName),
                new Claim(ClaimTypes.Role, verifiedUser.Role.ToString())
            };

            ClaimsIdentity identity = new ClaimsIdentity(claimsArray, authenticationType);

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            var authenticationTicket = new AuthenticationTicket(principal, this.Scheme.Name);

            return AuthenticateResult.Success(authenticationTicket);
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            byte[] messagebytes = Encoding.ASCII.GetBytes("Unauthorized. Check ApiKey in Header is correct.");
            Context.Response.StatusCode = 401;
            Context.Response.ContentType = "application/json";
            await Context.Response.Body.WriteAsync(messagebytes, 0, messagebytes.Length);
            await HttpContextAccessor.HttpContext.Response.CompleteAsync();
        }
    }
}