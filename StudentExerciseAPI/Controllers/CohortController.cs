using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExerciseAPI.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CohortController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CohortController(IConfiguration config)
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
        public async Task<IActionResult> GetAllCohorts()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT cohort.id as CohortID, cohort.name as CohortName, i.id as InstructorID, i.FirstName as InstructorFirst, i.LastName as InstructorLast, i.slackHandle as InstructorSlack, i.Speciality, i.cohortId as InstructorCohortID, +
                                      s.id as StudentID, s.FirstName as StudentFirst, s.LastName as StudentLast, s.SlackHandle as StudentSlack, s.cohortId as StudentCohortID 
                                      FROM cohort
                                      LEFT JOIN student as s ON cohort.id = s.cohortId  
                                      LEFT JOIN instructor as i ON cohort.id = i.cohortId";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();
                    while (reader.Read())
                    {
                        int currentCohortID = reader.GetInt32(reader.GetOrdinal("CohortID"));
                        Cohort newCohort = cohorts.FirstOrDefault(i => i.Id == currentCohortID);
                        //If there's no cohort, create one and add it to the list.
                        if (newCohort == null)
                        {
                            newCohort = new Cohort
                            {
                                Id = currentCohortID,
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            };
                            cohorts.Add(newCohort);
                        }

                        //Add new student in Instructor's cohort list
                        int currentStudentID = reader.GetInt32(reader.GetOrdinal("StudentID"));
                        foreach (Cohort cohort in cohorts)
                        {
                            if (cohort.Id == reader.GetInt32(reader.GetOrdinal("StudentCohortId")) && cohort.Students.FirstOrDefault(s => s.Id == currentStudentID) == null)
                            {
                                cohort.Students.Add(new Student
                                {
                                    Id = currentStudentID,
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirst")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLast")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohortId")),
                                });
                            }
                        }

                        //Check if cohort matches instructor cohort id
                        int currentInstructorID = reader.GetInt32(reader.GetOrdinal("InstructorID"));
                        foreach (Cohort cohort in cohorts)
                        {
                            if (cohort.Id == reader.GetInt32(reader.GetOrdinal("InstructorCohortID")) && cohort.Instructors.FirstOrDefault(c => c.Id == currentInstructorID) == null)
                            {
                                cohort.Instructors.Add(new Instructor
                                {
                                    Id = currentInstructorID,
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirst")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLast")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack")),
                                    Specialty = reader.GetString(reader.GetOrdinal("Speciality")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("InstructorCohortID")),
                                });
                            }
                        }
                    }

                    reader.Close();

                    return Ok(cohorts);
                }
            }
        }
    }
}
