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
                ass = Assembly.LoadFile(fileName(args.Name));
            }
            catch { }

            return ass;
        }

        private static string fileName(string p)
        {
            if (p.EndsWith(".dll"))
            {
                return p.Replace("\\\\", "\\");
            }
            else
            {
                return Path.Combine(DirectoryName, p.Split(',')[0] + ".dll");
            }
        }
    }
}
