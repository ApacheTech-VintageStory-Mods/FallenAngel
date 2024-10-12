using ProtoBuf;
using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.FallenAngel.Features.FlightBeacon;

/// <summary>
///     A DTO, containing meta data information, required to synchronise Flight Beacon block entities between the client, and the server.
/// </summary>
[ProtoContract]
public sealed class FlightBeaconPacket
{
    /// <summary>
    ///     Gets or sets a value indicating whether the Flight Beacon at the given BlockPos is enabled.
    /// </summary>
    /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
    [ProtoMember(1)]
    public bool Enabled { get; init; }

    /// <summary>
    ///     Gets or sets the radius at which the Flight Beacon is active.
    /// </summary>
    /// <value>An <see cref="int"/> value, determining the active radius of the beacon.</value>
    [ProtoMember(2)]
    public int Radius { get; init; }

    /// <summary>
    ///     Gets or sets the position within the gameworld, of this specific Flight Beacon.
    /// </summary>
    /// <value>An instance of <see cref="BlockPos"/>, giving an absolute position within the gameworld.</value>
    [ProtoMember(3)]
    public BlockPos Position { get; init; }

    /// <summary>
    ///     Gets or sets the volume at which the ambient sounds are played.
    /// </summary>
    /// <value>An <see cref="int"/> value, determining the volume of the ambient sounds.</value>
    [ProtoMember(4)]
    public int AmbientVolume { get; init; }
}