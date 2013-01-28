using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thunderstruck.Test.Dependencies
{
    public class Car
    {
        public Car()
        {
            Date = DateTime.Today;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int ModelYear { get; set; }

        public DateTime Date { get; set; }

        public double? Mileage { get; set; }

        public CarCategory Category { get; set; }

        public string Chassis
        {
            get { return String.Concat(ManufacturerId, "_", Name).ToUpper(); }
        }

        public int ManufacturerId { get; set; }

        [Ignore]
        public HttpStyleUriParser MadIgnoredProperty { get; set; }
    }
}
