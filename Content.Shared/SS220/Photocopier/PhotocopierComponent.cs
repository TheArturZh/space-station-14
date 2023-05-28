using Content.Shared.Containers.ItemSlots;
using Content.Shared.SS220.Photocopier.Forms;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.SS220.Photocopier;

[RegisterComponent, NetworkedComponent]
public sealed class PhotocopierComponent : Component
{
    public const string PaperSlotId = "Paper";

    /// <summary>
    /// 	Used by the server to determine how long the photocopier stays in the "Printing" state.
    /// </summary>
    [DataField("printingTime")]
    public float PrintingTime = 2.0f;

    /// <summary>
    ///     Sound that plays when inserting paper.
    ///     Whether it plays or not depends on power availability.
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
    ///     Sound that plays when printing
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
    /// 	Contains the item to be copied, assumes it's paper
    /// </summary>
    [DataField("paperSlot", required: true)]
    public ItemSlot PaperSlot = new();

    /// <summary>
    /// 	Remaining time of printing
    /// </summary>
    [DataField("printingTimeRemaining")]
    public float PrintingTimeRemaining;

    /// <summary>
    /// 	Remaining amount of copies to print
    /// </summary>
    [ViewVariables]
    [DataField("copiesQueued")]
    public int CopiesQueued;

    /// <summary>
    /// 	Whether this photocopier is currently scanning.
    ///     Used by server to unlock the slot after copying and to change appearance.
    /// </summary>
    [ViewVariables]
    [DataField("isScanning")]
    public bool IsScanning;

    /// <summary>
    /// 	Collections of forms available in UI
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("formCollections")]
    public HashSet<string> FormCollections = new();

    /// <summary>
    /// 	Maximum amount of copies that can be queued
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("maxQueueLength")]
    public int MaxQueueLength = 10;

    /// <summary>
    /// 	A content that is cached for copying.
    /// </summary>
    [ViewVariables]
    [DataField("dataToCopy", serverOnly: true)]
    public Form? DataToCopy;

    /// <summary>
    /// 	A content that is cached for copying.
    /// </summary>
    [DataField("printAudioStream", serverOnly: true)]
    public IPlayingAudioStream? PrintAudioStream;
}

