using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace BeltExam2.Models
{
    public class DojoAct
    {
        [Key]
        public int DojoActId {get;set;}
        [Required]
        [MinLength(2,ErrorMessage="Title must be at least 2 characters.")]
        public string Title {get;set;}

        [Required(ErrorMessage="Please enter date.")]
        [Date(ErrorMessage="Date must be in the near future.")]
        public DateTime StartDateTime {get;set;}

        [Required(ErrorMessage="Please enter duration")]
        [Range(0,double.PositiveInfinity,ErrorMessage="Must be positive number.")]
        public int Duration {get;set;}

        [Required(ErrorMessage="Please enter a description.")]
        [MinLength(3,ErrorMessage="Must be at least 3 characters.")]
        public string Description {get;set;}


        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;

        // Foreign Key
        public int CreatorId {get;set;}

        // Navigation for one to many - one user can create many dojoacts
        public User Creator {get;set;}

        // Navigation for many to many
        public List<UserDojoAct> AllUsers {get;set;}


        [NotMapped]
        public string DurationString
        {
            get
            {
                int days = this.Duration / (60*24);
                int hours = this.Duration / 60;
                int minutes = this.Duration;

                if (days != 0)
                {
                    return $"{days} days";
                }
                else if (hours != 0)
                {
                    return $"{hours} hours";
                }
                else
                {
                    return $"{minutes} minutes";
                }
            }
        }

        [NotMapped]
        public string DateTimeString
        {
            get
            {
                return this.StartDateTime.ToString("MMMM dd, yyyy @ hh:mm tt");
            }
        }

    }
    public class DateAttribute : RangeAttribute
    {
        public DateAttribute()
        : base(typeof(DateTime), 
            DateTime.Now.ToString("MM/dd/yyyy"),     
            DateTime.Now.AddYears(100).ToString("MM/dd/yyyy")) 
        { } 
    }
}
