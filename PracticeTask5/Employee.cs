using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracticeTask5
{
    public class Employee
    {
        public string Name { get; set; }
        public List<Vacation> Vacations { get; set; }

        public Employee(string name)
        {
            Name = name;
            Vacations = new List<Vacation>();
        }
    }
}
