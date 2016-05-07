using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace LoadAssemblyIntoDifferentAppDomain
{
    [Serializable]
    class Loader : MarshalByRefObject
    {
        internal static string DirectoryName;

        internal static Assembly LoadDependency(object sender, ResolveEventArgs args)
        {
            Assembly ass = null;
            try
            {
                ass = Assembly.LoadFile(fileNameFromAssemblyName(args.Name));
            }
            catch { }

            return ass;
        }

        private static string fileNameFromAssemblyName(string p)
        {
            return Path.Combine(DirectoryName, p.Split(',')[0] + ".dll");
        }
    }
}
