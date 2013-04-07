Thunderstruck
=============

Thunderstruck is a .NET library that makes access to database simpler and faster using ADO.NET. A really fast way to access databases.

Another ORM?
------------

No! Thunderstruck isn't a ORM. It doesn't abstract the powerful database, just makes the access easier.

Download
--------

Stable binary version => "download here":http://github.com/downloads/wagnerandrade/Thunderstruck/Thunderstruck-Bin-Stable.zip

Licence
-------

http://www.apache.org/licenses/LICENSE-2.0

Quick Guide
-----------

### DataContext

Default connection string.

    <connectionStrings>
        <add name="Default" providerName="..." connectionString="..." />
    </connectionStrings>

Default DataContext.

    using (var context = new DataContext())
    {
        context.Execute("DELETE FROM Cars");
        context.Execute("DELETE FROM Tools");
        context.Commit();
    }

Non transactional DataContext.

    using (var context = new DataContext(Transaction.No))
    {
        context.Execute("DELETE FROM Cars");
        context.Execute("DELETE FROM Tools");
    }

Using another database (connection string).

    new DataContext("ConnectionStringName", Transaction.Begin)

Get a value from database.

    var query = "SELECT COUNT(Id) FROM Tools";
    object toolsCount = context.GetValue(query);

Or typed...

    int toolsCount = context.GetValue<int>(query);

Or get many values...

    var query = "SELECT Name FROM Tools";
    var toolsName = context.GetValues<string>(query);

List of objects from database.

    var cars = context.All<Car>("SELECT * FROM Cars");

### Parameters Binding

Properties binding.

    var car = new Car { Name = "Esprit Turbo", ModelYear = 1981 };
    var command = "INSERT INTO Car VALUES (@Name, @ModelYear)";
    context.Execute(command, car);

Select the cars of the future.

    var query = "SELECT * FROM Car WHERE ModelYear > @Year";
    car futureCars = context.All<Car>(query, DateTime.Today);

You can bind a Dictionary<string, object> too.

### DataObjectCommand

Creating a object command.

    var command = new DataObjectCommand<Car>();

With property.

    public DataObjectCommand<Car> Command
    {
        get { return new DataObjectCommand<Car>(); }
    }

Insert.

    var car = new Car { Name = "Esprit Turbo", ModelYear = 1981 };
    command.Insert(car);

Insert binds the generated primary key.

    // car.Id == 0
    command.Insert(car);
    // car.Id > 0

Transactional DataObjectCommand.

    Command.Insert(car, context);

    (...)

    context.Commit();

Update and Delete have the same behavior.

    car.Name = "Esprit S3";
    Command.Update(car);

    Command.Delete(car);

### DataObjectQuery

Creating a object query.

    var select = new DataObjectQuery<Car>();

Or...

    new DataObjectQuery<Car>(table: "TB_CARS");

    new DataObjectQuery<Car>(primaryKey: "IdCar");

    new DataObjectQuery<Car>(table: "TB_CARS", primaryKey: "IdCar");

As a property.

    public DataObjectQuery<Car> Select
    {
        get { return new DataObjectQuery<Car>(); }
    }

    var allCars = Select.All();
    var lotusCars = Select.All("WHERE Name Like '%Lotus%'");
    var newerCar = Select.First("ORDER BY ModelYear DESC");

Parameters binding.

    anyObject.CarName = "Lotus Esprit Turbo";

    var cars = Select.All("WHERE Name = @CarName", anyObject);

Transactional DataObjectQuery.

    Select.With(context).First("ORDER BY ModelYear DESC");

### Custom DataObjects

> If TB_CARS is the name of the table.

Custom DataObjectCommand.

    new DataObjectCommand<Car>("TB_CAR");

Custom DataObjectQuery.

    new DataObjectQuery<Car>("Name, ModelYear FROM TB_CAR");

Projection with the default fields.

    new DataObjectQuery<Car>("{0} FROM TB_CAR");

### More

Using a custom provider.

    ProviderFactory.CustomProvider = (providerName) => new MyProvider();

Changing default connection string name.

    DataContext.DefaultConnectionStringName = "OtherDatabase";