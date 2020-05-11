using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace BeltExam2.Models
{
public class DashboardWrap
    {
        public User User {get;set;}
        public DojoAct DojoAct {get;set;}
        public List<DojoAct> AllDojoActs {get;set;}
        public string TimeConflicts {get;set;} = "";

    }
}