using Content.Shared.SS220.Photocopier.Forms.FormManagerShared;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.Photocopier;

[Serializable, NetSerializable]
public enum PhotocopierUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class PhotocopierUiState : BoundUserInterfaceState
{
    public bool IsPaperInserted { get; }
    public float TonerRemaining { get; }
    public int PrintQueueLength { get; }
    public bool IsSlotLocked { get; }
    public HashSet<string> AvailableFormCollections { get; }

    public PhotocopierUiState(
        bool isSlotLocked,
        bool isPaperInserted,
        float tonerRemaining,
        int printQueueLength ,
        HashSet<string> availableFormCollections)
    {
        IsSlotLocked = isSlotLocked;
        IsPaperInserted = isPaperInserted;
        TonerRemaining = tonerRemaining;
        PrintQueueLength = printQueueLength;
        AvailableFormCollections = availableFormCollections;
    }
}

[Serializable, NetSerializable]
public sealed class PhotocopierPrintMessage : BoundUserInterfaceMessage
{
    public int Amount { get; }
    public FormDescriptor Descriptor { get; }

    public PhotocopierPrintMessage(int amount, FormDescriptor descriptor)
    {
        Amount = amount;
        Descriptor = descriptor;
    }
}

[Serializable, NetSerializable]
public sealed class PhotocopierCopyMessage : BoundUserInterfaceMessage
{
    public int Amount { get; }

    public PhotocopierCopyMessage(int amount)
    {
        Amount = amount;
    }
}

[Serializable, NetSerializable]
public sealed class PhotocopierStopMessage : BoundUserInterfaceMessage
{
}
