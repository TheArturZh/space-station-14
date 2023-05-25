using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.Photocopier
{
    [RegisterComponent, NetworkedComponent]
    public sealed class PhotocopierComponent : Component
    {
        public const string PaperSlotId = "Paper";

        /// <summary>
        /// 	Used by the server to determine how long the photocopier stays in the "Printing" state.
        /// 	Used by the client to determine how long the photocopier printing/scanning animation should be played.
        /// </summary>
        [DataField("printingTime")] public float PrintingTime = 2.0f;

        /// <summary>
        ///     Sound that plays when printing
        /// </summary>
        [DataField("printSound")] public SoundSpecifier PrintSound =
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
        [DataField("printingTimeRemaining")] public float PrintingTimeRemaining;

        /// <summary>
        /// 	Remaining amount of copies to print
        /// </summary>
        [ViewVariables] [DataField("copiesQueued")]
        public int CopiesQueued;

        /// <summary>
        /// 	Collections of forms available from UI
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)] [DataField("formCollections")]
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
        [DataField("contentToCopy")]
        public DataToCopy? DataToCopy;
    }

    [DataDefinition]
    public sealed class DataToCopy
    {
        [DataField("name", required: true)]
        public string Name { get; } = default!;

        [DataField("content", required: true)]
        public string Content { get; } = default!;

        [DataField("prototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>), required: true)]
        public string PrototypeId { get; } = default!;

        [DataField("stampState")]
        public string? StampState { get; }

        [DataField("stampedBy")]
        public List<string> StampedBy { get; } = new();

        private DataToCopy()
        {
        }

        public DataToCopy(string content, string name, string? prototypeId = null, string? stampState = null, List<string>? stampedBy = null)
        {
            Content = content;
            Name = name;
            PrototypeId = prototypeId ?? "";
            StampState = stampState;
            StampedBy = stampedBy ?? new List<string>();
        }
    }
}
