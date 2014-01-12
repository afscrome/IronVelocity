using System;

namespace IronVelocity.Tests.Regression.Provider
{
    public class Something
    {
        private string firstName = "hammett";
        private string middleNameInitial = "V";

        public String Print(String arg)
        {
            return arg;
        }

        public String Contents(params String[] args)
        {
            return String.Join(",", args);
        }

        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public string MiddleNameInitial
        {
            get { return middleNameInitial; }
            set { middleNameInitial = value; }
        }
    }

    public class Something2
    {
        public String FormatDate(DateTime dt)
        {
            return dt.Day.ToString();
        }

        public String Contents(String name, int age, params String[] args)
        {
            return name + age + String.Join(",", args);
        }
    }
}
