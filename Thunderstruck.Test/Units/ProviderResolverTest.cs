using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thunderstruck.Runtime;
using FluentAssertions;
using Thunderstruck.Provider.Common;
using Moq;
using Thunderstruck.Provider;

namespace Thunderstruck.Test.Units
{
    [TestClass]
    public class ProviderResolverTest
    {
        [TestMethod]
        public void ProviderResolver_Should_Solve_Sql_Server_Provider()
        {
            var provider = ProviderResolver.Get("System.Data.SqlClient");

            provider.Should().BeOfType<SqlProvider>();
        }

        [TestMethod]
        public void ProviderResolver_Should_Solve_Oracle_Provider()
        {
            var provider = ProviderResolver.Get("System.Data.OracleClient");

            provider.Should().BeOfType<OracleProvider>();
        }

        [TestMethod]
        public void ProviderResolver_Should_Solve_MySql_Provider()
        {
            var provider = ProviderResolver.Get("MySql.Data.MySqlClient");

            provider.Should().BeOfType<MySqlProvider>();
        }

        [TestMethod]
        [ExpectedException(typeof(ThunderException), "Thunderstruck do not supports the Invalid.Provider provider. Try create and set a ProviderResolver.CustomProvider, it is easy.")]
        public void ProviderResolver_Should_Throw_Exception_When_Not_Found_A_Valid_Provider()
        {
            ProviderResolver.Get("Invalid.Provider");
        }

        [TestMethod]
        public void ProviderResolver_Should_Throw_Exception_With_An_Informative_Message_When_Not_Found_A_Valid_Provider()
        {
            try
            {
                ProviderResolver.Get("Invalid.Provider");
            }
            catch (Exception error)
            {
                error.Message.Should().Contain("Invalid.Provider");
            }

            try
            {
                ProviderResolver.Get("Another.Crazy.Provider");
            }
            catch (Exception error)
            {
                error.Message.Should().Contain("Another.Crazy.Provider");
            }
        }

        [TestMethod]
        public void ProviderResolver_Should_Solve_A_Custom_Provider()
        {
            var providerMock = new Mock<IDataProvider>();
            ProviderResolver.CustomProvider = providerMock.Object;
            
            var provider = ProviderResolver.Get("Any.Provider");

            provider.Should().Be(providerMock.Object);
        }
    }
}
