using System.Collections.Generic;

namespace StudentExerciseAPI.Model
{
    public class Student
    {



        /*
        public Student(int id, string firstName, string lastname, string slack, Cohort cohort)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastname;
            SlackHandle = slack;
            CohortId = cohort;
        }
        */

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SlackHandle { get; set; }
        public Cohort Cohort { get; set; }

        public int CohortId { get; set; }


        public List<Exercise> Exercises { get; set; }
    }
}

