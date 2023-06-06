using Content.Shared.Containers.ItemSlots;
using Content.Shared.SS220.Photocopier.Forms;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.SS220.Photocopier;

[RegisterComponent, NetworkedComponent]
public sealed class PhotocopierComponent : Component
{
    public const string PaperSlotId = "Paper";
    public const string TonerSlotId = "TonerCartridge";

    /// <summary>
    /// Used by the server to determine how long the photocopier stays in the "Printing" state.
    /// </summary>
    [DataField("printingTime")]
    public float PrintingTime = 2.0f;

    /// <summary>
    /// Sound that plays when inserting paper.
    /// Whether it plays or not depends on power availability.
    /// </summary>
    [DataField("paperInsertSound")]
    public SoundSpecifier PaperInsertSound =
        new SoundPathSpecifier("/Audio/Machines/scanning.ogg")
        {
            Params = new AudioParams
            {
                Volume = 0f
            }
        };

    /// <summary>
    /// Sound that plays when printing
    /// </summary>
    [DataField("printSound")]
    public SoundSpecifier PrintSound =
        new SoundPathSpecifier("/Audio/Machines/printer.ogg")
        {
            Params = new AudioParams
            {
                Volume = -2f
            }
        };

    /// <summary>
    /// Contains an item to be copied, assumes it's paper
    /// </summary>
    [DataField("paperSlot", required: true)]
    public ItemSlot PaperSlot = new();

    /// <summary>
    /// Contains a toner cartridge
    /// </summary>
    [DataField("tonerSlot", required: true)]
    public ItemSlot TonerSlot = new();

    /// <summary>
    /// Remaining time of printing
    /// </summary>
    [DataField("printingTimeRemaining")]
    public float PrintingTimeRemaining;

    /// <summary>
    /// Remaining amount of copies to print
    /// </summary>
    [ViewVariables]
    [DataField("copiesQueued")]
    public int CopiesQueued;

    /// <summary>
    /// Whether this photocopier is currently scanning.
    /// Used by server to unlock the slot after copying and to change appearance.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsScanning;

    /// <summary>
    /// Whether this photocopier is currently copying butt.
    /// Is used by server to determine whether an entity overlap check at the end of the printing is needed.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsCopyingButt;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsCopyingPhysicalButt;

    /// <summary>
    /// Collections of forms available in UI
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("formCollections")]
    public HashSet<string> FormCollections = new();

    /// <summary>
    /// Maximum amount of copies that can be queued
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("maxQueueLength")]
    public int MaxQueueLength
    {
        get => _maxQueueLength;
        set
        {
            if (value < 1)
                throw new Exception("MaxQueueLength can't be less than 1.");

            _maxQueueLength = value;
        }
    }
    private int _maxQueueLength = 10;

    /// <summary>
    /// A content that is cached for copying.
    /// </summary>
    [ViewVariables]
    public Form? DataToCopy;

    /// <summary>
    /// Used by photocopier to determine what to print out as a result of a copy operation
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public string? ButtTextureToCopy;

    /// <summary>
    /// Used by photocopier to determine whether the species on top of the photocopier is the same as it was
    /// without having to fetch the texture every tick.
    /// </summary>
    public string? ButtSpecies;

    /// <summary>
    /// An audio stream of printing sound.
    /// Is saved in a variable so sound can be stopped later.
    /// </summary>
    public IPlayingAudioStream? PrintAudioStream;
}

