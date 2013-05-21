using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Thunderstruck.Test.Models;
using System.Linq;
using System.Data.SqlClient;
using Thunderstruck.Provider;

namespace Thunderstruck.Test.Functional
{
    [TestClass]
    public class SqlTest
    {
        private static SqlEnvironment environment;
        private DataObject<Car> carDataObject;
        private DataObject<Manufacturer> manufacturerDataObject;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tests)
        {
            environment = new SqlEnvironment();
        }

        [TestInitialize]
        public void Initialize()
        {
            carDataObject = new DataObject<Car>();
            manufacturerDataObject = new DataObject<Manufacturer>(table: "Le_Manufacturer", primaryKey: "TheId");

            ProviderFactory.CustomProvider = null;
            ProviderFactory.ConnectionFactory = null;
        }

        [TestMethod]
        public void A_DataContext_Transitional_Should_Execute_A_Insert_SQL_Command_With_Anonymous_Object_Parameters_And_Read_Created_Data_In_Same_And_Count_Data_In_Another_Context()
        {
            using (var context = new DataContext())
            {
                var insertParameters = new { Name = "Lotus", Year = 1952 };
                context.Execute("INSERT INTO Le_Manufacturer VALUES (@Name, @Year)", insertParameters);
                var manufacturer = context.First<Manufacturer>("SELECT TOP 1 * FROM Le_Manufacturer");
                context.Commit();

                manufacturer.TheId.Should().BeGreaterThan(0);
                manufacturer.Name.Should().Be("Lotus");
                manufacturer.BuildYear.Should().Be(1952);
            }

            using (var context = new DataContext())
            {
                var manufacturerCount = context.GetValue<int>("SELECT COUNT(TheId) FROM Le_Manufacturer");

                manufacturerCount.Should().Be(1);
            }
        }

        [TestMethod]
        public void B_DataContext_Transitional_Should_Execute_A_Insert_SQL_Command_With_Array_Parameters_And_Read_Created_Data_In_Same_And_Count_Data_In_Another_Context()
        {
            using (var context = new DataContext())
            {
                context.Execute("INSERT INTO Le_Manufacturer VALUES (@0, @1)", "McLaren", 1963);
                var manufacturer = context.First<Manufacturer>("SELECT TOP 1 * FROM Le_Manufacturer WHERE Name = @0", "McLaren");
                context.Commit();

                manufacturer.TheId.Should().BeGreaterThan(0);
                manufacturer.Name.Should().Be("McLaren");
                manufacturer.BuildYear.Should().Be(1963);
            }

            using (var context = new DataContext())
            {
                var manufacturerCount = context.GetValue<int>("SELECT COUNT(TheId) FROM Le_Manufacturer");

                manufacturerCount.Should().Be(2);
            }
        }

        [TestMethod]
        public void C_DataContext_Non_Transitional_Should_Execute_A_Insert_SQL_Command_With_Object_Parameters_And_Read_Created_Data_In_Same_And_Count_Data()
        {
            using (var context = new DataContext(Transaction.No))
            {
                var insertParameters = new Manufacturer { Name = "General Motors", BuildYear = 1908 };
                context.Execute("INSERT INTO Le_Manufacturer VALUES (@Name, @BuildYear)", insertParameters);
                var manufacturer = context.First<Manufacturer>("SELECT * FROM Le_Manufacturer WHERE BuildYear = 1908");
                var manufacturerCount = context.GetValue<int>("SELECT COUNT(TheId) FROM Le_Manufacturer");

                manufacturer.TheId.Should().BeGreaterThan(0);
                manufacturer.Name.Should().Be("General Motors");
                manufacturer.BuildYear.Should().Be(1908);
                manufacturerCount.Should().Be(3);
            }
        }

        [TestMethod]
        public void D_DataContext_Transitional_Should_Not_Commit_Data_If_Not_Explicit_Commit_Command_Call()
        {
            using (var context = new DataContext())
            {
                var insertParameters = new { Name = "Bentley", Year = 1919 };
                context.Execute("INSERT INTO Le_Manufacturer VALUES (@Name, @Year)", insertParameters);
            }

            using (var context = new DataContext())
            {
                var manufacturers = context.All<Manufacturer>("SELECT * FROM Le_Manufacturer");

                manufacturers.Count().Should().Be(3);
                manufacturers.Should().Contain(m => m.Name == "Lotus");
                manufacturers.Should().Contain(m => m.Name == "General Motors");
            }
        }

