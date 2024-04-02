using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistSysAcwServer.Controllers
{
    public class TalkbackController : BaseController
    {
        /// <summary>
        /// Constructs a TalkBack controller, taking the UserContext through dependency injection
        /// </summary>
        /// <param name="context">DbContext set as a service in Startup.cs and dependency injected</param>
        public TalkbackController(Models.UserContext dbcontext) : base(dbcontext) { }


        #region TASK1
        //    TODO: add api/talkback/hello response
        #endregion
        [HttpGet]
        public IActionResult Hello()
        {
            return Ok("Hello World");
        }

        #region TASK1
        //    TODO:
        //       add a parameter to get integers from the URI query
        //       sort the integers into ascending order
        //       send the integers back as the api/talkback/sort response
        //       conform to the error handling requirements in the spec
        #endregion
        [HttpGet]
        public IActionResult Sort([FromQuery] int[] integers)
        {
            if (integers.Any(item => !(item is int)))
            {
                return BadRequest();
            }

            var sortedIntegers = integers.OrderBy(i => i).ToArray();

            return Ok(sortedIntegers);
        }
    }
}
