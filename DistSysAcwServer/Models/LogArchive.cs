using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DistSysAcwServer.Models
{
    public class LogArchive
    {
        [Key]
        public int logID { get; set; }
        public string logString { get; set; }
        public DateTime logDateTime { get; set; }
        public string UserApiKey { get; set; }

        #region Task2
        // TODO: Create a User Class for use with Entity Framework
        // Note that you can use the [key] attribute to set your ApiKey Guid as the primary key 
        #endregion
        public LogArchive() { }

        public LogArchive(string pLogString)
        {
            logString = pLogString;
        }
    }

    #region Task13?
    // TODO: You may find it useful to add code here for Logging
    #endregion


}