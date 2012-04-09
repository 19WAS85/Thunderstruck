using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shakedown;
using Thunderstruck.Test.Dependencies;
using System.Data;

namespace Thunderstruck.Test.Cenarios
{
    public class DataContextTest
    {
        public void Execute()
        {
            var manufacturerId = 0;

            BeginTest.To(new DataContext())
                .Should("Get datacontext transaction mode").Assert(d => d.TransactionMode == Transaction.Begin)
                .Should("Get a value from database").Assert(d =>
                {
                    manufacturerId = d.GetValue<int>("SELECT TOP 1 TheId FROM Le_Manufacturer");
                    return manufacturerId > 0;
                })
                .Should("Insert car with a transactional datacontext").Assert(d =>
                {
                    var car = new Car { Name = "Esprit", ModelYear = 1976, ManufacturerId = manufacturerId };
                    var id = d.Execute("INSERT INTO Car VALUES (@Name, @ModelYear, @Date, @Chassis, @Mileage, @ManufacturerId)", car);
                    return id > 0;
                })
                .Should("Select new car without commit datacontext").Assert(d =>
                {
                    var car = d.First<Car>("SELECT * FROM Car WHERE Mileage IS NULL");
                    return car != null;
                })
                .Should("Commit datacontext transaction").Assert(d =>
                {
                    d.Commit();
                    return true;
                })
                .Should("Select commited data on database with another datacontext").Assert(d =>
                {
                    using (var dataContext = new DataContext())
                    {
                        var cars = dataContext.All<Car>("SELECT * FROM Car");
                        return cars.Count == 1;
                    }
                })
                .Should("Get values from database").Assert(d =>
                {
                    var mileages = d.GetValues<double?>("SELECT Mileage FROM Car");
                    return mileages.Count == 1 && mileages[0] == null;
                })
                .Should("Throw error when property format exception").Assert(d =>
                {
                    try
                    {
                        var car = d.First<ErrorCar>("SELECT * FROM Car");
                    }
                    catch (FormatException) { return true; }

                    return false;
                })
                .Should("Dispose datacontext close connection and transaction").Assert(d =>
                {
                    d.Dispose();
                    return d.Provider.DbConnection.State == ConnectionState.Closed;
                });
        }
    }
}
