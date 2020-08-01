using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Services;

namespace SIMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //to enable cors
    [EnableCors("CorsPolicy")]
    public class StudentController : ControllerBase
    {

        private readonly SIMSContext _SIMSContext;
        private readonly IEmailService _emailSender;


        public StudentController(SIMSContext sIMSContext, IEmailService emailSender)
        {
            _SIMSContext = sIMSContext;
            _emailSender = emailSender;
        }


        // GET api/student/testServer
        [HttpGet("testServer")]
        public string testServer()
        {
            return  "Server is Working ";
        }
        
        // GET Student List
        [HttpGet("GetStudentList")]
        //for jwt authentication
        [Authorize]
        public IActionResult GetStudentList()
        {
            var studentList = _SIMSContext.Students.ToList();
            return Ok(studentList);
        }

        // GET particular Student 
        [HttpGet("GetStudent")]
        //for jwt authentication
        [Authorize]
        public IActionResult GetStudent(int StudentId)
        {
            var studentList = _SIMSContext.Students.Where(id => id.StudentId == StudentId ).FirstOrDefault();
            if (studentList == null)
            {
                return Ok(new { status = 200, message = "Student Not Found" });

            }
            return Ok(studentList);
        }

        // Add new Student 
        [HttpPost("RegisterStudent")]
        //for jwt authentication
        [Authorize]
        public IActionResult AddNewStudentList([FromBody]Student student)
        {

            _SIMSContext.Students.Add(student);
            try
            {
                
                int otp = _emailSender.SendOtp(student.Email, "Email Verification", "Your Otp is ");

                if(otp == 20202) 
                {
                    _SIMSContext.SaveChanges();
                    return Ok(new { status = 200, message = "Student Added Successfully" });
                }
                return Ok(new { status = 201, message = "Otp verification failed" });
            }
            catch(Exception Exception)
            {
                return Ok(new { status = 201, message = "Student Added Failed",errorMsg= Exception.InnerException.Message });
            }
        }

        // Delete Student 
        [HttpDelete("DeleteStudent")]
        //for jwt authentication
        [Authorize]
        public IActionResult DeleteStudent(int StudentId)
        {
            var student = _SIMSContext.Students.Where(id => id.StudentId == StudentId).FirstOrDefault();
            if(student == null)
            {
                return Ok(new { status = 200, message = "Student Not Found" });

            }
            else
            {
                try
                {
                    _SIMSContext.Remove(student);
                    _SIMSContext.SaveChanges();
                    return Ok(new { status = 200, message = "Student Deleted Successfully" });
                }
                catch (Exception Exception)
                {
                    return Ok(new { message = "Student Deleted Failed", errorMsg = Exception.InnerException.Message });
                }
            }
            
        }


        // Update Student 
        [HttpPut("UpdateStudent")]
        //for jwt authentication
        [Authorize]
        public IActionResult UpdateStudent(int StudentId,[FromBody]Student NewData)
        {
            var student = _SIMSContext.Students.Where(id => id.StudentId == StudentId).FirstOrDefault();
            if (student == null)
            {
                return Ok(new { status = 200, message = "Student Not Found" });

            }
            else
            {
                try
                {
                    student.FullName = NewData.FullName;
                    student.Contact = NewData.Contact;
                    student.Address = NewData.Address;
                    student.Email = NewData.Email;
                    student.Password = NewData.Password;
                    student.DOB = NewData.DOB;
                    
                    _SIMSContext.SaveChanges();
                    return Ok(new { status = 200, message = "Student Updated Successfully" });
                }
                catch (Exception Exception)
                {
                    return Ok(new { message = "Student update Failed", errorMsg = Exception.InnerException.Message });
                }
            }

        }


    }
}