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
            else RemoveSymbolicLink(path, type);
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
            CreateSymbolicLinkFromTo(this.dependencyPath, this.fileInfo.DirectoryName, SymbolicLink.Directory);
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
            CreateSymbolicLinkFromTo(handlerDllPath, AppDomain.CurrentDomain.SetupInformation.ApplicationBase, SymbolicLink.Directory);
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

        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        [DllImport("kernel32.dll")]
        static extern int GetLastError();

        static bool CreateSymbolicLinkFromTo(string linkName, string targetName, SymbolicLink type)
        {
            if(CreateSymbolicLink(linkName, targetName, type)) return true;

            var errorcode = GetLastError();
            var message = GetSystemMessage(errorcode);
            throw new UnauthorizedAccessException(message);
        }

        static bool RemoveSymbolicLink(string lpSymlinkFileName, SymbolicLink dwFlags)
        {
            switch (dwFlags)
            {
                case SymbolicLink.Directory:
                    try
                    {
                        Directory.Delete(lpSymlinkFileName);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                case SymbolicLink.File:
                    try
                    {
                        File.Delete(lpSymlinkFileName);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }

        static string GetSystemMessage(int errorCode)
        {
            int capacity = 512;
            int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
            StringBuilder sb = new StringBuilder(capacity);
            FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, errorCode, 0,
                sb, sb.Capacity, IntPtr.Zero);
            int i = sb.Length;
            if (i > 0 && sb[i - 1] == 10) i--;
            if (i > 0 && sb[i - 1] == 13) i--;
            sb.Length = i;
            return sb.ToString();
        }

        [DllImport("kernel32.dll")]
        static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId,
            int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr Arguments);

        enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        public string dependencyPath { get; set; }
    }
}
