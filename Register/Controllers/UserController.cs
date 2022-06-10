using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Register.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Text;
//using System.Net.Mail;
//using System.Net;
using MimeKit;
using MimeKit.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;

namespace Register.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]

    
    public class UserController : ControllerBase
    {
        //private readonly IConfiguration _config;
        //public readonly UserContext _context;

        private readonly IConfiguration _config;
        public UserController(IConfiguration config)
        {
            _config = config;
        }
        string connectionString = "Data Source=DESKTOP-I6UMNSF;Initial Catalog=LoginDB;Integrated Security=True";

        [AllowAnonymous]
        [HttpPost("CreateUser")]

        public IActionResult Create(User user)
        {
        SqlConnection con = new SqlConnection(connectionString);
            //Console.WriteLine(user.Email);
            //return Ok(user.FirstName+ user.LastName+ user.Email+ user.Password);
            string Q = "SELECT COUNT(*) FROM Users WHERE email='" + user.Email + "'";

            SqlDataAdapter sda = new SqlDataAdapter(Q, con);

            DataTable dt = new DataTable();
            sda.Fill(dt);
            //return Ok(dt.Rows[0][0].ToString());
            if (dt.Rows[0][0].ToString() != "0")
            {
                //return Ok(dt.Rows[0][0].ToString());
                return Ok("User Already Exists");
            }
            else
            {
                try
                {
                    con.Open();

                    string Query = "insert into Users(FirstName,LastName,Email,Password) values(@FirstName,@LastName,@Email,@Password)";


                    SqlCommand cmd = new SqlCommand(Query, con);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.ExecuteNonQuery();
                    con.Close();
                    return Ok("Success");
                }
                catch (Exception e)
                {

                    return Ok(e.ToString());
                }
            }

        }


        [AllowAnonymous]
        [HttpPost("LoginUser")]
        public IActionResult Login(Login user)
        {
            try
            {
                SqlConnection con = new SqlConnection(connectionString);
                con.Open();
                string Query = "SELECT COUNT(*) FROM Users WHERE email='"+user.Email+ "' AND password='"+user.Password+"'";
                
                SqlDataAdapter sda = new SqlDataAdapter(Query,con);
                string result = "";
                DataTable dt = new DataTable();
                sda.Fill(dt);
                if (dt.Rows[0][0].ToString() == "1")
                {
                    string returnQuery = "select * from users where email='" + user.Email + "'";
                    SqlCommand cmd = new SqlCommand(returnQuery, con);
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        //return Ok(oReader.Read());
                        while (oReader.Read())
                        {
                            result += " "+(oReader["FirstName"].ToString());
                            result += " " + (oReader["LastName"].ToString());
                            result += " " + (oReader["Email"].ToString());

                        }

                        con.Close();
                    }
                    var uDet = result.Split(' ');
                    return Ok(new JwtService(_config).GenerateToken(
                        uDet[1].ToString(),
                        uDet[2].ToString(),
                        uDet[3].ToString()
                        ));
                 }
                else
                    return Ok("Failure");

            }
            catch (Exception e)
            {

                return Ok(e);
            }

        }
        
        [HttpPost("forgot-password")]
        public IActionResult Forgot(Login user)
        {
            try
            {
                //return Ok(user.Email);
                SqlConnection con = new SqlConnection(connectionString);
                con.Open();

                string Q = "SELECT COUNT(*) FROM Users WHERE email='" + user.Email + "'";

                SqlDataAdapter sda = new SqlDataAdapter(Q, con);

                DataTable dt = new DataTable();
                sda.Fill(dt);
                //return Ok(dt.Rows[0][0].ToString());
                if (dt.Rows[0][0].ToString() != "0")
                {
                    //return Ok(dt.Rows[0][0].ToString());
                    return Ok("Found");
                }
                else
                {
                    return Ok("Not Found");
                }

                //string Query = "SELECT * FROM Users WHERE email='" + user.Email +"'";

                //SqlCommand cmd = new SqlCommand(Query, con);
                //using (SqlDataReader oReader = cmd.ExecuteReader())
                //{
                //    int count = 0;
                //    string str = "";
                //    while (oReader.Read())
                //    {
                //        count++;
                //    }
                //    return Ok(count);
                //}

            }
            catch (Exception)
            {

                return Ok("Not Found");
            }
        }
        
        [HttpPost("Reset")]
        public IActionResult Reset(Login user)
        {
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();

            string Q = "UPDATE Users SET Password = '"+user.Password+"' where email = '"+user.Email+"'";
            try
            {
                SqlCommand cmd = new SqlCommand(Q, con);
                cmd.ExecuteNonQuery();
                return Ok("done");
            }
            catch (Exception e)
            {

                return BadRequest("error");
            }
            
        }
    }
}
