using ApacheTech.Common.DependencyInjection.Abstractions.Extensions;
using ApacheTech.Common.Extensions.System;
using ApacheTech.VintageMods.FallenAngel.Features.FlightBeacon.Dialogue;
using ApacheTech.VintageMods.FallenAngel.Features.FlightBeacon.GameContent.BlockEntities;
using Gantry.Core;
using Gantry.Core.Annotation;
using Gantry.Core.GameContent.Extensions.Gui;
using Gantry.Core.Hosting;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

// ReSharper disable ClassNeverInstantiated.Global

namespace ApacheTech.VintageMods.FallenAngel.Features.FlightBeacon.GameContent.Blocks;

/// <summary>
///     Represents the template for a Flight Beacon block, within the game.
/// </summary>
/// <seealso cref="Block" />
public class BlockFlightBeacon : Block
{
    /// <summary>
    ///     Gets the particles to emit from the block, when it is enabled.
    /// </summary>
    public SimpleParticleProperties IdleParticles { get; private set; }

    /// <summary>
    ///     Should return the light HSV values.
    ///     Warning: This method is likely to get called in a background thread. Please make sure your code in here is thread safe.
    /// </summary>
    /// <param name="blockAccessor">The block accessor.</param>
    /// <param name="pos">May be null</param>
    /// <param name="stack">Set if its an item-stack for which the engine wants to check the light level</param>
    /// <returns>System.Byte[].</returns>
    public override byte[] GetLightHsv(IBlockAccessor blockAccessor, BlockPos pos, ItemStack stack = null)
    {
        if (pos is not null && blockAccessor.GetBlockEntity(pos) is BlockEntityFlightBeacon beacon)
        {
            return beacon.LightHsv;
        }
        return base.GetLightHsv(blockAccessor, pos, stack);
    }

    /// <summary>
    ///     Extra check on whether the ambient sound defined by the block should be played at this location
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="pos">The position.</param>
    public override bool ShouldPlayAmbientSound(IWorldAccessor world, BlockPos pos)
    {
        if (pos is null || world.BlockAccessor.GetBlockEntity(pos) is not BlockEntityFlightBeacon beacon) return false;
        Sounds.AmbientBlockCount = 100f - beacon.AmbientVolume;
        return beacon.Enabled;
    }

    /// <summary>
    ///     When a player does a right click while targeting this placed block.
    ///     Should return true if the event is handled, so that other events can occur, e.g. eating a held item if the block is not interact-able with.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="byPlayer">The by player.</param>
    /// <param name="blockSel">The block sel.</param>
    /// <returns>
    ///     False if the interaction should be stopped. True if the interaction should continue.
    ///     If you return false, the interaction will not be synced to the server.
    /// </returns>
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (world.Api.Side.IsServer()) return false;
        if (!byPlayer.Entity.Controls.Sneak) return false;
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BlockEntityFlightBeacon beacon) return false;

        var dialogue = IOC.Services.CreateInstance<FlightBeaconDialogue>(beacon);
        dialogue.OnOkAction = packet =>
        {
            ApiEx.Client.Network.GetChannel("FlightBeacon").SendPacket(packet);
        };
        dialogue.ToggleGui();
        return true;
    }

    /// <summary>
    ///     Called when the block is loaded by the game.
    /// </summary>
    /// <param name="coreApi">The core API.</param>
    public override void OnLoaded(ICoreAPI coreApi)
    {
        base.OnLoaded(coreApi);

        if (coreApi is ICoreServerAPI sapi)
        {
            sapi.Network.GetChannel("FlightBeacon")
                .SetMessageHandler<FlightBeaconPacket>(OnClientPacketReceived);
        }

        IdleParticles = new SimpleParticleProperties(
            minQuantity: 1f,
            maxQuantity: 5f,
            color: 0,
            minPos: new Vec3d(),
            maxPos: new Vec3d(),
            minVelocity: new Vec3f(-0.1f, -0.1f, -0.1f),
            maxVelocity: new Vec3f(0.1f, 0.3f, 0.1f),
            lifeLength: 1.0f,
            gravityEffect: 0f,
            minSize: 0.1f,
            maxSize: 0.75f)
        {
            SizeEvolve = EvolvingNatFloat.create(EnumTransformFunction.QUADRATIC, -0.6f)
        };
        IdleParticles.AddPos.Set(1.0, 2.0, 1.0);
        IdleParticles.addLifeLength = 0.5f;
    }

    /// <summary>
    ///     Called when a survival player has broken the block. This method needs to remove the block.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="pos">The position.</param>
    /// <param name="byPlayer">The by player.</param>
    /// <param name="dropQuantityMultiplier">The drop quantity multiplier.</param>
    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
    {
        if (api is ICoreServerAPI sapi)
        {
            var blockAccessor = sapi.World.BlockAccessor;
            if (blockAccessor.GetBlockEntity(pos) is not BlockEntityFlightBeacon beacon) return;
            if (!beacon.Pos.Equals(pos)) return;
            FlightBeaconSystem.UpdateBeaconCache(beacon, false);
        }
        base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
    }

    /// <summary>
    ///     Called when a <see cref="FlightBeaconPacket"/> packet is received from the client.
    /// </summary>
    /// <param name="fromPlayer">The player that sent the packet.</param>
    /// <param name="packet">The packet.</param>
    [RunsOn(EnumAppSide.Server)]
    private static void OnClientPacketReceived(IServerPlayer fromPlayer, FlightBeaconPacket packet)
    {
        var blockAccessor = ApiEx.ServerMain.GetBlockAccessorBulkUpdate(true, true);
        if (blockAccessor.GetBlockEntity(packet.Position) is not BlockEntityFlightBeacon beacon) return;
        if (!beacon.Pos.Equals(packet.Position)) return;

        beacon.Radius = packet.Radius;
        beacon.AmbientVolume = packet.AmbientVolume;
        beacon.Enabled = packet.Enabled;

        FlightBeaconSystem.UpdateBeaconCache(beacon, packet.Enabled);

        blockAccessor.RemoveBlockLight(beacon.LightHsv.With(p => p[2] = 31), beacon.Pos);
        blockAccessor.MarkBlockDirty(beacon.Pos);
        blockAccessor.MarkBlockEntityDirty(beacon.Pos);
        blockAccessor.Commit();
    }

    /// <summary>
    ///     Called by the block info HUD for display the interaction help besides the cross-hair.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="selection">The selection.</param>
    /// <param name="forPlayer">For player.</param>
    /// <returns>Vintagestory.API.Client.WorldInteraction[].</returns>
    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
    {
        if (world.BlockAccessor.GetBlockEntity(selection.Position) is not BlockEntityFlightBeacon)
            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);

        return new[]
        {
            new WorldInteraction
            {
                ActionLangCode = "fallenangel:Features.FlightBeacon.Interactions.OpenGUI",
                MouseButton = EnumMouseButton.Right,
                RequireFreeHand = true,
                HotKeyCode = "sneak"
            }
        }.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
    }
}