using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RawnaqProject.Models;
using System.Text.Json;

namespace RawnaqProject.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class getnameAPIController : ControllerBase
    {
        // getting users search by role
        [HttpGet("{role}")]
        public IEnumerable<usersaccounts> userSearch(string role)
        {
            List<usersaccounts> li = new List<usersaccounts>();
            var builder = WebApplication.CreateBuilder();
            string conStr = builder.Configuration.GetConnectionString("RawnaqProjectContext");
            SqlConnection conn1 = new SqlConnection(conStr);
            string sql;
            sql = "SELECT * FROM usersaccounts where role ='" + role + "' ";
            SqlCommand comm = new SqlCommand(sql, conn1);
            conn1.Open();
            SqlDataReader reader = comm.ExecuteReader();

            while (reader.Read())
            {
                li.Add(new usersaccounts
                {
                    Id = (int)reader["Id"],
                    name = (string)reader["name"],
                    pass = (string)reader["pass"],
                    role = (string)reader["role"],
                });

            }

            reader.Close();
            conn1.Close();
            return li;
        }
      
    }
}
