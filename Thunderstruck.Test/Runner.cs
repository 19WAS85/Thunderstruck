using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shakedown;
using Thunderstruck.Test.Cenarios;

namespace Thunderstruck.Test
{
    class Runner
    {
        static void Main(string[] args)
        {
            using (new TestEnvironment())
            {
                new DataObjectsTest().Execute();
                new DataContextTest().Execute();
            }

            TestSumary.Current.Print();
        }
    }
}
