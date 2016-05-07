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
    public class LoadAssemblyWithDependency
    {
        private const string assemblyPath = "../../AssemblyWithDependency/bin/AssemblyWithDependency.dll";
        private List<string> foundClassNames;
        private AppDomain tempDomain;
        private FileInfo fileInfo;
        private Assembly ass;
        private string handlerDllPath;

        [SetUp]
        public void SetUp()
        {
            this.foundClassNames = new List<string>();
        }

        [TearDown]
        public void TearDown()
        {
            if (tempDomain != null)
            {
                AppDomain.Unload(this.tempDomain);
            }

            RemoveSymbolicLinkIfPathGiven(this.handlerDllPath, SymbolicLink.Directory);
            RemoveSymbolicLinkIfPathGiven(this.dependencyPath, SymbolicLink.Directory);
            
        }

        static void RemoveSymbolicLinkIfPathGiven(string path, SymbolicLink type)
        {
            if(string.IsNullOrEmpty(path)) { }
            else LinkHelper.RemoveSymbolicLinkFrom(path, type);
        }

        [Test]
        public void LoadAssemblyIntoTemporaryDomainTest()
        {
            this.fileInfo = new FileInfo(assemblyPath);
            this.LoadAssemblyIntoTemporaryAppDomain();
            this.GetSubclassNames();
            Assert.That(this.foundClassNames.Count(), Is.EqualTo(1));
            Assert.That(this.foundClassNames.First(), Is.EqualTo("Subclass"));
        }

        [Test]
        public void LoadAssemblyIntoCurrentDomainTest()
        {
            this.fileInfo = new FileInfo(assemblyPath);
            this.LoadAssemblyIntoCurrentAppDomain();
            this.GetSubclassNames();
            Assert.That(this.foundClassNames.Count(), Is.EqualTo(1));
            Assert.That(this.foundClassNames.First(), Is.EqualTo("Subclass"));
        }

        private void LoadAssemblyIntoCurrentAppDomain()
        {
            Loader.DirectoryName = this.fileInfo.DirectoryName;

            SetupResolverHandlerOnCurrentDomain();

            this.ass = this.LoadAssemblyIntoCurrentDomain();
        }

        private void LoadAssemblyIntoTemporaryAppDomain()
        {
            Loader.DirectoryName = this.fileInfo.DirectoryName;

            this.CreateTemporaryDomain();
            
            SetupResolveHandlerOnTemporaryDomain();
            //SetupResolverHandlerOnCurrentDomain();
            
            this.ass = this.LoadIntoTemporaryDomain();
        }

        private void GetSubclassNames()
        {
            var types = ass.GetTypes();

            this.foundClassNames = types.Where(
                t => t.IsClass && typeof(Interface).IsAssignableFrom(t)).Select(t => t.Name).ToList();
        }

        private Assembly LoadAssemblyIntoCurrentDomain()
        {
            var ass = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(this.fileInfo.FullName));
            return ass;
        }

        private void SetupResolverHandlerOnCurrentDomain()
        {
            this.dependencyPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "dep");
            LinkHelper.CreateSymbolicLinkFromTo(this.dependencyPath, this.fileInfo.DirectoryName, SymbolicLink.Directory);
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = "dep";
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Loader.LoadDependency);
            AppDomain.CurrentDomain.TypeResolve += new ResolveEventHandler(Loader.LoadDependency);
        }

        private Assembly LoadIntoTemporaryDomain()
        {
            var ass = tempDomain.Load(AssemblyName.GetAssemblyName(this.fileInfo.FullName));
            return ass;
        }

        private void CreateTemporaryDomain()
        {
            AppDomainSetup domainInfo = new AppDomainSetup();
            var subDir = "Handlers";
            this.handlerDllPath = Path.Combine(this.fileInfo.DirectoryName, subDir);
            LinkHelper.CreateSymbolicLinkFromTo(handlerDllPath, AppDomain.CurrentDomain.SetupInformation.ApplicationBase, SymbolicLink.Directory);
            domainInfo.ApplicationBase = this.fileInfo.DirectoryName;
            domainInfo.PrivateBinPath = subDir;
            domainInfo.PrivateBinPathProbe = string.Empty; // include application base for assembly search
            this.tempDomain = AppDomain.CreateDomain("tempDomain", AppDomain.CurrentDomain.Evidence, domainInfo);
        }

        private void SetupResolveHandlerOnTemporaryDomain()
        {
            tempDomain.AssemblyResolve += new ResolveEventHandler(Loader.LoadDependency);
            tempDomain.TypeResolve += new ResolveEventHandler(Loader.LoadDependency);
        }

        public string dependencyPath { get; set; }
    }
}
