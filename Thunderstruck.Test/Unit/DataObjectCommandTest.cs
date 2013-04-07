using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using Moq;
using Thunderstruck.Provider;
using FluentAssertions;
using Thunderstruck.Provider.Common;
using Thunderstruck.Runtime;

namespace Thunderstruck.Test.Unit
{
    [TestClass]
    public class DataObjectCommandTest
    {
        private Airplane target;
        private Mock<IDbConnection> connectionMock;
        private Mock<IDbCommand> commandMock;
        private Mock<IDbTransaction> transactionMock;
        private Mock<IDbDataParameter> parameterMock;
        private Mock<IDataParameterCollection> parameterCollectionMock;
        private DataObjectCommand<Airplane> command;
        private DataObjectCommand<AnotherAirplaneClass> anotherCommand;
        private AnotherAirplaneClass anotherTarget;

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

            parameterMock = new Mock<IDbDataParameter>();
            parameterCollectionMock = new Mock<IDataParameterCollection>();
            commandMock.Setup(c => c.CreateParameter()).Returns(parameterMock.Object);
            commandMock.Setup(c => c.Parameters).Returns(parameterCollectionMock.Object);
            commandMock.Setup(c => c.CommandText).Returns(String.Empty);

            target = new Airplane { Name = "Fokker Dr.I", FirstFlight = 1917 };
            anotherTarget = new AnotherAirplaneClass { Name = "Fokker Dr.I", FirstFlight = 1917 };
            command = new DataObjectCommand<Airplane>();
            anotherCommand = new DataObjectCommand<AnotherAirplaneClass>(table: "AirplanesTable", primaryKey: "AirplaneCode");
        }

        [TestMethod]
        public void DataObjectCommand_Should_Insert_A_New_Object_In_Database()
        {
            commandMock.Setup(c => c.CommandText).Returns("INSERT INTO Airplane ([Name], [FirstFlight]) VALUES (@Name, @FirstFlight); SELECT SCOPE_IDENTITY()");

            command.Insert(target);

            connectionMock.Verify(c => c.CreateCommand(), Times.Once());
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Never());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteScalar(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "INSERT INTO Airplane ([Name], [FirstFlight]) VALUES (@Name, @FirstFlight); SELECT SCOPE_IDENTITY()");
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            commandMock.Verify(c => c.CreateParameter(), Times.Exactly(2));
            transactionMock.Verify(t => t.Commit(), Times.Never());
        }

        [TestMethod]
        public void DataObjectCommand_Should_Insert_A_New_Object_And_Binds_Primary_Key_Property()
        {
            commandMock.Setup(c => c.ExecuteScalar()).Returns(318);

            target.Id.Should().Be(0);

            command.Insert(target);

            target.Id.Should().Be(318);
        }

        [TestMethod]
        public void DataObjectCommand_Should_Receive_A_Context_To_Make_Transactional_Commands()
        {
            using (var context = new DataContext())
            {
                command.Insert(target, context);

                // Simulate connection open state later the first execute.
                connectionMock.Setup(c => c.State).Returns(ConnectionState.Open);

                command.Insert(target, context);

                context.Commit();
            }

            connectionMock.Verify(c => c.CreateCommand(), Times.Exactly(2));
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Once());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteScalar(), Times.Exactly(2));
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            transactionMock.Verify(t => t.Commit(), Times.Once());
        }

        [TestMethod]
        public void DataObjectCommand_Should_Update_A_Item_On_Database()
        {
            commandMock.Setup(c => c.CommandText).Returns("UPDATE Airplane SET Name = @Name, FirstFlight = @FirstFlight WHERE Id = @Id");
            
            command.Update(target);

            connectionMock.Verify(c => c.CreateCommand(), Times.Once());
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Never());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteNonQuery(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "UPDATE Airplane SET Name = @Name, FirstFlight = @FirstFlight WHERE Id = @Id");
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            commandMock.Verify(c => c.CreateParameter(), Times.Exactly(3));
            transactionMock.Verify(t => t.Commit(), Times.Never());
        }

        [TestMethod]
        public void DataObjectCommand_Should_Delete_A_Item_On_Database()
        {
            commandMock.Setup(c => c.CommandText).Returns("DELETE FROM Airplane Where Id = @Id");

            command.Delete(target);

            connectionMock.Verify(c => c.CreateCommand(), Times.Once());
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Never());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteNonQuery(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "DELETE FROM Airplane Where Id = @Id");
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            commandMock.Verify(c => c.CreateParameter(), Times.Exactly(1));
            transactionMock.Verify(t => t.Commit(), Times.Never());
        }

        [TestMethod]
        public void DataObjectCommand_Should_Provides_Table_Name_And_Primary_Key_Configuration_To_Insert()
        {
            commandMock.Setup(c => c.CommandText).Returns(String.Empty);
            commandMock.Setup(c => c.ExecuteScalar()).Returns(318);

            anotherTarget.AirplaneCode.Should().Be(0);

            anotherCommand.Insert(anotherTarget);

            anotherTarget.AirplaneCode.Should().Be(318);
            commandMock.VerifySet(c => c.CommandText = "INSERT INTO AirplanesTable ([Name], [FirstFlight]) VALUES (@Name, @FirstFlight); SELECT SCOPE_IDENTITY()");
        }

        [TestMethod]
        public void DataObjectCommand_Should_Provides_Table_Name_And_Primary_Key_Configuration_To_Update()
        {
            commandMock.Setup(c => c.CommandText).Returns(String.Empty);

            anotherCommand.Update(anotherTarget);
            
            commandMock.VerifySet(c => c.CommandText = "UPDATE AirplanesTable SET Name = @Name, FirstFlight = @FirstFlight WHERE AirplaneCode = @AirplaneCode");
        }

        [TestMethod]
        public void DataObjectCommand_Should_Provides_Table_Name_And_Primary_Key_Configuration_To_Delete()
        {
            commandMock.Setup(c => c.CommandText).Returns(String.Empty);

            anotherCommand.Delete(anotherTarget);
            
            commandMock.VerifySet(c => c.CommandText = "DELETE FROM AirplanesTable Where AirplaneCode = @AirplaneCode");
        }

        [TestCleanup]
        public void CleanUp()
        {
            ProviderFactory.CustomProvider = null;
            ProviderFactory.ConnectionFactory = null;
        }
    }

    class Airplane
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int FirstFlight { get; set; }
    }

    class AnotherAirplaneClass
    {
        public int AirplaneCode { get; set; }

        public string Name { get; set; }

        public int FirstFlight { get; set; }
    }
}
