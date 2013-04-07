using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thunderstruck.Test.Functional
{
    public class SqlEnvironment : IDisposable
    {
        public SqlEnvironment()
        {
            CreateDatabase();
            CreateTables();
        }

        private void CreateDatabase()
        {
            using (var data = new DataContext("Master", Transaction.No))
            {
                data.Execute("CREATE DATABASE ThunderTest");
            }
        }

        private void CreateTables()
        {
            var createManufacturerTable = new StringBuilder("CREATE TABLE Le_Manufacturer (")
                .Append("[TheId] NUMERIC(8) NOT NULL IDENTITY PRIMARY KEY,")
                .Append("[Name] VARCHAR(32) NOT NULL,")
                .Append("[BuildYear] NUMERIC(4) NOT NULL)")
                .ToString();

            var createCarTable = new StringBuilder("CREATE TABLE Car (")
                .Append("[Id] NUMERIC(8) NOT NULL IDENTITY PRIMARY KEY,")
                .Append("[Name] VARCHAR(32),")
                .Append("[ModelYear] NUMERIC(4) NOT NULL,")
                .Append("[CreatedAt] DATETIME NOT NULL,")
                .Append("[Chassis] VARCHAR(32) NOT NULL,")
                .Append("[Mileage] FLOAT,")
                .Append("[Category] NUMERIC(1) NOT NULL,")
                .Append("[ManufacturerId] NUMERIC(8) REFERENCES Le_Manufacturer(TheId))")
                .ToString();

            using (var data = new DataContext())
            {
                data.Execute(createManufacturerTable);
                data.Execute(createCarTable);
                data.Commit();
            }
        }

        private void DropDatabase()
        {
            using (var data = new DataContext("Master", Transaction.No))
            {
                data.Execute("DROP DATABASE ThunderTest");
            }
        }

        public void Dispose()
        {
            DropDatabase();
        }
    }
}