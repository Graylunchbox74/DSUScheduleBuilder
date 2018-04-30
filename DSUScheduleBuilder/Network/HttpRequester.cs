﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using DSUScheduleBuilder.Models;

namespace DSUScheduleBuilder.Network {
    static class ErrorPrinter
    {
        public static void BadData(string msg)
        {
            Console.WriteLine("[Bad Data] " + msg);
        }

        public static void Code(Errorable error)
        {
            Console.WriteLine("[Error " + error.errorCode + "] " + error.errorMessage);
        }
    }
    
    #region RESPONSE CLASSES
    class Errorable
    {
        public int? errorCode { get; set; }
        public string errorMessage { get; set; }
    }

    class UserResponse : Errorable
    {
        //public int uid { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string majors { get; set; }
        public string minors { get; set; }
        public string email { get; set; }

        public User ToUser()
        {
            return new User
            {
                //Uid = this.uid,
                FirstName = this.fname,
                LastName = this.lname,
                Majors = this.majors,
                Minors = this.minors,
                Email = this.email
            };
        }
    }

    class CourseResponse : Errorable
    {
        public int key { get; set; }
        public int startTime { get; set; }
        public int endTime { get; set; }
        public int credits { get; set; }
        public string classID { get; set; }
        public string className { get; set; }
        public List<string> teacher { get; set; }
        public string location { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string daysOfWeek { get; set; }

        public Course ToCourse()
        {
            return new Course()
            {
                Key = this.key,
                StartTime = this.startTime,
                EndTime = this.endTime,
                StartDate = this.startDate,
                EndDate = this.endDate,
                Credits = this.credits,
                CourseID = this.classID,
                CourseName = this.className,
                Teacher = this.teacher?[0],
                Location = this.location,
                DaysOfWeek = this.daysOfWeek
            };
        }
    }

    class FullCourseResponse : Errorable
    {
        public List<CourseResponse> classes { get; set; }

        public List<Course> ToCourses()
        {
            if (classes == null) return null;

            return classes.ConvertAll<Course>((course) => course.ToCourse());
        }
    }
    
    class LoginResponse : Errorable
    {
        public UserResponse user { get; set; }
        public string uuid { get; set; }
    }

    class SuccessResponse : Errorable
    {
        public int success { get; set; }
    }

    class EnrollResponse
    {
        public List<string> recommended { get; set; }
        public List<string> required { get; set; }
    }

    class FullEnrollResponse : Errorable
    {
        public List<EnrollResponse> classes { get; set; }
    }

    class AvailableCourseResponse
    {
        public string sectionID { get; set; }
        public bool open { get; set; }
        public string academicLevel { get; set; }
        public string courseID { get; set; }
        public string description { get; set; }
        public string courseName { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string location { get; set; }
        public string meetingInformation { get; set; }
        public string supplies { get; set; }
        public int credits { get; set; }
        public int slotsAvailable { get; set; }
        public int slotsCapacity { get; set; }
        public int slotsWaitlist { get; set; }
        public int timeStart { get; set; }
        public int timeEnd { get; set; }
        public string professorEmails { get; set; }
        public List<string> teacher { get; set; }
        public string prereqNonCourse { get; set; }
        public string recConcurrentCourses { get; set; }
        public string reqConcurrentCourses { get; set; }
        public string prereqCoursesAnd { get; set; }
        public string prereqCoursesOr { get; set; }
        public string instructionalMethods { get; set; }
        public string daysOfWeek { get; set; }
        public string term { get; set; }
        public int key { get; set; }

        public AvailableCourse ToAvailableCourse()
        {
            return new AvailableCourse() {
                SectionID = sectionID,
                Open = open,
                AcademicLevel = academicLevel,
                CourseID = courseID,
                Description = description,
                CourseName = courseName,
                StartDate = startDate,
                EndDate = endDate,
                Location = location,
                MeetingInformation = meetingInformation,
                Supplies = supplies,
                Credits = credits,
                SlotsAvailable = slotsAvailable,
                SlotsCapacity = slotsCapacity,
                SlotsWaitlist = slotsWaitlist,
                StartTime = timeStart,
                EndTime = timeEnd,
                ProfessorEmails = professorEmails,
                Teacher = teacher?[0],
                PrereqNonCourse = prereqNonCourse,
                RecConcurrentCourses = recConcurrentCourses,
                ReqConcurrentCourses = reqConcurrentCourses,
                PrereqCoursesAnd = prereqCoursesAnd,
                PrereqCoursesOr = prereqCoursesOr,
                InstructionalMethods = instructionalMethods,
                DaysOfWeek = daysOfWeek,
                Term = term,
                Key = key
            };
        }
    }

    class FullAvailableCourseResponse : Errorable
    {
        public List<AvailableCourseResponse> classes { get; set; }

        public List<AvailableCourse> ToCourses()
        {
            return classes?.ConvertAll<AvailableCourse>((AvailableCourseResponse acr) => acr.ToAvailableCourse());
        }
    }
    
