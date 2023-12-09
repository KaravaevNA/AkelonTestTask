using System;
using System.Collections.Generic;

namespace PracticeTask5
{
    public class Program
    {
        public static void Main(string[] args)
        {
            List<Employee> employees = new List<Employee>
            {
                new Employee("Иванов Иван Иванович"),
                new Employee("Петров Петр Петрович"),
                new Employee("Юлина Юлия Юлиановна"),
                new Employee("Сидоров Сидор Сидорович"),
                new Employee("Павлов Павел Павлович"),
                new Employee("Георгиев Георг Георгиевич")
            };

            VacationParameters vacParams = VacationParameters.Create(DateTime.Today.Year, 28, 3, 30, new int[] { 7, 14 });

            if (VacationGenerator.TryRandomGeneration(employees, in vacParams))
                foreach (Employee employee in employees)
                {
                    Console.WriteLine($"Сотрудник: {employee.Name}");
                    Console.WriteLine("Отпуска:");
                    foreach (Vacation vacation in employee.Vacations)
                        Console.WriteLine($"{(vacation.EndDate.DayNumber - vacation.StartDate.DayNumber):D2} Дней: {vacation.StartDate} - {vacation.EndDate}");
                    Console.WriteLine();
                }
            else
            {
                employees.ForEach(employee => employee.Vacations = new List<Vacation>());
                Console.WriteLine("Похоже, с подобными входными данными генерация отпусков невозможна");
            }
        }
    }
}