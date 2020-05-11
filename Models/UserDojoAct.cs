using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace BeltExam2.Models
{
    public class UserDojoAct
    {
        [Key]
        public int UserDojoActId {get;set;}
        public int UserId {get;set;}
        public int DojoActId {get;set;}

        // Navigation properties
        public User User {get;set;}
        public DojoAct DojoAct {get;set;}

        // Created and updated at
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;

    }
}