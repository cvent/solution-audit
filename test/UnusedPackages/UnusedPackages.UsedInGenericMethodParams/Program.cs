using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace UnusedPackages.UsedInGenericMethodParams
{
    static class Program
    {
        static void Main(string[] args)
        {
        }

        static void TestMethod(IEnumerable<IEnumerable<IEnumerable<IPackage>>> listOfPackageThings)
        {
            //Do nothing
        }
    }
}
