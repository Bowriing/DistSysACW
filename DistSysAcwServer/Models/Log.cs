using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DistSysAcwServer.Models
{
    public class Log
    {
        [Key]
        public int logID { get; set; }
        public string logString { get; set; }
        public DateTime logDateTime { get; set; }

        //Foreign Key Stuff
        public string UserApiKey { get; set; }

        //Navigation Property
        [ForeignKey("UserApiKey")]
        public virtual User User { get; set; }

        #region Task2
        // TODO: Create a User Class for use with Entity Framework
        // Note that you can use the [key] attribute to set your ApiKey Guid as the primary key 
        #endregion
        public Log() { }

        public Log(string pLogString, DateTime pDT)
        {
            logString = pLogString;
            logDateTime = pDT;
        }
    }

    #region Task13?
    // TODO: You may find it useful to add code here for Logging
    #endregion


}