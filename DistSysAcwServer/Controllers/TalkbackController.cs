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
        [HttpGet("Hello")]
        public ActionResult<string> HelloResponse()
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
        [HttpGet("Sort")]
        public ActionResult<int[]> SortFunction([FromQuery] int[] pNumbers)
        {
            List<int> numbersList = new();
            numbersList.AddRange(pNumbers);

            if (!numbersList.Any())
            {
                return BadRequest("No numbers have been provided");
            }

            numbersList.Sort();
            int[] returnNumbers = numbersList.ToArray();

            return Ok(returnNumbers);
        }
    }
}
