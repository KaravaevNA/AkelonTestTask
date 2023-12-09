using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracticeTask5
{
    public struct Vacation
    {
        public DateOnly StartDate { get; }
        public DateOnly EndDate { get; }

        public Vacation(DateOnly startDate, DateOnly endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }
    }

    public struct VacationParameters
    {
        public int Year { get; set; }
        public int TotalVacationDays { get; }
        public int MinDaysBetweenVac { get; }
        public int MinDaysBetweenSameEmployeeVac { get; }
        public int[] VacationIntervals { get; }

        private VacationParameters(int year, int totalVacationDays, int minDaysBetweenVac, int minDaysBetweenSameEmployeeVac, int[] vacationIntervals)
        {
            Year = year;
            TotalVacationDays = totalVacationDays;
            MinDaysBetweenVac = minDaysBetweenVac;
            MinDaysBetweenSameEmployeeVac = minDaysBetweenSameEmployeeVac;
            VacationIntervals = vacationIntervals;
        }

        public static VacationParameters Create(int year, int totalVacationDays, int minDaysBetweenVac, int minDaysBetweenSameEmployeeVac, int[] vacationIntervals)
        {
            VacationParameters parameters = new VacationParameters(year, totalVacationDays, minDaysBetweenVac, minDaysBetweenSameEmployeeVac, vacationIntervals);
            if (!parameters.IsValidParameters())
                throw new ArgumentException("Неверные параметры - ошибка в интервалах отпуска");
            return parameters;
        }

        private bool IsValidParameters()
        {
            int totalVacationDays = TotalVacationDays;
            return VacationIntervals.All(interval => totalVacationDays % interval == 0);
        }
    }
}