    class PreviousCourseResponse
    {
        public string courseID { get; set; }
        public string courseName { get; set; }
        public int credits { get; set; }
    }

    class FullPreviousCourseResponse : Errorable
    {
        public List<PreviousCourseResponse> classes { get; set; }

        public List<PreviousCourse> ToCourses()
        {
            return classes?.ConvertAll<PreviousCourse>((PreviousCourseResponse pcr) => new PreviousCourse()
            {
                CourseID = pcr.courseID,
                CourseName = pcr.courseName,
                Credits = pcr.credits,
            });
        }
    }

    #endregion

    class HttpRequester
    {
        //STATIC FIELD
        //Used to keep the internal code cleaner because we don't have to pass
        //around a HttpRequester instance. Also, we will never need more than one HttpRequester.
        private static HttpRequester _default;
        public static HttpRequester Default
        {
            get
            {
                return _default;
            }
        }

        //HttpRCB is short for HttpRequestCallback
        public delegate bool HttpRCB<T>(T t);

        //CLASS FIELDS
        private RestClient _client;
        private string _session_token;

        public HttpRequester(string target)
        {
            if (_default == null)
                _default = this;

            _session_token = "";

            _client = new RestClient(target);
        }

        #region REQUESTS
        public void Login(string email, string password, HttpRCB<LoginResponse> callback)
        {
            Console.WriteLine("LOGGING IN");
            var postRequest = new RestRequest(Method.POST)
            {
                Resource = "api/user/login"
            };

            postRequest.AddParameter("email", email);
            postRequest.AddParameter("password", password);

            var response = _client.Execute<LoginResponse>(postRequest);

            if (response.Data == null)
            {
                ErrorPrinter.BadData("Failed to login");
                return;
            }

            bool worked = callback(response.Data);

            if (worked)
            {
                Console.Write("Setting session token: ");

                _session_token = response.Data.uuid;

                Console.WriteLine(_session_token);
            }
        }

        public void Logout()
        {
            if (_session_token == "") return;
            Console.WriteLine("LOGGING OUT");
            var postRequest = new RestRequest(Method.POST)
            {
                Resource = "api/user/logout"
            };

            postRequest.AddParameter("uuid", _session_token);
            var response = _client.Execute<SuccessResponse>(postRequest);
            if (response.Data?.success == 1)
            {
                _session_token = "";
            }
        }

        public void NewUser(string email, string password, string first, string last, HttpRCB<SuccessResponse> callback)
        {
            Console.WriteLine("CREATING NEW USER");
            var req = new RestRequest(Method.POST)
            {
                Resource = "api/user/newUser"
            };

            req.AddParameter("email", email);
            req.AddParameter("password", password);
            req.AddParameter("firstName", first);
            req.AddParameter("lastName", last);

            var res = _client.Execute<SuccessResponse>(req);
            if (res.Data == null)
            {
                ErrorPrinter.BadData("Failed to add new user");
                return;
            }

            callback(res.Data);
        }

        public void DeleteUser(Func<SuccessResponse, bool> callback)
        {
            var req = new RestRequest(Method.POST)
            {
                Resource = "api/user/delete"
            };
            req.AddParameter("uuid", _session_token);
            var res = _client.Execute<SuccessResponse>(req);
            SuccessResponse succ = res.Data;

            if (succ == null)
            {
                ErrorPrinter.BadData("Deleteing user failed");
                return;
            }

            if (callback(succ))
            {
                _session_token = "";
            }
        }

        public void ChangePassword(string oldPass, string newPass, HttpRCB<SuccessResponse> callback)
        {
            var req = new RestRequest(Method.POST)
            {
                Resource = "api/user/changePassword"
            };
            req.AddParameter("uuid", _session_token);
            req.AddParameter("currentPassword", oldPass);
            req.AddParameter("newPassword", newPass);
            var res = _client.Execute<SuccessResponse>(req);
            SuccessResponse succ = res.Data;

            if (succ == null)
            {
                ErrorPrinter.BadData("Parsing change password failed");
                return;
            }

            callback(succ);
        }

        public User GetUser()
        {
            Console.WriteLine("LOADING USER");
            var getRequest = new RestRequest(Method.GET)
            {
                Resource = "api/user/getData/" + _session_token
            };
            var response = _client.Execute<UserResponse>(getRequest);
            UserResponse user = response.Data;

            if (user == null)
            {
                ErrorPrinter.BadData("Parsing User failed");
                return null;
            }

            if (user.errorCode != null)
            {
                ErrorPrinter.Code(user);
                //Handle the different error codes for a user here
                return null;
            }

            return user.ToUser();
        }
        
        public List<PreviousCourse> GetPreviousCourses()
        {
            var getRequest = new RestRequest(Method.GET)
            {
                Resource = "api/courses/previous/" + _session_token
            };
            var response = _client.Execute<FullPreviousCourseResponse>(getRequest);
            FullPreviousCourseResponse courses = response.Data;

            if (courses == null)
            {
                ErrorPrinter.BadData("Parsing Previous Courses failed");
                return null;
            }

            if (courses.errorCode != null)
            {
                ErrorPrinter.Code(courses);
                return null;
            }

            return courses?.ToCourses();
        }

