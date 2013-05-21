using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Thunderstruck.Provider;
using Thunderstruck.Runtime;
using System.Data;
using FluentAssertions;

namespace Thunderstruck.Test.Unit
{
    [TestClass]
    public class DataContextTest
    {
        private Mock<IDbConnection> connectionMock;
        private Mock<IDbCommand> commandMock;
        private Mock<IDbTransaction> transactionMock;
        private Mock<DefaultProvider> providerMock;
        private Mock<IDataReader> dataReaderMock;
        private Mock<IDbDataParameter> parameterMock;
        private Mock<IDataParameterCollection> parameterCollectionMock;

        [TestInitialize]
        public void Initialize()
        {
            commandMock = new Mock<IDbCommand>();
            connectionMock = new Mock<IDbConnection>();
            transactionMock = new Mock<IDbTransaction>();
            connectionMock.Setup(c => c.CreateCommand()).Returns(commandMock.Object);
            connectionMock.Setup(c => c.BeginTransaction()).Returns(transactionMock.Object);
            ProviderFactory.ConnectionFactory = (provider) => connectionMock.Object;

            providerMock = new Mock<DefaultProvider>();
            ProviderFactory.CustomProvider = (provider) => providerMock.Object;

            dataReaderMock = new Mock<IDataReader>();
            commandMock.Setup(c => c.ExecuteReader()).Returns(dataReaderMock.Object);

            parameterMock = new Mock<IDbDataParameter>();
            parameterCollectionMock = new Mock<IDataParameterCollection>();
            commandMock.Setup(c => c.CreateParameter()).Returns(parameterMock.Object);
            commandMock.Setup(c => c.CommandText).Returns("INSERT INTO Airplane VALUES (@Name, @FirstFlight)");
            commandMock.Setup(c => c.Parameters).Returns(parameterCollectionMock.Object);
        }

        [TestMethod]
        public void DataContext_Should_Execute_And_Commit_A_SQL_Command_With_Transaction()
        {
            using (var context = new DataContext())
            {
                context.Execute("DELETE FROM Airplane");
                context.Commit();
            }

            connectionMock.Verify(c => c.CreateCommand(), Times.Once());
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Once());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteNonQuery(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "DELETE FROM Airplane");
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            commandMock.VerifySet(c => c.Transaction = transactionMock.Object);
            commandMock.Verify(c => c.CreateParameter(), Times.Never());
            transactionMock.Verify(t => t.Commit(), Times.Once());
            providerMock.Object.DbTransaction.Should().NotBeNull();
        }

