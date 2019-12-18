using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExerciseAPI.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Linq;

namespace StudentExerciseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string include, 
            [FromQuery] string firstName,
            [FromQuery] string lastName,
            [FromQuery] string orderBy,
            [FromQuery] string slackHandle)
        {
            if (include == "exercises")
            {
                var students = await GetStudentsWithExercises();
                return Ok(students);
            }
            else
            {
                var students = await GetAllStudents(orderBy, firstName, lastName, slackHandle);
                return Ok(students);
            }
            
        }

        private async Task<List<Student>> GetStudentsByName(string searchTerm)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, SlackHandle, CohortId 
                        FROM STUDENT ";

                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        cmd.CommandText += @"WHERE FirstName LIKE @q OR LastName LIKE @q";
                    }

                    cmd.Parameters.Add(new SqlParameter("@q", "%" + searchTerm + "%"));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    var students = new List<Student>();

                    while (reader.Read())
                    {
                        students.Add(new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                        });
                    }

                    reader.Close();
                    return students;
                }
            }
        }

        private async Task<List<Student>> GetStudentsWithExercises()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, e.[Name], e.Id as ExerciseId, e.[Language] 
                        FROM STUDENT s
                        LEFT JOIN StudentExercise se ON s.Id = se.StudentId
                        JOIN Exercise e ON se.ExerciseId = e.Id";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    var students = new List<Student>();

                    while (reader.Read())
                    {
                        var id = reader.GetInt32(reader.GetOrdinal("Id"));
                        var existingStudent = students.FirstOrDefault(s => s.Id == id);

                        if (existingStudent == null)
                        {
                            var newStudent = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            };

                            students.Add(newStudent);

                            newStudent.Exercises.Add(new Exercise
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Language = reader.GetString(reader.GetOrdinal("Language"))
                            });
                        }
                        else
                        {
                            existingStudent.Exercises.Add(new Exercise
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Language = reader.GetString(reader.GetOrdinal("Language"))
                            });
                        }

                        
                    }

                    reader.Close();
                    return students;
                }
            }
        }

        private async Task<List<Student>> GetAllStudents(
            string orderBy, 
            string firstName, 
            string lastName,
            string slackHandle)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, SlackHandle, CohortId 
                        FROM STUDENT
                        WHERE 1=1";

                    if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName) || !string.IsNullOrWhiteSpace(slackHandle))
                    {
                        cmd.CommandText += " AND FirstName LIKE @firstName";
                        cmd.Parameters.Add(new SqlParameter("@firstName", "%" + firstName + "%"));
                        cmd.CommandText += " AND LastName LIKE @lastName";
                        cmd.Parameters.Add(new SqlParameter("@lastName", "%" + lastName + "%"));
                        cmd.CommandText += " AND SlackHandle LIKE @slackHandle";
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", "%" + slackHandle + "%"));
                    }

                    if (orderBy == "asc")
                    {
                        cmd.CommandText += " ORDER BY LastName";
                    }
                    else if (orderBy == "desc")
                    {
                        cmd.CommandText += " ORDER BY LastName DESC";
                    }

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    var students = new List<Student>();

                    while (reader.Read())
                    {
                        students.Add(new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                        });
                    }

                    reader.Close();
                    return students;
                }
            }
        }
    }

    

}
