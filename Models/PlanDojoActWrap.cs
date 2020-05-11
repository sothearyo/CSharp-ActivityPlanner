using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace BeltExam2.Models
{
public class PlanDojoActWrap
    {
        public DojoAct DojoAct {get;set;}

        [Required(ErrorMessage="Please enter a time.")]
        public DateTime StartTime {get;set;}
        public string DurationType {get;set;}

    }
}