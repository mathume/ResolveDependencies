using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace LoadIntoDifferentDomainInSameDirectory
{
    [TestFixture]
    public class LoadFromSameFolder
    {
        private const string assemblyPath = @"AssemblyWithDependency.dll";
        private AppDomain testDomain;
        private System.IO.FileInfo fileInfo;

        [TearDown]
        public void TearDown()
        {
            if (this.testDomain != null)
            {
                AppDomain.Unload(this.testDomain);
            }
        }

        [SetUp]
        public void SetUp()
        {
            this.fileInfo = new FileInfo(assemblyPath);
        }

        [Test]
        public void LoadAssemblyIntoTemporaryDomainTest()
        {
            this.InitTemporaryDomain();
            
            AssemblyHelper.SetData(this.testDomain, this.fileInfo);

            this.testDomain.DoCallBack(new CrossAppDomainDelegate(AssemblyHelper.SetSubclassNames));

            var subclassNames = AssemblyHelper.GetSubclassNames(this.testDomain);
            Assert.That(AppDomain.CurrentDomain.GetAssemblies().Count(), Is.EqualTo(12));
            Assert.That(AssemblyHelper.NumberOfLoadedAssembliesInTestDomain(this.testDomain), Is.EqualTo(6));
            Assert.That(subclassNames, Is.Not.Null.Or.Empty);
            Assert.That(subclassNames.Count(), Is.EqualTo(1));
            Assert.That(subclassNames[0], Is.EqualTo("Subclass"));
        }

        [Test]
        public void LoadAssemblyIntoCurrentDomain()
        {
            this.InitCurrentDomain();

            AssemblyHelper.SetData(this.testDomain, this.fileInfo);

            AssemblyHelper.SetSubclassNames();

            var subclassNames = AssemblyHelper.GetSubclassNames(this.testDomain);
            Assert.That(AssemblyHelper.NumberOfLoadedAssembliesInTestDomain(this.testDomain), Is.EqualTo(16));
            Assert.That(subclassNames, Is.Not.Null.Or.Empty);
            Assert.That(subclassNames.Count(), Is.EqualTo(1));
            Assert.That(subclassNames[0], Is.EqualTo("Subclass"));
        
        }

        private void InitCurrentDomain()
        {
            this.testDomain = AppDomain.CurrentDomain;
        }

        private void InitTemporaryDomain()
        {
            AppDomainSetup domainInfo = new AppDomainSetup();
            domainInfo.ApplicationBase = this.fileInfo.DirectoryName;
            domainInfo.PrivateBinPath = "";
            domainInfo.PrivateBinPathProbe = string.Empty; // include application base for assembly search
            this.testDomain = AppDomain.CreateDomain("tempDomain", AppDomain.CurrentDomain.Evidence, domainInfo);
        }
    }
}
