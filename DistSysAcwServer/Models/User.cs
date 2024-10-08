﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DistSysAcwServer.Models
{
    /// <summary>
    /// User data class
    /// </summary>
    public class User
    {
        #region Task2
        // TODO: Create a User Class for use with Entity Framework
        // Note that you can use the [key] attribute to set your ApiKey Guid as the primary key 
        #endregion
        public User()
        {
            logs = new HashSet<Log>();
        }


        public enum UserRole {admin = 0,user = 1} // number corresponds to the role type

        [Key]
        public string ApiKey { get; set; }

        public string UserName { get; set; }
        public UserRole Role { get; set; }

        public ICollection<Log> logs { get; set; }
    }

    #region Task13?
    // TODO: You may find it useful to add code here for Logging
    #endregion


}