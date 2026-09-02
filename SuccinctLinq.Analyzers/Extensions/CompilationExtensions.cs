using Microsoft.CodeAnalysis;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class CompilationExtensions
{
    extension(Compilation compilation)
    {
        public Version? TargetFrameworkVersion
        {
            get
            {
                // System.Environment is defined in System.Runtime on
                // .NET (5+), in mscorlib on .NET Framework, and in
                // netstandard on .NET Standard. The assembly version of
                // the containing assembly identifies the target
                // framework (e.g. 8.0.0.0 for .NET 8).
                return compilation.GetTypeByMetadataName("System.Environment")?
                    .ContainingAssembly?
                    .Identity.Version;
            }
        }

        public bool IsTargetFrameworkAtLeast(int major, int minor = 0) =>
            compilation.TargetFrameworkVersion is { } version && version >= new Version(major, minor);
    }
}