        public List<Course> GetEnrolledCourses()
        {
            if (_session_token == "") return null;

            var getRequest = new RestRequest(Method.GET)
            {
                Resource = "api/courses/enrolled/" + _session_token
            };
            var response = _client.Execute<FullCourseResponse>(getRequest);
            FullCourseResponse courses = response.Data;

            if (courses == null)
            {
                ErrorPrinter.BadData("Parsing Enrolled Courses failed");
                return null;
            }

            if (courses.errorCode != null)
            {
                ErrorPrinter.Code(courses);
                //Properly handle errors
                return null;
            }

            return courses.ToCourses();
        }

        public List<AvailableCourse> GetAvailableCourses()
        {
            if (_session_token == "") return null;

            var req = new RestRequest(Method.GET)
            {
                Resource = "api/courses/available/" + _session_token
            };
            var res = _client.Execute<FullAvailableCourseResponse>(req);
            FullAvailableCourseResponse courses = res.Data;

            if (courses == null)
            {
                ErrorPrinter.BadData("Parsing Available courses failed");
                return null;
            }

            if (courses.errorCode != null)
            {
                ErrorPrinter.Code(courses);
                return null;
            }

            return courses.ToCourses();
        }

        public List<AvailableCourse> SearchForCourses(string term, string prefix, string number, string ilastname, int startTime, int endTime, int slots, HttpRCB<FullAvailableCourseResponse> callback)
        {
            if (_session_token == "") return null;

            var req = new RestRequest(Method.GET)
            {
                Resource = "api/courses/search/" + _session_token
            };

            if (term != "") req.AddParameter("term", term);
            if (prefix != "") req.AddParameter("prefix", prefix);
            if (number != "") req.AddParameter("number", number);
            if (ilastname != "") req.AddParameter("instructor", ilastname);
            if (startTime != -1 && endTime != -1) {
                if (startTime < endTime) {
                    req.AddParameter("startTime", startTime);
                    req.AddParameter("endTime", endTime);
                }
            }
            if (slots >= 0) req.AddParameter("slotsAvailable", slots);

            var res = _client.Execute<FullAvailableCourseResponse>(req);
            FullAvailableCourseResponse courses = res.Data;

            if (courses == null)
            {
                ErrorPrinter.BadData("Parsing searched courses failed");
                return null;
            }

            if (callback(courses))
            {
                return courses?.ToCourses();
            }
            else
            {
                return null;
            }
        }

        public void EnrollInCourse(int courseKey, HttpRCB<FullEnrollResponse> callback)
        {
            if (_session_token == "") return;

            var req = new RestRequest(Method.POST)
            {
                Resource = "api/user/enroll/"
            };
            req.AddParameter("uuid", _session_token);
            req.AddParameter("key", courseKey);
            var res = _client.Execute<FullEnrollResponse>(req);
            FullEnrollResponse succ = res.Data;

            if (succ == null)
            {
                ErrorPrinter.BadData("Parsing enrolled class failed");
                return;
            }

            callback(succ);
        }

        public void DropCourse(int courseKey, HttpRCB<SuccessResponse> callback)
        {
            if (_session_token == "") return;

            var req = new RestRequest(Method.POST)
            {
                Resource = "api/user/dropEnrolledCourse"
            };
            req.AddParameter("uuid", _session_token);
            req.AddParameter("courseID", courseKey);

            var res = _client.Execute<SuccessResponse>(req);
            SuccessResponse succ = res.Data;

            if (succ == null)
            {
                ErrorPrinter.BadData("Parsing drop course failed");
                return;
            }

            callback(succ);
        }

        public void AddPreviousCourse(string courseID, string courseName, int credits, HttpRCB<SuccessResponse> callback)
        {
            if (_session_token == "") return;

            var req = new RestRequest(Method.POST)
            {
                Resource = "api/user/addPrevious"
            };
            req.AddParameter("uuid", _session_token);
            req.AddParameter("courseID", courseID);
            req.AddParameter("courseName", courseName);
            req.AddParameter("credits", credits);
            var res = _client.Execute<SuccessResponse>(req);
            SuccessResponse succ = res.Data;

            if (succ == null)
            {
                ErrorPrinter.BadData("Parsing add previous class failed");
                return;
            }

            callback(succ);
        }

        public void DeletePreviousCourse(string courseID, HttpRCB<SuccessResponse> callback)
        {
            if (_session_token == "") return;

            var req = new RestRequest(Method.POST)
            {
                Resource = "api/user/deletePrevious"
            };
            req.AddParameter("uuid", _session_token);
            req.AddParameter("courseID", courseID);
            var res = _client.Execute<SuccessResponse>(req);
            SuccessResponse succ = res.Data;

            if (succ == null)
            {
                ErrorPrinter.BadData("Parsing delete previous class failed");
                return;
            }

            callback(succ);
        }

        #endregion
    }
}