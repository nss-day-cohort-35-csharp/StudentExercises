SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle,s.Speciality, s.CohortId as InstructorCohortId, c.Id as CohortId, c.Name
FROM Instructor s
LEFT JOIN Cohort c on c.id = s.CohortId

SELECT cohort.id as CohortID, cohort.name as CohortName, i.id as InstructorID, i.FirstName as InstructorFirst, i.LastName as InstructorLast, i.slackHandle as InstructorSlack, i.cohortId as InstructorCohortID, i.Speciality, s.id as StudentID, s.FirstName as StudentFirst, s.LastName as StudentLast, s.SlackHandle as StudentSlack, s.cohortId as StudentCohortID 
FROM cohort
LEFT JOIN student as s ON cohort.id = s.cohortId  
LEFT JOIN instructor as i ON cohort.id = i.cohortId 

SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Name AS CohortName, +
e.Id AS ExerciseId, e.Language, e.Name
FROM Student s
INNER JOIN Cohort c On s.CohortId = c.Id
INNER JOIN StudentExercise se ON se.StudentId = s.Id
INNER JOIN Exercise e ON se.ExerciseId = e.Id
GROUP BY s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Name, e.Language, e.Name, e.Id
