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
    public int PrintQueueLength { get; }
    public bool IsSlotLocked { get; }
    public HashSet<string> AvailableFormCollections { get; }
    public int TonerAvailable { get; }
    public int TonerCapacity { get; }
    public bool AssIsOnScanner { get; }
    public int MaxQueueLength { get; }

    public PhotocopierUiState(
        bool isSlotLocked,
        bool isPaperInserted,
        int printQueueLength ,
        HashSet<string> availableFormCollections,
        int tonerAvailable,
        int tonerCapacity,
        bool assIsOnScanner,
        int maxQueueLength)
    {
        IsSlotLocked = isSlotLocked;
        IsPaperInserted = isPaperInserted;
        PrintQueueLength = printQueueLength;
        AvailableFormCollections = availableFormCollections;
        TonerAvailable = tonerAvailable;
        TonerCapacity = tonerCapacity;
        AssIsOnScanner = assIsOnScanner;
        MaxQueueLength = maxQueueLength;
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

[Serializable, NetSerializable]
public sealed class PhotocopierRefreshUiMessage : BoundUserInterfaceMessage
{
}
