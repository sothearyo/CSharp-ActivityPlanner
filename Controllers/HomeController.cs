using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using BeltExam2.Models;

namespace BeltExam2.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        // ------------------- Login and Registration -----------------------------
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email","Email already in use.");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                dbContext.Add(newUser);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("UserId",newUser.UserId);

                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.UserEmail);
                if (userInDb == null)
                {
                    ModelState.AddModelError("UserEmail", "Invalid Email/Password.");
                    return View("Index");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.UserPassword);
                if(result == 0)
                {
                    ModelState.AddModelError("UserEmail", "Incorrect Email/Password.");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("UserId",userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // -------------------- Belt Exam 2 --------------------------------------

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            if (user_id == 0)
            {
                return View("Index");
            }
            DashboardWrap newView = new DashboardWrap();
            newView.User = dbContext.Users
                .Include(u => u.AllDojoActs)
                .ThenInclude(ua => ua.DojoAct)
                .FirstOrDefault(u => u.UserId == user_id);
            // newView.AllDojoActs = dbContext.DojoActs
            //     .Include(d => d.AllUsers)
            //     .Include(d => d.Creator)
            //     .OrderBy(d => d.StartDateTime)
            //     .ToList();

            // Only display current activities
            List<DojoAct> AllPastPresent = dbContext.DojoActs
                .Include(d => d.AllUsers)
                .Include(d => d.Creator)
                .OrderBy(d => d.StartDateTime)
                .ToList();
            List<DojoAct> OnlyCurrentActs = new List<DojoAct>();
            foreach (DojoAct i in AllPastPresent)
            {
                int act_date = i.StartDateTime.Year*1000 + i.StartDateTime.DayOfYear;
                int today_date = DateTime.Now.Year*1000 + DateTime.Now.DayOfYear;
                if(act_date > today_date)
                {
                    OnlyCurrentActs.Add(i);
                }
            }
            newView.AllDojoActs = OnlyCurrentActs;

            return View(newView);
        }

        [HttpGet("planDojoAct")]
        public IActionResult PlanDojoAct()
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            if (user_id == 0)
            {
                return View("Index");
            }
            return View();
        }

        [HttpGet("oneDojoAct/{dojoact_id}")]
        public IActionResult OneDojoAct(int dojoact_id)
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            if (user_id == 0)
            {
                return View("Index");
            }
            DashboardWrap newView = new DashboardWrap();
            newView.DojoAct = dbContext.DojoActs
                .Include(d => d.Creator)
                .Include(d => d.AllUsers)
                .ThenInclude(ud => ud.User)
                .FirstOrDefault(d => d.DojoActId == dojoact_id);
            newView.User = dbContext.Users
                .Include(u => u.AllDojoActs)
                .ThenInclude(ua => ua.DojoAct)
                .FirstOrDefault(u => u.UserId == user_id);
            return View(newView);    

        }

        // ------------------ Join, Leave, and Delete Activity ----------
        [HttpGet("join/{dojoact_id}")]
        public IActionResult Join(int dojoact_id)
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            if (user_id == 0)
            {
                return View("Index");
            }
            UserDojoAct newAssoc = new UserDojoAct();
            newAssoc.UserId = user_id;
            newAssoc.DojoActId = dojoact_id;

            // Check for time conflicts
            List<UserDojoAct> myAlreadyJoined = dbContext.UserDojoActs
                .Include(ua => ua.DojoAct)
                .Include(ua => ua.User)
                .Where(ua => ua.UserId == user_id)
                .ToList();
            DojoAct thisDojoAct = dbContext.DojoActs
                .FirstOrDefault(d => d.DojoActId == dojoact_id);    
            foreach(UserDojoAct j in myAlreadyJoined)  
            {
                DateTime jActEndTime = j.DojoAct.StartDateTime.AddMinutes(j.DojoAct.Duration);
                DateTime thisActStartTime = thisDojoAct.StartDateTime;
                if(jActEndTime > thisActStartTime)
                {
                    DashboardWrap newView = new DashboardWrap();
                    newView.User = dbContext.Users
                        .Include(u => u.AllDojoActs)
                        .ThenInclude(ua => ua.DojoAct)
                        .FirstOrDefault(u => u.UserId == user_id);
                    // Only display current activities
                    List<DojoAct> AllPastPresent = dbContext.DojoActs
                        .Include(d => d.AllUsers)
                        .Include(d => d.Creator)
                        .OrderBy(d => d.StartDateTime)
                        .ToList();
                    List<DojoAct> OnlyCurrentActs = new List<DojoAct>();
                    foreach (DojoAct i in AllPastPresent)
                    {
                        int act_date = i.StartDateTime.Year*1000 + i.StartDateTime.DayOfYear;
                        int today_date = DateTime.Now.Year*1000 + DateTime.Now.DayOfYear;
                        if(act_date > today_date)
                        {
                            OnlyCurrentActs.Add(i);
                        }
                    }
                    newView.AllDojoActs = OnlyCurrentActs;
                    newView.TimeConflicts = "Cannot join activity due to time conflict with another one of your activities.";
                    System.Console.WriteLine("**********************************************************");
                    System.Console.WriteLine(j.DojoAct.StartDateTime.AddMinutes(j.DojoAct.Duration).Ticks);
                    System.Console.WriteLine(thisDojoAct.StartDateTime.Ticks);
                    return View("Dashboard", newView);
                }
            }          
            dbContext.Add(newAssoc);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("leave/{dojoact_id}")]
        public IActionResult Leave(int dojoact_id)
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            if (user_id == 0)
            {
                return View("Index");
            }
            UserDojoAct thisAssoc = dbContext.UserDojoActs
                .FirstOrDefault(a => a.UserId == user_id && a.DojoActId == dojoact_id);
            dbContext.Remove(thisAssoc);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("delete/{dojoact_id}")]
        public IActionResult DeleteDojoAct(int dojoact_id)
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            if (user_id == 0)
            {
                return View("Index");
            }
            DojoAct thisDojoAct = dbContext.DojoActs
                .FirstOrDefault(w => w.DojoActId == dojoact_id);
            dbContext.Remove(thisDojoAct);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }


        // ------------------  Create an Activity ---------------------
        [HttpPost("createDojoAct")]
        public IActionResult CreateDojoAct(PlanDojoActWrap fromForm)
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            if (user_id == 0)
            {
                return View("Index");
            }
            if(ModelState.IsValid)
            {
                if(fromForm.DurationType == "Hours")
                {
                    fromForm.DojoAct.Duration *= 60;
                }
                if(fromForm.DurationType == "Days")
                {
                    fromForm.DojoAct.Duration *= 60*24;
                }
                fromForm.DojoAct.CreatorId = user_id;
                fromForm.DojoAct.StartDateTime += fromForm.StartTime.TimeOfDay;
                dbContext.Add(fromForm.DojoAct);
                dbContext.SaveChanges();
                return RedirectToAction("OneDojoAct", new { dojoact_id = fromForm.DojoAct.DojoActId});
            }
            return View("PlanDojoAct");


        }

        // ------------------- Privacy and ErrorView Model -----------------------

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
