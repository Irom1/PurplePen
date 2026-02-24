/* ========================================================================================================
 * ASSEMBLY RESOLUTION WORKAROUND FOR .NET FRAMEWORK 4.8 + .NET STANDARD 2.0
 * ========================================================================================================
 * * WHY THIS IS NECESSARY:
 * This project targets .NET Framework 4.8, but it consumes shared libraries targeting .NET Standard 2.0. 
 * Those shared libraries depend on the `netstandard2.0` contract of `Microsoft.Extensions.Logging.Abstractions`.
 * * However, when NuGet restores packages for this .NET 4.8 test project, its internal fallback rules decide 
 * that the `net462` implementation of the Logging package is a "closer match" for .NET 4.8 than the 
 * `netstandard2.0` version. It places the `net462` DLL in the bin folder. 
 * * At runtime, the .NET Standard library demands the exact assembly signature it was compiled against. 
 * The CLR sees the `net462` DLL, notices the manifest/version mismatch, and throws a `FileLoadException`.
 * * WHY APP.CONFIG DOESN'T WORK HERE:
 * Normally, an `<assemblyBinding>` redirect in `app.config` forces the runtime to accept the physical DLL. 
 * However, because this is a test project, the test runner (e.g., MSTest or NUnit via testhost.exe) spins 
 * up its own isolated AppDomain to execute the tests. This test host frequently ignores, mangles, or fails 
 * to load the local `app.config` binding redirects. 
 * * HOW THIS FIXES IT:
 * We wire up an event handler to `AppDomain.CurrentDomain.AssemblyResolve`. This event fires exactly when 
 * the .NET runtime throws its hands up and says "I can't find this exact assembly version." 
 * * When it asks for *any* version of `Microsoft.Extensions.Logging.Abstractions`, we intercept the request 
 * and hand it the assembly containing `typeof(ILogger)`. This forces the runtime to accept the `net462` 
 * DLL that NuGet already loaded into memory, satisfying the .NET Standard library's requirement and 
 * bypassing the strict manifest check.
 * * FUTURE CLEANUP:
 * This entire file/method is a legacy workaround for the .NET Framework 4.8 assembly binder. 
 * Once this test project and its dependencies are fully migrated to .NET 10, the modern CoreCLR 
 * handles transitive dependencies and roll-forwards natively. 
 * * -> THIS CODE CAN BE SAFELY DELETED ONCE THE TARGET FRAMEWORK IS UPGRADED TO .NET 10. <-
 * ======================================================================================================== */

using System;
using System.Reflection;
using NUnit.Framework;

[SetUpFixture]
public class AssemblyResolutionSetup
{
    [OneTimeSetUp]
    public void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
            if (args.Name.StartsWith("Microsoft.Extensions.Logging.Abstractions", StringComparison.OrdinalIgnoreCase)) {
                return typeof(Microsoft.Extensions.Logging.ILogger).Assembly;
            }
            return null;
        };
    }
}