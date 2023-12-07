using System;
using System.Collections.Generic;
using System.Linq;

namespace PracticeTask1
{
    class Program
    {
        static void Main(string[] args)
        {
            var employees = new Dictionary<string, List<DateOnly>>()
            {
                ["Иванов Иван Иванович"] = new List<DateOnly>(),
                ["Петров Петр Петрович"] = new List<DateOnly>(),
                ["Юлина Юлия Юлиановна"] = new List<DateOnly>(),
                ["Сидоров Сидор Сидорович"] = new List<DateOnly>(),
                ["Павлов Павел Павлович"] = new List<DateOnly>(),
                ["Георгиев Георг Георгиевич"] = new List<DateOnly>()
            };
            var workingDays = new List<DayOfWeek>()
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday
            };
            // Список отпусков сотрудников
            List<DateOnly> allVacations = new List<DateOnly>();
            Random gen = new Random();
            DateOnly start = new DateOnly(DateTime.Today.Year, 1, 1);
            DateOnly end = new DateOnly(DateTime.Today.Year, 12, 31);
            int range = (end.DayNumber - start.DayNumber);
            DateOnly startDate;
            DateOnly endDate;
            int remainVacationDuration;
            const int maxVacationDuration = 28;
            int[] vacationSteps = {
                7,
                14
            };
            foreach (var employee in employees)
            {
                remainVacationDuration = maxVacationDuration;
                while (remainVacationDuration > 0)
                {
                    startDate = start.AddDays(gen.Next(range));
                    endDate = end;

                    if (workingDays.Contains(startDate.DayOfWeek))
                    {
                        int difference = vacationSteps[remainVacationDuration == 7 ? 1 : gen.Next(vacationSteps.Length)];
                        endDate = startDate.AddDays(difference);

                        if (endDate.Year != end.Year)
                            continue;

                        // Если отпуска других сотрудников не в диапазоне трех дней и если другие отпуска этого сотрудника не в диапазоне месяца
                        if (!allVacations.Any(vacation => vacation.AddDays(3) >= startDate && vacation.AddDays(-3) <= endDate) &&
                            !employee.Value.Any(vacation => vacation.AddMonths(1) >= startDate && vacation.AddMonths(-1) <= endDate))
                        {
                            for (DateOnly dt = startDate; dt < endDate; dt = dt.AddDays(1))
                            {
                                allVacations.Add(dt);
                                employee.Value.Add(dt);
                            }
                            remainVacationDuration -= difference;
                        }
                    }
                }
            }
            foreach (var vacationList in employees)
            {
                Console.WriteLine("Дни отпуска " + vacationList.Key + " : ");
                vacationList.Value.ForEach(value => Console.WriteLine(value));
            }
            Console.ReadKey();
        }
    }
}
