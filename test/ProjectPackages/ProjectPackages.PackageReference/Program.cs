using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cvent.Framework.Localization.Extensions;

namespace ProjectPackages.PackageReference
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(GetResource(args[0]));
        }

        public static string GetResource(string arg)
        {
            //There is an overload of GetResourceString that uses Cvent.Framework.Shared
            var resource = ResourceExtensions.GetResourceString(arg, 1033);
            return resource;
        }
    }
}
