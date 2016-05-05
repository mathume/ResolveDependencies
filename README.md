Sample code of how to load into new AppDomain resolving dependencies.

- AssemblyWithDependency implements Interface from Dependency and JsonConverter from nuget package.
- LoadAssemblyIntoDifferentAppDomain has a test loading AssemblyWithDependency.dll and obtains Subclass by filtering over Dependency.Interface.

It seemed important to not only set the ApplicationBase but also add the current directory (where the ResolveHandler dll lies) to the AppDomain PrivateBinPath.