// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.SS220.Photocopier;

[RegisterComponent, NetworkedComponent]
public sealed class PhotocopierComponent : Component
{
    // ReSharper disable RedundantLinebreak

    public const string PaperSlotId = "CopierScan";
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
    /// Sound to play when component has been emagged
    /// </summary>
    [DataField("emagSound")]
    public SoundSpecifier EmagSound = new SoundCollectionSpecifier("sparks");

    /// <summary>
    /// Sound that plays when an emagged photocopier burns someones butt
    /// </summary>
    [DataField("buttDamageSound")]
    public SoundSpecifier ButtDamageSound =
        new SoundPathSpecifier("/Audio/Items/welder2.ogg")
        {
            Params = new AudioParams
            {
                Volume = -4f
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
    /// Damage dealt to a creature when they try to photocopy their butt on an emagged photocopier.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("emagButtDamage")]
    public DamageSpecifier? EmagButtDamage;

    /// <summary>
    /// Used by photocopier to determine whether the species on top of the photocopier is the same as it was
    /// without having to fetch the texture every tick.
    /// </summary>
    [ViewVariables]
    public string? ButtSpecies;

    /// <summary>
    /// Contains fields of components that will be copied.
    /// Is applied to a new entity that is created as a result of photocopying.
    /// </summary>
    [ViewVariables]
    public Dictionary<Type, IPhotocopiedComponentData>? DataToCopy;

    /// <summary>
    /// Contains metadata that will be copied.
    /// Is applied to a new entity that is created as a result of photocopying.
    /// </summary>
    public PhotocopyableMetaData? MetaDataToCopy;

    /// <summary>
    /// An audio stream of printing sound.
    /// Is saved in a variable so sound can be stopped later.
    /// </summary>
    public IPlayingAudioStream? PrintAudioStream;

    public PhotocopierState State = PhotocopierState.Idle;
}

