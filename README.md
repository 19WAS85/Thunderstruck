Thunderstruck
=============

Thunderstruck is a .NET library that makes access to database simpler and faster using ADO.NET. A really fast way to access databases.

Another ORM?
------------

No! Thunderstruck isn't an ORM. It doesn't abstract the features of the database, just makes the access easier.

Install
-------

Use Nuget:

    PM> Install-Package Thunderstruck

Quick Guide
-----------

### DataContext — Executing

Executes a transactional SQL command with the DataContext:

    using (var context = new DataContext())
    {
        context.Execute("DELETE FROM Cars");
        context.Execute("DELETE FROM Tools");

        context.Commit();
    }

Executes a non transactional command:

    using (var context = new DataContext(Transaction.No))
    {
        context.Execute("DELETE FROM Cars");
        context.Execute("DELETE FROM Tools");
    }

DataContext uses the Connection String named "Default" in Application.config (or Web.config):

    <add name="Default" providerName="..." connectionString="..." />

If you need a different configuration, you can say to the DataContext what's the ConnectionString name to use:

    DataContext.DefaultConnectionStringName = "AnotherDefaultConnection";

Or, to connect just one context to another database:

    var context = new DataContext("AnotherDatabaseConnection");

Parameters binding of the Thunderstruck commands prevents string concatenation, errors, conversions and SQL injection:

    var insertParams = new { Name = "Esprit Turbo", ModelYear = 1981 };

    context.Execute("INSERT INTO Cars VALUES (@Name, @ModelYear)", insertParams);

You can use any object to bind:

    context.Execute("INSERT INTO Dates VALUES (@Year, @Month, @Day)", DateTime.Today);

Or you can use an params array to bind values:

    context.Execute("INSERT INTO Dates VALUES (@0, @1, @2)", 2005, 3, 31);

### DataContext — Reading

Read a value from database is simple with Thunderstruck:

    var carsCount = context.GetValue("SELECT COUNT(Id) From Cars");

Or use a typed read to cast the value of the reading:

    var last = context.GetValue<DateTime>("SELECT MAX(CreatedAt) From Cars");

Or take a list of objects:

    var carNames = context.GetValues<string>("SELECT Name FROM Cars")

Read data from database and fill it to object:

    var car = context.First<Car>("SELECT TOP 1 Id, Name, ModelYear, Category FROM Cars");

    car.Id // 318
    car.Name // "Lotus Essex Turbo Esprit"
    car.ModelYear // 1980
    car.Category // CarCategory.Sport (enum!)

Or a list of objects:

    var cars = context.All<Car>("SELECT * FROM Cars");

Parameters binding works likewise:

    context.All<Car>("SELECT * FROM Cars WHERE Name LIKE %@0%", "Lotus");

You can call procedures with Thunderstruck:

    var whoResults = context.All<WhoResult>("EXEC sp_who @0", "active");

### DataContext — Extending

Thunderstruck supports SQL Server, Oracle and MySQL with these providers name:
    
    System.Data.SqlClient
    System.Data.OracleClient
    MySql.Data.MySqlClient

But is easy to create a custom provider, less than 50 lines ([extending DefaultProvider][sql-provider-link]), and set it in Thunderstruck:

    ProviderFactory.CustomProvider = (providerName) => new MyCustomProvider();

### DataObject

Thunderstruck abstracts CRUD functions with DataObjects:
    
    var carData = new DataObject<Car>();

    var allCars = carData.Select.All();
    var lotusCars = carData.Select.All("WHERE Name LIKE '%Lotus%'");
    var newerCar = carData.Select.First("ORDER BY ModelYear DESC"); // SQL TOP 1

    var car = new Car { Name = "McLaren P1", ModelYear = 2013 };
    carData.Insert(car);

    // Primary key filled, car.Id > 0

    car.Category = CarCategory.Sport;
    carData.Update(car);

    carData.Delete(car);

DataObject can make transactional interactions to execute commands, just using a DataContext:

    carData.Insert(car, context);
    context.Commit();

Or, on read commands:
    
    carData.Select.With(context).All("ORDER BY Name");

Licence
-------

http://www.apache.org/licenses/LICENSE-2.0

[sql-provider-link]: http://github.com/wagnerandrade/Thunderstruck/blob/master/Thunderstruck/Provider/Common/SqlProvider.cs