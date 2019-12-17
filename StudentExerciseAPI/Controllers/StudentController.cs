using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExerciseAPI.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Get([FromQuery]string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {   
                    
                    if (include != null)
                    {
                        if (include == "exercise")
                        {
                            cmd.CommandText += @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Name AS CohortName,
                                                 e.Id AS ExerciseId, e.Language, e.Name
                                                 FROM Student s
                                                 INNER JOIN Cohort c On s.CohortId = c.Id
                                                 INNER JOIN StudentExercise se ON se.StudentId = s.Id 
                                                 INNER JOIN Exercise e ON se.ExerciseId = e.Id";
                        
                        }
                    }else
                    {
                        cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Name AS CohortName
                                      FROM Student s
                                      INNER JOIN Cohort c On s.CohortId = c.Id
                                      ";
                    }
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Student> students = new List<Student>();

                    while (reader.Read())
                    {
                       

                        //Cohort
                        int idColumnPosition = reader.GetOrdinal("CohortId");
                        int idValue = reader.GetInt32(idColumnPosition);

                        int nameColonPosition = reader.GetOrdinal("CohortName");
                        string cohortNameValue = reader.GetString(nameColonPosition);

                        //Student
                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = idValue,
                                Name = cohortNameValue
                            },
                        
                        };

                        if (include != null)
                        {
                            //Exercise
                            int idExerciseValue = reader.GetInt32(reader.GetOrdinal("ExerciseId"));
                            string nameValue = reader.GetString(reader.GetOrdinal("Name"));
                            string languageValue = reader.GetString(reader.GetOrdinal("Language"));
                            List<Exercise> exercises = new List<Exercise>();
                            Exercise newExercise = new Exercise(idExerciseValue, nameValue, languageValue);
                            exercises.Add(newExercise);
                            student.Exercises = exercises;
                        }

                        students.Add(student);
                    };





                    reader.Close();

                    return Ok(students);
                }
            }
        }

        [HttpGet("{id}", Name = "GetStudent")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId as StudentCohortId, c.Id as CohortId, c.Name
                        FROM Student s
                        LEFT JOIN Cohort c on c.id = s.CohortId
                        WHERE s.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    if (reader.Read())
                    {
                        int idColumnPosition = reader.GetOrdinal("CohortId");
                        int idValue = reader.GetInt32(idColumnPosition);

                        int nameColonPosition = reader.GetOrdinal("Name");
                        string cohortNameValue = reader.GetString(nameColonPosition);
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohortId")),
                            Cohort = new Cohort
                            {
                                Id = idValue,
                                Name = cohortNameValue
                            }
                        };


                    };
                    reader.Close();

                    if (student == null)
                    {
                        return NotFound($"No Student found with the id of {id}");
                    }
                    return Ok(student);
                }
            }
        }
        
        /*
        [HttpGet]
        [Route("studentCohort")]
        public async Task<IActionResult> Get([FromQuery]int? cohortId, [FromQuery]string lastName)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId as StudentCohortId, c.Id as CohortId, c.Name
                                        FROM Student s
                                        LEFT JOIN Cohort c on c.id = s.CohortId
                                        WHERE 1=1";
                    if (cohortId != null)
                    {
                        cmd.CommandText += " AND CohortId = @cohortId";
                        cmd.Parameters.Add(new SqlParameter("@cohortId", cohortId));
                    }
                    if (lastName != null)
                    {
                        cmd.CommandText += " AND LastName LIKE @lastName";
                        cmd.Parameters.Add(new SqlParameter("@lastName", "%" + lastName + "%"));
                    }
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Student> allStudents = new List<Student>();
                    while (reader.Read())
                    {
                        Student stu = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };
                        allStudents.Add(stu);
                    }
                    reader.Close();
                    return Ok(allStudents);
                }
            }
        }

        [HttpGet]
        [Route("studentExercise")]
        public async Task<IActionResult> GetStudentExercise([FromQuery]string include, [FromQuery]int? exerciseID)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Name AS CohortName, +
                                      e.Id AS ExerciseId, e.Language, e.Name
                                      FROM Student s
                                      INNER JOIN Cohort c On s.CohortId = c.Id
                                      INNER JOIN StudentExercise se ON se.StudentId = s.Id
                                      INNER JOIN Exercise e ON se.ExerciseId = e.Id
                                      WHERE 1=1
                                      GROUP BY s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Name, e.Language, e.Name, e.Id"
                                      ;
                    if (exerciseID != null)
                    {
                        cmd.CommandText += " AND ExerciseId = @exerciseId";
                        cmd.Parameters.Add(new SqlParameter("@exerciseId", exerciseID));
                    }
                    SqlDataReader reader = cmd.ExecuteReader();

                   
                    List<Student> allStudents = new List<Student>();
                    while (reader.Read())
                    {

                        int idValue = reader.GetInt32(reader.GetOrdinal("ExerciseId"));
                        string nameValue = reader.GetString(reader.GetOrdinal("Name"));
                        string languageValue = reader.GetString(reader.GetOrdinal("Language"));
                        List<Exercise> exercises = new List<Exercise>();
                        Exercise newExercise = new Exercise(idValue, nameValue, languageValue);
                        exercises.Add(newExercise);


                        Student stu = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Exercises = exercises
                    };
                        allStudents.Add(stu);
                    }
                    reader.Close();
                    return Ok(allStudents);
                }
            }
        }
        */

        private bool StudentExist(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, SlackHandle, CohortId 
                        FROM Student
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }

    

}
