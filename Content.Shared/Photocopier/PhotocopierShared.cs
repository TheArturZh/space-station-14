using Robust.Shared.Serialization;

namespace Content.Shared.Photocopier;

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
    public bool IsSlotLocked { get;  }

    public PhotocopierUiState(
        bool isSlotLocked,
        bool isPaperInserted,
        float tonerRemaining,
        int printQueueLength)
    {
        IsSlotLocked = isSlotLocked;
        IsPaperInserted = isPaperInserted;
        TonerRemaining = tonerRemaining;
        PrintQueueLength = printQueueLength;
    }
}

[Serializable, NetSerializable]
public sealed class PhotocopierPrintMessage : BoundUserInterfaceMessage
{
    public int Amount { get; }
    public string Collection { get; }
    public string Group { get; }
    public string FormName { get; }

    public PhotocopierPrintMessage(int amount, string collection, string group, string formName)
    {
        Amount = amount;
        Collection = collection;
        Group = group;
        FormName = formName;
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
