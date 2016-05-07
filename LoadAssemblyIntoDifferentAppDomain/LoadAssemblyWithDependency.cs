using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using Dependency;
using System.Runtime.InteropServices;

namespace LoadAssemblyIntoDifferentAppDomain
{
    [TestFixture]
    [Serializable]
    public class LoadAssemblyWithDependency
    {
        private const string assemblyPath = "../../AssemblyWithDependency/bin/AssemblyWithDependency.dll";
        private AppDomain tempDomain;
        private FileInfo fileInfo;
        private Assembly ass;
        private string handlerDllPath;
        private AppDomain testDomain;

        [SetUp]
        public void SetUp()
        {
            this.fileInfo = new FileInfo(assemblyPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (tempDomain != null)
            {
                AppDomain.Unload(this.tempDomain);
            }

            RemoveSymbolicLinkIfPathGiven(this.handlerDllPath, SymbolicLink.Directory);
        }

        static void RemoveSymbolicLinkIfPathGiven(string path, SymbolicLink type)
        {
            if (string.IsNullOrEmpty(path)) { }
            else LinkHelper.RemoveSymbolicLinkFrom(path, type);
        }

        [Test]
        public void LoadAssemblyIntoTemporaryDomainTest()
        {
            this.InitTemporaryDomain();
            this.CreateLinkInTargetForResolver();
            this.testDomain = this.tempDomain;

            this.SetData();

            this.tempDomain.DoCallBack(new CrossAppDomainDelegate(SetSubclassNames));
            
            var subclassNames = this.GetSubclassNames();
            Assert.That(AppDomain.CurrentDomain.GetAssemblies().Count(), Is.EqualTo(12));
            Assert.That(NumberOfLoadedAssembliesInTestDomain(), Is.EqualTo(6));
            Assert.That(subclassNames, Is.Not.Null.Or.Empty);
            Assert.That(subclassNames.Count(), Is.EqualTo(1));
            Assert.That(subclassNames[0], Is.EqualTo("Subclass"));
        }

        private List<string> GetSubclassNames()
        {
            return (List<string>)this.testDomain.GetData("subclassNames");
        }

        private void SetData()
        {
            this.testDomain.SetData("assemblyName", AssemblyName.GetAssemblyName(this.fileInfo.FullName));
            this.testDomain.SetData("loadFromDirectoryName", this.fileInfo.DirectoryName);
        }

        [Test, Explicit]
        public void LoadAssemblyIntoCurrentDomainTest()
        {
            this.fileInfo = new FileInfo(assemblyPath);
            this.testDomain = AppDomain.CurrentDomain;
            this.SetData();

            SetSubclassNames();
            
            var subclassNames = (List<string>)AppDomain.CurrentDomain.GetData("subclassNames");
            Assert.That(AppDomain.CurrentDomain.GetAssemblies().Count(), Is.EqualTo(15));
            Assert.That(NumberOfLoadedAssembliesInTestDomain(), Is.EqualTo(15));
            Assert.That(subclassNames, Is.Not.Null.Or.Empty);
            Assert.That(subclassNames.Count(), Is.EqualTo(1));
            Assert.That(subclassNames[0], Is.EqualTo("Subclass"));

        }

        int NumberOfLoadedAssembliesInTestDomain()
        {
            return (int)this.testDomain.GetData("numberOfLoadedAssemblies");
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

        private void InitTemporaryDomain()
        {
            AppDomainSetup domainInfo = new AppDomainSetup();
            domainInfo.ApplicationBase = this.fileInfo.DirectoryName;
            domainInfo.PrivateBinPath = subDir;
            domainInfo.PrivateBinPathProbe = string.Empty; // include application base for assembly search
            this.tempDomain = AppDomain.CreateDomain("tempDomain", AppDomain.CurrentDomain.Evidence, domainInfo);
        }

        private const string subDir = "Handlers";

        private void CreateLinkInTargetForResolver()
        {
            this.handlerDllPath = Path.Combine(this.fileInfo.DirectoryName, subDir);
            LinkHelper.CreateSymbolicLinkFromTo(handlerDllPath, AppDomain.CurrentDomain.SetupInformation.ApplicationBase, SymbolicLink.Directory);
        }
    }
}
