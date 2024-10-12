using ApacheTech.Common.DependencyInjection.Abstractions
using Gantry.Core.Hosting;
using Gantry.Services.FileSystem.Hosting;
using Gantry.Services.HarmonyPatches.Hosting;
using Gantry.Services.Network.Hosting;
using JetBrains.Annotations;
using Vintagestory.API.Common;

namespace ApacheTech.VintageMods.FallenAngel
{
    /// <summary>
    ///     Entry-point for the mod. This class will configure and build the IOC Container, and Service list for the rest of the mod.
    ///     
    ///     Registrations performed within this class should be global scope; by convention, features should aim to be as stand-alone as they can be.
    /// </summary>
    /// <remarks>
    ///     Only one derived instance of this class should be added to any single mod within
    ///     the VintageMods domain. This class will enable Dependency Injection, and add all
    ///     of the domain services. Derived instances should only have minimal functionality, 
    ///     instantiating, and adding Application specific services to the IOC Container.
    /// </remarks>
    /// <seealso cref="ModHost" />
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal sealed class Program : ModHost
    {
        protected override void ConfigureUniversalModServices(IServiceCollection services, ICoreAPI api)
        {
            services.AddFileSystemService(api, o => o.RegisterSettingsFiles = true);
            services.AddHarmonyPatchingService(api, o => o.AutoPatchModAssembly = true);
            services.AddNetworkService(api);
        }
    }
}
