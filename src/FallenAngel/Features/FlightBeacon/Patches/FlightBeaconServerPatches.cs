using Gantry.Services.HarmonyPatches.Annotations;
using JetBrains.Annotations;
using Vintagestory.API.Common;

// ReSharper disable UnusedType.Global// 
// ReSharper disable InconsistentNaming

namespace ApacheTech.VintageMods.FallenAngel.Features.FlightBeacon.Patches;

/// <summary>
///     Harmony Patches for the <see cref="FlightBeacon"/> Feature. This class cannot be inherited.
/// </summary>
[HarmonySidedPatch(EnumAppSide.Server)]
[UsedImplicitly(ImplicitUseTargetFlags.All)]
public sealed partial class FlightBeaconServerPatches;