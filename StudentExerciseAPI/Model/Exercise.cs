using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseAPI.Model
{
    public class Exercise
    {
        public Exercise(int id, string name, string language)
        {
            Id = id;
            Name = name;
            Language = language;


        }

        public Exercise()
        {

        }

        public int Id { get; set; }
        public string Name { get; set; }

        [Required(ErrorMessage = "Exercise must have a language associated")]
        public string Language { get; set; }

        public List<Student> Students { get; set; }

    }
}