        [TestMethod]
        public void E_DataContext_Transitional_Should_Execute_A_SQL_And_Share_Transaction_With_A_Data_Reader()
        {
            using (var context = new DataContext())
            {
                var insertParameters = new { Name = "BMW", Year = 1916 };
                context.Execute("INSERT INTO Le_Manufacturer VALUES (@Name, @Year)", insertParameters);
                var selectParameters = new { Name = "BMW" };
                var manufacturer = context.First<Manufacturer>("SELECT * FROM Le_Manufacturer WHERE Name = @Name", selectParameters);

                manufacturer.Should().NotBeNull();
            }

            using (var context = new DataContext())
            {
                var selectParameters = new { Name = "BMW" };
                var manufacturer = context.First<Manufacturer>("SELECT * FROM Le_Manufacturer WHERE Name = @Name", selectParameters);

                manufacturer.Should().BeNull();
            }
        }

        [TestMethod]
        public void F_DataContext_Should_Read_Data_With_One_Field_And_Insert_A_Data_With_Some_Parameters_Types()
        {
            using (var context = new DataContext())
            {
                var selectParameters = new { Name = "Lotus" };
                var manufacturer = context.First<Manufacturer>("SELECT TheId FROM Le_Manufacturer WHERE Name = @Name", selectParameters);
                
                var insertParameters = new Car
                {
                    Name = "Esprit Turbo",
                    ModelYear = 1981,
                    Mileage = 318.19850801,
                    ManufacturerId = manufacturer.TheId,
                    Category = CarCategory.Sport
                };

                context.Execute("INSERT INTO Car VALUES (@Name, @ModelYear, @CreatedAt, @Chassis, @Mileage, @Category, @ManufacturerId)", insertParameters);
                var car = context.First<Car>("SELECT TOP 1 * FROM Car");
                context.Commit();

                car.Id.Should().BeGreaterThan(0);
                car.Name.Should().Be("Esprit Turbo");
                car.ModelYear.Should().Be(1981);
                car.Mileage.Should().Be(318.19850801);
                car.ManufacturerId.Should().Be(manufacturer.TheId);
                car.CreatedAt.Date.Should().Be(DateTime.Today.Date);
                car.Category.Should().Be(CarCategory.Sport);
            }
        }

        [TestMethod]
        public void G_DataContext_Should_Execute_A_Procedure()
        {
            using (var context = new DataContext())
            {
                var procedureParams = new { Status = "active" };
                var whoResults = context.All<WhoResult>("EXEC sp_who @Status", procedureParams);

                whoResults.Should().Contain(r => r.dbname == "ThunderTest" && r.cmd == "SELECT");
            }
        }