        [TestMethod]
        public void DataContext_Should_Execute_And_Commit_Some_SQL_Commands_With_Transaction()
        {
            using (var context = new DataContext())
            {
                context.Execute("DELETE FROM Airplane");

                // Simulate connection open state later the first execute.
                connectionMock.Setup(c => c.State).Returns(ConnectionState.Open);

                context.Execute("DELETE FROM Tools");
                context.Commit();
            }

            connectionMock.Verify(c => c.CreateCommand(), Times.Exactly(2));
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Once());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteNonQuery(), Times.Exactly(2));
            commandMock.VerifySet(c => c.CommandText = "DELETE FROM Airplane");
            commandMock.VerifySet(c => c.CommandText = "DELETE FROM Tools");
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            commandMock.VerifySet(c => c.Transaction = transactionMock.Object);
            commandMock.Verify(c => c.CreateParameter(), Times.Never());
            transactionMock.Verify(t => t.Commit(), Times.Once());
            providerMock.Object.DbTransaction.Should().NotBeNull();
        }

        [TestMethod]
        public void DataContext_Should_Execute_And_Commit_Some_SQL_Commands_Without_Transaction()
        {
            using (var context = new DataContext(Transaction.No))
            {
                context.Execute("DELETE FROM Airplane");

                // After the first Execute, simulates the connection state to "open".
                connectionMock.Setup(c => c.State).Returns(ConnectionState.Open);

                context.Execute("DELETE FROM Tools");
            }

            connectionMock.Verify(c => c.CreateCommand(), Times.Exactly(2));
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Never());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteNonQuery(), Times.Exactly(2));
            commandMock.VerifySet(c => c.CommandText = "DELETE FROM Airplane");
            commandMock.VerifySet(c => c.CommandText = "DELETE FROM Tools");
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            commandMock.VerifySet(c => c.Transaction = transactionMock.Object, Times.Never());
            commandMock.Verify(c => c.CreateParameter(), Times.Never());
            transactionMock.Verify(t => t.Commit(), Times.Never());
            providerMock.Object.DbTransaction.Should().BeNull();
        }

        [TestMethod]
        [ExpectedException(typeof(ThunderException))]
        public void DataContext_Should_Throws_Exception_When_Connection_String_Not_Exists()
        {
            var context = new DataContext("InexistentConnection", Transaction.Begin);
        }

        [TestMethod]
        public void DataContext_Should_Be_Possible_To_Use_Another_Connection_String()
        {
            var context = new DataContext("AnotherDatabase", Transaction.Begin);

            context.ConnectionSettings.ConnectionString.Should().Be("whatever");
        }

        [TestMethod]
        public void DataContext_Should_Not_Create_Connection_If_Not_Use_Commands()
        {
            var context = new DataContext();

            connectionMock.Verify(c => c.CreateCommand(), Times.Never());
            connectionMock.Verify(c => c.Open(), Times.Never());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Never());
            connectionMock.Verify(c => c.Close(), Times.Never());
        }

        [TestMethod]
        public void DataContext_Should_Get_A_Value_From_Database()
        {
            using (var context = new DataContext())
            {
                var value = context.GetValue("SELECT COUNT(Id) FROM Airplane");
            }

            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.Close(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Never());
            commandMock.Verify(c => c.ExecuteScalar(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "SELECT COUNT(Id) FROM Airplane");
            providerMock.Object.DbTransaction.Should().BeNull();
        }

        [TestMethod]
        public void DataContext_Should_Get_A_Typed_Value_From_Database()
        {
            commandMock.Setup(c => c.ExecuteScalar()).Returns(1);

            using (var context = new DataContext())
            {
                var value = context.GetValue<int>("SELECT COUNT(Id) FROM Airplane");
            }

            commandMock.Verify(c => c.ExecuteScalar(), Times.Once());
        }

        [TestMethod]
        public void DataContext_Should_Get_A_List_Of_Values_From_Database()
        {
            using (var context = new DataContext())
            {
                var value = context.GetValues<string>("SELECT Name FROM Tools");
            }

            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.Close(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Never());
            commandMock.Verify(c => c.ExecuteReader(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "SELECT Name FROM Tools");
            providerMock.Object.DbTransaction.Should().BeNull();
            dataReaderMock.Verify(r => r.Read(), Times.Once());
        }

        [TestMethod]
        public void DataContext_Should_Get_A_List_Of_Objects_From_Database()
        {
            using (var context = new DataContext())
            {
                var cars = context.All<object>("SELECT * FROM Airplane");
            }

            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.Close(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Never());
            commandMock.Verify(c => c.ExecuteReader(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "SELECT * FROM Airplane");
            providerMock.Object.DbTransaction.Should().BeNull();
            dataReaderMock.Verify(r => r.Read(), Times.Once());
        }

        [TestMethod]
        public void DataContext_Should_Execute_Command_With_Anonymous_Object_Parameters()
        {
            using (var context = new DataContext())
            {
                var airplane = new { Name = "Fokker Dr.I", FirstFlight = (int?) null };
                var command = "INSERT INTO Airplane VALUES (@Name, @FirstFlight)";
                context.Execute(command, airplane);
            }

            commandMock.Verify(c => c.ExecuteNonQuery(), Times.Once());
            commandMock.Verify(c => c.CreateParameter(), Times.Exactly(2));
            parameterMock.VerifySet(p => p.ParameterName = "Name");
            parameterMock.VerifySet(p => p.ParameterName = "FirstFlight");
            parameterMock.VerifySet(p => p.Value = "Fokker Dr.I");
            parameterMock.VerifySet(p => p.Value = DBNull.Value);
            parameterCollectionMock.Verify(p => p.Add(parameterMock.Object), Times.Exactly(2));
        }

        [TestMethod]
        public void DataContext_Should_Execute_Command_With_Array_Parameters()
        {
            commandMock.Setup(c => c.CommandText).Returns("INSERT INTO Airplane VALUES (@0, @1)");

            using (var context = new DataContext())
            {
                var command = "INSERT INTO Airplane VALUES (@0, @1)";
                context.Execute(command, "Fokker Dr.I", null);
            }

            commandMock.Verify(c => c.ExecuteNonQuery(), Times.Once());
            commandMock.Verify(c => c.CreateParameter(), Times.Exactly(2));
            parameterMock.VerifySet(p => p.ParameterName = "0");
            parameterMock.VerifySet(p => p.ParameterName = "1");
            parameterMock.VerifySet(p => p.Value = "Fokker Dr.I");
            parameterMock.VerifySet(p => p.Value = DBNull.Value);
            parameterCollectionMock.Verify(p => p.Add(parameterMock.Object), Times.Exactly(2));
        }

        [TestMethod]
        public void DataContext_Should_Execute_Command_With_Length_One_Array_Parameters()
        {
            commandMock.Setup(c => c.CommandText).Returns("INSERT INTO Airplane VALUES (@0)");

            using (var context = new DataContext())
            {
                context.Execute("INSERT INTO Airplane VALUES (@0)", "Fokker Dr.I");
            }

            commandMock.Verify(c => c.ExecuteNonQuery(), Times.Once());
            commandMock.Verify(c => c.CreateParameter(), Times.Once());
            parameterMock.VerifySet(p => p.ParameterName = "0");
            parameterMock.VerifySet(p => p.Value = "Fokker Dr.I");
            parameterCollectionMock.Verify(p => p.Add(parameterMock.Object), Times.Once());
        }

        [TestCleanup]
        public void CleanUp()
        {
            ProviderFactory.CustomProvider = null;
            ProviderFactory.ConnectionFactory = null;
        }
    }
}
