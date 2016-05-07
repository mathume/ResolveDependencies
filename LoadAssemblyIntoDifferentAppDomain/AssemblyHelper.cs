using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Dependency;

namespace LoadAssemblyIntoDifferentAppDomain
{
    public class AssemblyHelper
    {
        public static void SetData(AppDomain appDomain, System.IO.FileInfo fileInfo)
        {
            appDomain.SetData("assemblyName", AssemblyName.GetAssemblyName(fileInfo.FullName));
            appDomain.SetData("loadFromDirectoryName", fileInfo.DirectoryName);
        }

        public static List<string> GetSubclassNames(AppDomain appDomain)
        {
            return (List<string>)appDomain.GetData("subclassNames");
        }

        public static int NumberOfLoadedAssembliesInTestDomain(AppDomain appDomain)
        {
            return (int)appDomain.GetData("numberOfLoadedAssemblies");
        }

        public static void SetSubclassNames()
        {
            Loader.LoadFromDirectoryName = (string)AppDomain.CurrentDomain.GetData("loadFromDirectoryName");
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Loader.LoadDependency);
            AppDomain.CurrentDomain.TypeResolve += new ResolveEventHandler(Loader.LoadDependency);


            var ass = AppDomain.CurrentDomain.Load(((AssemblyName)AppDomain.CurrentDomain.GetData("assemblyName")).FullName);
            var subclasses = ass.GetTypes().Where(
                t => t.IsClass && typeof(Interface).IsAssignableFrom(t)).Select(t => t.Name).ToList();
            AppDomain.CurrentDomain.SetData("subclassNames", subclasses);
            AppDomain.CurrentDomain.SetData("numberOfLoadedAssemblies", AppDomain.CurrentDomain.GetAssemblies().Count());
        }
    }
}
