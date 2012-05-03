using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thunderstruck.Test.Dependencies;
using Shakedown;
using System.Data;
using Thunderstruck.Runtime;

namespace Thunderstruck.Test.Cenarios
{
    public class DataObjectsTest
    {
        public void Execute()
        {
            ProviderResolver.CustomProviderType = typeof(AnotherSqlProvider);
            var commandManufacturer = new DataObjectCommand<Manufacturer>(table: "Le_Manufacturer", primaryKey: "TheId");
            var commandCar = new DataObjectCommand<Car>();
            var selectManufacturer = new DataObjectQuery<Manufacturer>("{0} FROM Le_Manufacturer");
            var selectCar = new DataObjectQuery<Car>();
            var manufacturerId = 0;

            BeginTest.To(new Manufacturer { Name = "Lotus", BuildYear = 1952 }).ThrowExceptions()
                .Should("Not has id before insert object on database").Assert(m => m.TheId == 0)
                .Should("Be possible insert a manufacturer and bind generated id").Assert(m =>
                {
                    commandManufacturer.Insert(m);
                    manufacturerId = m.TheId;
                    return manufacturerId > 0;
                })
                .Should("Commands and query manufacturers with custom datacontext").Assert(m =>
                {
                    using (var datacontext = new DataContext())
                    {
                        var manufacturer = new Manufacturer { Name = "General Motors", BuildYear = 1908 };
                        commandManufacturer.Insert(manufacturer, datacontext);

                        var manufacturers = selectManufacturer.With(datacontext).All();
                        return manufacturers.Count == 2;

                        // Not Committed!
                    }
                })
                .Should("Have used custom provider to insert").Assert(c => AnotherSqlProvider.ThisWasUsed);

            BeginTest.To(new Car { Name = "Esprit Turbo", ModelYear = 1981, Mileage = 318, ManufacturerId = manufacturerId })
                .Should("Insert car with manufacturer id previously created").Assert(c =>
                {
                    commandCar.Insert(c);
                    var insertedCar = selectCar.First("WHERE Name = @Name", c);
                    return insertedCar != null;
                })
                .Should("Save correct properties on database").Assert(c =>
                {
                    var car = selectCar.First("WHERE Id = @Id", c);
                    return car.Name == "Esprit Turbo" &&
                        car.ModelYear == 1981 &&
                        car.Mileage == 318 &&
                        car.ManufacturerId == manufacturerId &&
                        car.Date.Day == DateTime.Today.Day;
                })
                .Should("Update car informations on database").Assert(c =>
                {
                    c.Name = "Esprit S3";
                    commandCar.Update(c);
                    var updatedCar = selectCar.First("WHERE Id = @Id", c);
                    return updatedCar.Name == "Esprit S3";
                })
                .Should("Delete created car").Assert(c =>
                {
                    commandCar.Delete(c);
                    var deletedCar = selectCar.First("WHERE Id = @Id", c);
                    return deletedCar == null;
                });
        }
    }
}
