using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExerciseAPI.Model
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required(ErrorMessage = "A student must have a slack handle")]
        [StringLength(12, MinimumLength = 3, ErrorMessage = "Slack handle must be between 3 and 12 characters")]
        public string SlackHandle { get; set; }
        public Cohort Cohort { get; set; }

        public int CohortId { get; set; }


        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}

