using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracticeTask5
{
    internal static class VacationGenerator
    {
        internal static bool TryRandomGeneration(List<Employee> employees, in VacationParameters vacParams)
        {
            if (!IsGenerationPossible(employees.Count, vacParams))
                return false;

            Random gen = new Random();
            DateOnly startYear = new DateOnly(vacParams.Year, 1, 1);
            DateOnly endYear = new DateOnly(vacParams.Year, 12, 31);
            int vacStartRange = (endYear.DayNumber - startYear.DayNumber - vacParams.VacationIntervals.Min());

            int KOCTbIJlb = 0; // Мне жаль

            foreach (var employee in employees)
            {
                int remainVacationDuration = vacParams.TotalVacationDays;
                while (remainVacationDuration > 0)
                {
                    if (KOCTbIJlb++ == 10000)
                        return false;

                    DateOnly startVacation;
                    do startVacation = startYear.AddDays(gen.Next(vacStartRange));
                    while (!IsValidStartDate(startVacation, vacParams, employees, employee));

                    int difference;
                    do difference = vacParams.VacationIntervals[gen.Next(vacParams.VacationIntervals.Length)];
                    while (difference > remainVacationDuration);

                    DateOnly endVacation = startVacation.AddDays(difference);
                    if (!IsValidEndDate(endVacation, vacParams, employees, employee))
                        continue;

                    Vacation currentVacation = new Vacation(startVacation, endVacation);
                    if (!IsValidVacation(currentVacation, employees))
                        continue;

                    employee.Vacations.Add(currentVacation);
                    remainVacationDuration -= difference;
                }
            }

            VacationsSort(employees);
            return true;
        }

        private static bool IsValidStartDate(DateOnly startDate, VacationParameters vacParams, List<Employee> employees, Employee currentEmployee)
        {
            if (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday) // Проверка на то, что дата начала отпуска приходится на рабочий день недели (ПН-ПТ включительно)
                return false;

            if (startDate.AddDays(vacParams.VacationIntervals.Min()).Year != vacParams.Year) // Проверка на то, что для текущего начала отпуска возможна генерация отпуска, не уходящая на следующий год
                return false;

            bool notValidAllEmpCondition = employees.Any(employee =>
                                                employee.Vacations.Any(vacation =>
                                                    vacation.StartDate <= startDate &&
                                                    vacation.EndDate.AddDays(vacParams.MinDaysBetweenVac) >= startDate)); // Проверка на минимальный промежуток между отпусками разных сотрудников
            if (notValidAllEmpCondition)
                return false;

            bool notValidSameEmpCondition = currentEmployee.Vacations.Any(vacation => 
                                                vacation.StartDate <= startDate &&
                                                vacation.EndDate.AddDays(vacParams.MinDaysBetweenSameEmployeeVac) >= startDate); //Проверка на минимальный промежуток между отпусками одного сотрудника
            if (notValidSameEmpCondition)
                return false;

            return true;
        }

        private static bool IsValidEndDate (DateOnly endDate, VacationParameters vacParams, List<Employee> employees, Employee currentEmployee)
        {
            if (endDate.Year != vacParams.Year) // Проверка, что текущая генерация отпуска не ушла на следующий год
                return false;

            bool notValidAllEmpCondition = employees.Any(employee =>
                                                employee.Vacations.Any(vacation =>
                                                    vacation.StartDate.AddDays(-vacParams.MinDaysBetweenVac) <= endDate &&
                                                    vacation.EndDate >= endDate)); // Проверка на минимальный промежуток между отпусками остальных сотрудников
            if (notValidAllEmpCondition)
                return false;

            bool notValidSameEmpCondition = currentEmployee.Vacations.Any(vacation =>
                                                vacation.StartDate.AddDays(-vacParams.MinDaysBetweenSameEmployeeVac) <= endDate &&
                                                vacation.EndDate >= endDate); //Проверка на минимальный промежуток между отпусками одного сотрудника
            if (notValidSameEmpCondition)
                return false;

            return true;
        }

        private static bool IsValidVacation (Vacation currentVacation, List<Employee> employees) // Дополнительная проверка на пересечение с отпусками других сотрудников
        {
            return !employees.Any(employee =>
                        employee.Vacations.Any(vacation =>
                            vacation.StartDate >= currentVacation.StartDate && vacation.StartDate <= currentVacation.StartDate ||
                            vacation.EndDate >= currentVacation.EndDate && vacation.EndDate <= currentVacation.EndDate));
        }

        private static void VacationsSort(List<Employee> employees)
        {
            foreach (var employee in employees)
            {
                employee.Vacations.Sort((vacation1, vacation2) => vacation1.StartDate.CompareTo(vacation2.StartDate));
            }
        }

        // TODO Проверка на то, что с текущими VacationIntervals теоретически возможна генерация отпусков длиной TotalVacationDays при худшем расположении отпусков сотрудников
        private static bool IsGenerationPossible(int numberOfEmployees, VacationParameters vacParams)
        {
            int numberOfDays = (new DateOnly(vacParams.Year, 12, 31).DayNumber - new DateOnly(vacParams.Year, 1, 1).DayNumber);

            if (vacParams.TotalVacationDays + vacParams.TotalVacationDays / vacParams.VacationIntervals.Min() * vacParams.MinDaysBetweenSameEmployeeVac > numberOfDays)
                return false;

            if (numberOfEmployees * (vacParams.TotalVacationDays + vacParams.MinDaysBetweenVac) > numberOfDays)
                return false;

            return true;
        }
    }
}