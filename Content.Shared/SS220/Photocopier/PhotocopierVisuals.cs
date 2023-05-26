using Robust.Shared.Serialization;

namespace Content.Shared.SS220.Photocopier;

[Serializable, NetSerializable]
public enum PhotocopierVisuals : byte
{
    VisualState,
}

[Serializable, NetSerializable]
public enum PhotocopierVisualLayers : byte
{
    Base,
    Led,
    Top,
    TopPaper,
    PrintAnim
}

[Serializable, NetSerializable]
public enum PhotocopierVisualState : byte
{
    Off,
    Powered,
    Printing,
    Copying,
    OutOfToner
}

[Serializable, NetSerializable]
public sealed class PhotocopierCombinedVisualState : ICloneable
{
    public PhotocopierVisualState State { get; }
    public bool GotItem { get; }

    public PhotocopierCombinedVisualState(PhotocopierVisualState state, bool gotItem)
    {
        State = state;
        GotItem = gotItem;
    }

    public object Clone() => MemberwiseClone();
}
