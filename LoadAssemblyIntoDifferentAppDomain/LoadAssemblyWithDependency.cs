using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using Dependency;

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
            SetupResolverHandlerOnCurrentDomain();
            
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
            var ass = AppDomain.CurrentDomain.Load(this.fileInfo.FullName);
            return ass;
        }

        private static void SetupResolverHandlerOnCurrentDomain()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Loader.LoadDependency);
            AppDomain.CurrentDomain.TypeResolve += new ResolveEventHandler(Loader.LoadDependency);
        }

        private Assembly LoadIntoTemporaryDomain()
        {
            var ass = tempDomain.Load(this.fileInfo.FullName);
            return ass;
        }

        private void CreateTemporaryDomain()
        {
            AppDomainSetup domainInfo = new AppDomainSetup();
            domainInfo.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            domainInfo.PrivateBinPath = this.fileInfo.DirectoryName + ";" + AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            this.tempDomain = AppDomain.CreateDomain("tempDomain", AppDomain.CurrentDomain.Evidence, domainInfo);
        }

        private void SetupResolveHandlerOnTemporaryDomain()
        {
            tempDomain.AssemblyResolve += new ResolveEventHandler(Loader.LoadDependency);
            tempDomain.TypeResolve += new ResolveEventHandler(Loader.LoadDependency);
        }
    }
}
