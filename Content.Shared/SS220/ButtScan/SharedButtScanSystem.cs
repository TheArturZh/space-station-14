using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.ButtScan;

/// <summary>
/// This handles butt scan replication
/// </summary>
public sealed class SharedButtScanSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ButtScanComponent, ComponentHandleState>(HandleState);
        SubscribeLocalEvent<ButtScanComponent, ComponentGetState>(GetState);
    }

    private static void HandleState(EntityUid uid, ButtScanComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not ButtScanComponentState state)
            return;

        component.ButtTexturePath = state.ButtTexturePath;
    }

    private static void GetState(EntityUid uid, ButtScanComponent component, ref ComponentGetState args)
    {
        args.State = new ButtScanComponentState(component);
    }
}

[NetSerializable, Serializable]
public sealed class ButtScanComponentState : ComponentState
{
    public string ButtTexturePath;

    public ButtScanComponentState(ButtScanComponent component)
    {
        ButtTexturePath = component.ButtTexturePath;
    }
}
