using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;
using Thunderstruck.Provider;
using Thunderstruck.Provider.Common;
using Thunderstruck.Runtime;

namespace Thunderstruck.Test.Unit
{
    [TestClass]
    public class DataObjectQueryTest
    {
        private Mock<IDbConnection> connectionMock;
        private Mock<IDbCommand> commandMock;
        private Mock<IDbTransaction> transactionMock;
        private Mock<IDataReader> dataReaderMock;
        private Mock<IDbDataParameter> parameterMock;
        private Mock<IDataParameterCollection> parameterCollectionMock;
        private DataObjectQuery<Airplane> select;
        private DataObjectQuery<AnotherAirplaneClass> anotherSelect;

        [TestInitialize]
        public void Initialize()
        {
            commandMock = new Mock<IDbCommand>();
            connectionMock = new Mock<IDbConnection>();
            transactionMock = new Mock<IDbTransaction>();
            connectionMock.Setup(c => c.CreateCommand()).Returns(commandMock.Object);
            connectionMock.Setup(c => c.BeginTransaction()).Returns(transactionMock.Object);
            ProviderFactory.ConnectionFactory = (provider) => connectionMock.Object;
            ProviderFactory.CustomProvider = (provider) => new SqlProvider();

            dataReaderMock = new Mock<IDataReader>();
            commandMock.Setup(c => c.ExecuteReader()).Returns(dataReaderMock.Object);

            parameterMock = new Mock<IDbDataParameter>();
            parameterCollectionMock = new Mock<IDataParameterCollection>();
            commandMock.Setup(c => c.CreateParameter()).Returns(parameterMock.Object);
            commandMock.Setup(c => c.Parameters).Returns(parameterCollectionMock.Object);

            select = new DataObjectQuery<Airplane>();
            anotherSelect = new DataObjectQuery<AnotherAirplaneClass>("{0} FROM AirplanesTable");
        }

        [TestMethod]
        public void DataObjectQuery_Should_Select_All_Items_From_Database()
        {
            select.All();
            
            connectionMock.Verify(c => c.CreateCommand(), Times.Once());
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Never());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteReader(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "SELECT [Id], [Name], [FirstFlight] FROM Airplane");
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            commandMock.VerifySet(c => c.Transaction = transactionMock.Object, Times.Never());
            commandMock.Verify(c => c.CreateParameter(), Times.Never());
            transactionMock.Verify(t => t.Commit(), Times.Never());
        }

        [TestMethod]
        public void DataObjectQuery_Should_Select_All_Items_From_Database_With_Parameter_Binding_And_Context_Transaction()
        {
            commandMock.Setup(c => c.CommandText).Returns("SELECT [Id], [Name], [FirstFlight] FROM Airplane WHERE Name = @Name");

            using (var context = new DataContext())
            {
                // Execute a command to open transaction.
                context.Execute("DELETE FROM Airplane");

                // Simulate connection open state later the first execute.
                connectionMock.Setup(c => c.State).Returns(ConnectionState.Open);

                select.With(context).All("WHERE Name = @Name", new { Name = "Omega" });

                context.Commit();
            }

            connectionMock.Verify(c => c.CreateCommand(), Times.Exactly(2));
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Once());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteReader(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "SELECT [Id], [Name], [FirstFlight] FROM Airplane WHERE Name = @Name");
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            commandMock.VerifySet(c => c.Transaction = transactionMock.Object);
            commandMock.Verify(c => c.CreateParameter(), Times.Once());
            parameterMock.VerifySet(p => p.ParameterName = "@Name");
            parameterMock.VerifySet(p => p.Value = "Omega");
            parameterCollectionMock.Verify(p => p.Add(parameterMock.Object), Times.Once());
            transactionMock.Verify(t => t.Commit(), Times.Once());
        }

        [TestMethod]
        public void DataObjectQuery_Should_Select_All_Items_From_Database_With_Array_Parameter_Binding_And_Context_Transaction()
        {
            commandMock.Setup(c => c.CommandText).Returns("SELECT [Id], [Name], [FirstFlight] FROM Airplane WHERE Name = @0");

            using (var context = new DataContext())
            {
                // Execute a command to open transaction.
                context.Execute("DELETE FROM Airplane");

                // Simulate connection open state later the first execute.
                connectionMock.Setup(c => c.State).Returns(ConnectionState.Open);

                select.With(context).All("WHERE Name = @0", "Omega");

                context.Commit();
            }

            connectionMock.Verify(c => c.CreateCommand(), Times.Exactly(2));
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Once());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteReader(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "SELECT [Id], [Name], [FirstFlight] FROM Airplane WHERE Name = @0");
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            commandMock.VerifySet(c => c.Transaction = transactionMock.Object);
            commandMock.Verify(c => c.CreateParameter(), Times.Once());
            parameterMock.VerifySet(p => p.ParameterName = "@0");
            parameterMock.VerifySet(p => p.Value = "Omega");
            parameterCollectionMock.Verify(p => p.Add(parameterMock.Object), Times.Once());
            transactionMock.Verify(t => t.Commit(), Times.Once());
        }

        [TestMethod]
        public void DataObjectQuery_Should_Select_Limited_Items_From_Database()
        {
            select.Take(13, "ORDER BY Year DESC");

            commandMock.Verify(c => c.ExecuteReader(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "SELECT TOP 13 [Id], [Name], [FirstFlight] FROM Airplane ORDER BY Year DESC");
        }

        [TestMethod]
        public void DataObjectQuery_Should_Select_First_Item_From_Database()
        {
            select.First();

            commandMock.Verify(c => c.ExecuteReader(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "SELECT TOP 1 [Id], [Name], [FirstFlight] FROM Airplane");
        }

        [TestMethod]
        public void DataObjectQuery_Should_Select_Items_From_Database_With_Custom_Projection()
        {
            anotherSelect.All();

            commandMock.VerifySet(c => c.CommandText = "SELECT [AirplaneCode], [Name], [FirstFlight] FROM AirplanesTable");
        }

        [TestCleanup]
        public void CleanUp()
        {
            ProviderFactory.CustomProvider = null;
            ProviderFactory.ConnectionFactory = null;
        }
    }
}
