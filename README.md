Thunderstruck
=============

Thunderstruck is a .NET library that makes access to databases simpler and faster using ADO.NET. A really fast way to access databases.

Another ORM?
------------

No! Thunderstruck isn't an ORM. It doesn't abstract the features of the database, just makes accessing the database easier.

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

Executes a non-transactional command:

    using (var context = new DataContext(Transaction.No))
    {
        context.Execute("DELETE FROM Cars");
        context.Execute("DELETE FROM Tools");
    }

DataContext uses the Connection String named "Default" in App.config (or Web.config):

    <add name="Default" providerName="..." connectionString="..." />

If you need a different default connection string, you can specify the DefaultConnectionString name to use on the DataContext class:

    DataContext.DefaultConnectionStringName = "AnotherDefaultConnection";

Or, to connect just one context to another database:

    var context = new DataContext("AnotherDatabaseConnection");

If you need to connect to a dynamic connection string, pass a ConnectionStringSettings object to the DataContext class:

    var context = new DataContext(new ConnectionStringSettings("myDb", "myConnectionString"));

Parameters binding of the Thunderstruck commands prevents string concatenation, errors, conversions and SQL injection:

    var insertParams = new { Name = "Esprit Turbo", ModelYear = 1981 };

    context.Execute("INSERT INTO Cars VALUES (@Name, @ModelYear)", insertParams);

You can use any object to bind:

    context.Execute("INSERT INTO Dates VALUES (@Year, @Month, @Day)", DateTime.Today);

Or you can use a params array to bind values:

    context.Execute("INSERT INTO Dates VALUES (@0, @1, @2)", 2005, 3, 31);

### DataContext — Reading

Reading a value from the database is simple with Thunderstruck:

    var carsCount = context.GetValue("SELECT COUNT(Id) From Cars");

Or specify the type to cast the value to when reading:

    var last = context.GetValue<DateTime>("SELECT MAX(CreatedAt) From Cars");

Or get a list of values:

    var carNames = context.GetValues<string>("SELECT Name FROM Cars")

Read data from the database and fill it to object:

    var car = context.First<Car>("SELECT TOP 1 Id, Name, ModelYear, Category FROM Cars");

    car.Id // 318
    car.Name // "Lotus Essex Turbo Esprit"
    car.ModelYear // 1980
    car.Category // CarCategory.Sport (enum!)

Or get a list of objects:

    var cars = context.All<Car>("SELECT * FROM Cars");

Or, load into a dynamic object. Each field name in the data source will be added dynamically:

	dynamic car = context.First<ExpandoObject>("SELECT Name, CreatedAt as CreatedOn FROM Car");
	IEnumerable<dynamic> cars = context.All<ExpandoObject>("SELECT * FROM Car");

Parameters binding works likewise:

    context.All<Car>("SELECT * FROM Cars WHERE Name LIKE %@0%", "Lotus");

You can call stored procedures with Thunderstruck:

    var whoResults = context.All<WhoResult>("EXEC sp_who @0", "active");

### DataContext — Extending

Thunderstruck supports SQL Server, Oracle and MySQL with these providers name:
    
    System.Data.SqlClient
    System.Data.OracleClient
    MySql.Data.MySqlClient

It is easy to create a custom provider, usually less than 50 lines of code ([extending DefaultProvider][sql-provider-link]).

There are two ways to add a custom provider:

1. Add it as an additional provider: ProviderFactory.AddProvider("My.Provider", typeof(MyProvider))
2. Add it as a custom provider: ProviderFactory.CustomProvider = (providerName) => new MyCustomProvider();

Note, when you set a CustomProvider, that is the only provider available to Thunderbird! If you want to use multiple
providers, use the AddProvider() method.

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