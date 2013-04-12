using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thunderstruck.Runtime;
using FluentAssertions;
using Thunderstruck.Provider.Common;
using Moq;
using Thunderstruck.Provider;

namespace Thunderstruck.Test.Unit
{
    [TestClass]
    public class ProviderFactoryTest
    {
        private ProviderFactory factory;

        [TestInitialize]
        public void Initialize()
        {
            ProviderFactory.CustomProvider = null;
            ProviderFactory.ConnectionFactory = null;

            factory = new ProviderFactory();
        }

        [TestMethod]
        public void ProviderFactory_Should_Solve_Sql_Server_Provider()
        {
            var provider = factory.ResolveDataProvider("System.Data.SqlClient");

            provider.Should().BeOfType<SqlProvider>();
        }

        [TestMethod]
        public void ProviderFactory_Should_Solve_Oracle_Provider()
        {
            var provider = factory.ResolveDataProvider("System.Data.OracleClient");

            provider.Should().BeOfType<OracleProvider>();
        }

        [TestMethod]
        public void ProviderFactory_Should_Solve_MySql_Provider()
        {
            var provider = factory.ResolveDataProvider("MySql.Data.MySqlClient");

            provider.Should().BeOfType<MySqlProvider>();
        }

        [TestMethod]
        [ExpectedException(typeof(ThunderException))]
        public void ProviderFactory_Should_Throw_Exception_When_Not_Found_A_Valid_Provider()
        {
            factory.ResolveDataProvider("Invalid.Provider");
        }

        [TestMethod]
        public void ProviderFactory_Should_Throw_Exception_With_An_Informative_Message_When_Not_Found_A_Valid_Provider()
        {
            try
            {
                factory.ResolveDataProvider("Invalid.Provider");
            }
            catch (Exception error)
            {
                error.Message.Should().Contain("Invalid.Provider");
            }

            try
            {
                factory.ResolveDataProvider("Another.Crazy.Provider");
            }
            catch (Exception error)
            {
                error.Message.Should().Contain("Another.Crazy.Provider");
            }
        }

        [TestMethod]
        public void ProviderFactory_Should_Solve_A_Custom_Provider()
        {
            var providerMock = new Mock<IDataProvider>();
            ProviderFactory.CustomProvider = (providerName) => providerMock.Object;

            var provider = factory.ResolveDataProvider("Any.Provider");

            provider.Should().Be(providerMock.Object);
        }
    }
}
