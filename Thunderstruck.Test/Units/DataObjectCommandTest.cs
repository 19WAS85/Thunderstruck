using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using Moq;
using Thunderstruck.Provider;
using FluentAssertions;
using Thunderstruck.Provider.Common;

namespace Thunderstruck.Test.Units
{
    [TestClass]
    public class DataObjectCommandTest
    {
        private DataObjectCommand<Car> command;
        private Car target;

        private Mock<IDbConnection> connectionMock;
        private Mock<IDbCommand> commandMock;
        private Mock<IDbTransaction> transactionMock;
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
            ProviderFactory.CustomProvider = (provider) => new SqlProvider();

            parameterMock = new Mock<IDbDataParameter>();
            parameterCollectionMock = new Mock<IDataParameterCollection>();
            commandMock.Setup(c => c.CreateParameter()).Returns(parameterMock.Object);
            commandMock.Setup(c => c.Parameters).Returns(parameterCollectionMock.Object);
            commandMock.Setup(c => c.CommandText).Returns(String.Empty);

            target = new Car { Name = "Esprit Turbo", ModelYear = 1981 };
            command = new DataObjectCommand<Car>();
        }

        [TestMethod]
        public void DataObjectCommand_Should_Insert_A_New_Object_In_Database()
        {
            commandMock.Setup(c => c.CommandText).Returns("INSERT INTO Car ([Name], [ModelYear]) VALUES (@Name, @ModelYear); SELECT SCOPE_IDENTITY()");

            command.Insert(target);

            connectionMock.Verify(c => c.CreateCommand(), Times.Once());
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Never());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteScalar(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "INSERT INTO Car ([Name], [ModelYear]) VALUES (@Name, @ModelYear); SELECT SCOPE_IDENTITY()");
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            commandMock.Verify(c => c.CreateParameter(), Times.Exactly(2));
            transactionMock.Verify(t => t.Commit(), Times.Never());
        }

        [TestMethod]
        public void DataObjectCommand_Should_Insert_A_New_Object_And_Binds_Primary_Key_Property()
        {
            commandMock.Setup(c => c.ExecuteScalar()).Returns(318);

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
            commandMock.Setup(c => c.CommandText).Returns("UPDATE Car SET Name = @Name, ModelYear = @ModelYear WHERE Id = @Id");
            
            command.Update(target);

            connectionMock.Verify(c => c.CreateCommand(), Times.Once());
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Never());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteNonQuery(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "UPDATE Car SET Name = @Name, ModelYear = @ModelYear WHERE Id = @Id");
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            commandMock.Verify(c => c.CreateParameter(), Times.Exactly(3));
            transactionMock.Verify(t => t.Commit(), Times.Never());
        }

        [TestMethod]
        public void DataObjectCommand_Should_Delete_A_Item_On_Database()
        {
            commandMock.Setup(c => c.CommandText).Returns("DELETE FROM Car Where Id = @Id");

            command.Delete(target);

            connectionMock.Verify(c => c.CreateCommand(), Times.Once());
            connectionMock.Verify(c => c.Open(), Times.Once());
            connectionMock.Verify(c => c.BeginTransaction(), Times.Never());
            connectionMock.Verify(c => c.Close(), Times.Once());
            commandMock.Verify(c => c.ExecuteNonQuery(), Times.Once());
            commandMock.VerifySet(c => c.CommandText = "DELETE FROM Car Where Id = @Id");
            commandMock.VerifySet(c => c.Connection = connectionMock.Object);
            commandMock.Verify(c => c.CreateParameter(), Times.Exactly(1));
            transactionMock.Verify(t => t.Commit(), Times.Never());
        }

        [TestCleanup]
        public void CleanUp()
        {
            ProviderFactory.CustomProvider = null;
            ProviderFactory.ConnectionFactory = null;
        }
    }

    class Car
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ModelYear { get; set; }
    }
}
