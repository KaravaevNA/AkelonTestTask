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

            int crutch = 0; // Поскольку в методе IsGenerationPossible не до конца реализована проверка на общее кол-во дней отпусков сотрудников с учетом промежутков между ними, здесь используется временная мера в виде проверки на кол-во попыток генерации
            int crutchTryNumber = vacStartRange * 1000;

            foreach (var employee in employees)
            {
                int remainVacationDuration = vacParams.TotalVacationDays;
                while (remainVacationDuration > 0)
                {
                    if (crutch++ == crutchTryNumber)
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

        // Проверка на то, что с текущими VacationIntervals теоретически возможна генерация отпусков длиной TotalVacationDays при худшем распределении отпусков сотрудников
        private static bool IsGenerationPossible(int numberOfEmployees, VacationParameters vacParams)
        {
            int numberOfDays = (new DateOnly(vacParams.Year, 12, 31).DayNumber - new DateOnly(vacParams.Year, 1, 1).DayNumber);
            int numberOfDaysAllVacations = vacParams.TotalVacationDays * numberOfEmployees;
            int maxVacationsSameEmp = vacParams.TotalVacationDays / vacParams.VacationIntervals.Min(); // Берем именно максимально возможное количество отдельных отпусков сотрудника, чтобы рассмотреть худший случай генерации
            int maxDaysBetweenVacationsSameEmp = maxVacationsSameEmp * vacParams.MinDaysBetweenSameEmployeeVac;
            int maxDaysBetweenVacationsDifferentEmp = maxVacationsSameEmp * vacParams.MinDaysBetweenVac * numberOfEmployees;
            
            if (vacParams.TotalVacationDays + maxDaysBetweenVacationsSameEmp > numberOfDays || // Проверка на то, что отпуск одного сотрудника помещается в выбранный диапазон отпусков, без учета остальных сотрудников
                numberOfDaysAllVacations + maxDaysBetweenVacationsDifferentEmp > numberOfDays) // Проверка на то, что отпуска всех сотрудников помещаются в выбранный диапазон отпусков, без учета обязательного промежутка между отпусками одного сотрудника. Используется проверка на лучший сценарий генерации
                return false; // Остается нереализованной проверка на общее количество дней отпусков в случае, если отпуска одного сотрудника идут подряд. В качестве временной меры используется проверка на кол-во повторений с помощью crutch в основном блоке генерации TryRandomGeneration

            return true;
        }
    }
}