        [TestMethod]
        public void H_DataContext_Reader_Should_Translate_DBNull_To_Null_Value()
        {
            using (var context = new DataContext())
            {
                context.Execute("UPDATE Car SET Name = NULL WHERE Name = 'Esprit Turbo'");
                var car = context.First<Car>("SELECT * FROM Car WHERE Name IS NULL");

                car.Name.Should().BeNull();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void I_DataContext_Should_Throws_An_Exception_When_Execute_An_Incorrect_Command()
        {
            using (var context = new DataContext())
            {
                context.Execute("DELETE FROM Blargh");
            }
        }

        [TestMethod]
        public void J_DataContext_Reader_First_Should_Return_A_Null_Object_When_Query_Do_Not_Found_Items()
        {
            using (var context = new DataContext())
            {
                var car = context.First<Car>("SELECT * FROM Car WHERE Name = 'Fusca'");

                car.Should().BeNull();
            }
        }

        [TestMethod]
        public void K_DataContext_Reader_All_Should_Return_An_Empty_Object_List_When_Query_Do_Not_Found_Items()
        {
            using (var context = new DataContext())
            {
                var cars = context.All<Car>("SELECT * FROM Car WHERE Name = 'Fusca'");

                cars.Should().BeEmpty();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void L_DataContext_Should_Throws_An_Exeption_When_A_Constraint_Was_Broken()
        {
            using (var context = new DataContext())
            {
                var insertParameters = new Car
                {
                    Name = "AGL 1113",
                    ModelYear = 1964,
                    Mileage = 1567922,
                    ManufacturerId = 98765, // Invalid ManufacturerId
                    Category = CarCategory.Transport
                };

                context.Execute("INSERT INTO Car VALUES (@Name, @ModelYear, @CreatedAt, @Chassis, @Mileage, @Category, @ManufacturerId)", insertParameters);
            }
        }

        [TestMethod]
        public void M_DataObject_Should_Insert_Item_From_Database_And_Set_Primery_Key()
        {
            var car = new Car { Name = "Aventador", ModelYear = 2011, Category = CarCategory.Sport };
            carDataObject.Insert(car);

            car.Id.Should().BeGreaterThan(0);

            using (var context = new DataContext())
            {
                var query = "SELECT TOP 1 * FROM Car WHERE Name = 'Aventador'";
                var createdManufacturer = context.First<Car>(query);

                createdManufacturer.Should().NotBeNull();
            }
        }

        [TestMethod]
        public void N_DataObject_Should_Read_Item_From_Database()
        {
            var car = carDataObject.Select.First("WHERE Name = 'Aventador'");

            car.Name.Should().Be("Aventador");
            car.ModelYear.Should().Be(2011);
        }

        [TestMethod]
        public void O_DataObject_Should_Create_Item_From_Database_With_Custom_Table_Name_And_Primary_Key()
        {
            var manufacturer = new Manufacturer { Name = "Lamborghini", BuildYear = 1963 };
            manufacturerDataObject.Insert(manufacturer);
            var queryParams = new { Name = "Lamborghini" };
            var createdManufacturer = manufacturerDataObject.Select.First("WHERE Name = @Name", queryParams);

            manufacturer.TheId.Should().BeGreaterThan(0);
            createdManufacturer.Name.Should().Be("Lamborghini");
            createdManufacturer.BuildYear.Should().Be(1963);
        }

        [TestMethod]
        public void P_DataObject_Should_Update_Item_From_Database()
        {
            var lamborghini = manufacturerDataObject.Select.First("WHERE Name = 'Lamborghini'");
            var aventador = carDataObject.Select.First("WHERE Name = 'Aventador'");
            aventador.ManufacturerId = lamborghini.TheId;
            carDataObject.Update(aventador);
        }

        [TestMethod]
        public void Q_DataObject_Should_Read_All_Items_From_Database()
        {
            var cars = carDataObject.Select.All();

            cars.Count().Should().Be(2);
            cars.Should().Contain(c => c.Name == "Esprit Turbo");
            cars.Should().Contain(c => c.Name == "Aventador");
        }

        [TestMethod]
        public void R_DataObject_Should_Delete_Item_From_Database()
        {
            var aventador = carDataObject.Select.First("WHERE Name = 'Aventador'");
            carDataObject.Delete(aventador);
            var deletedAventador = carDataObject.Select.First("WHERE Name = 'Aventador'");

            deletedAventador.Should().BeNull();
        }

        [TestMethod]
        public void S_DataObject_Should_CRUD_An_Item_With_Shared_Transactional_Data_Context()
        {
            using (var context = new DataContext())
            {
                var dodge = new Manufacturer { Name = "Dodge", BuildYear = 1900 };
                manufacturerDataObject.Insert(dodge, context);

                dodge.BuildYear = 1915;
                manufacturerDataObject.Update(dodge, context);
                var updatedDodge = context.First<Manufacturer>("SELECT TOP 1 * FROM Le_Manufacturer WHERE Name = 'Dodge'");
                
                var m4s = new Car { Name = "M4S", ModelYear = 1984, Category = CarCategory.Prototype, ManufacturerId = dodge.TheId };
                carDataObject.Insert(m4s, context);
                var createdM4s = carDataObject.Select.With(context).First("WHERE Name = 'M4S'");

                carDataObject.Delete(m4s, context);
                var deletedM4s = carDataObject.Select.With(context).First("WHERE Name = 'M4S'");

                updatedDodge.BuildYear.Should().Be(1915);
                createdM4s.Should().NotBeNull();
                deletedM4s.Should().BeNull();

                context.Commit();
            }

            var createdDodgeOutsiteContext = manufacturerDataObject.Select.First("WHERE Name = 'Dodge'");
            createdDodgeOutsiteContext.Should().NotBeNull();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            ProviderFactory.CustomProvider = null;
            ProviderFactory.ConnectionFactory = null;

            environment.Dispose();
        }
    }
}