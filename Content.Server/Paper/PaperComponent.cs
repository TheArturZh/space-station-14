using Content.Shared.Paper;
using Content.Shared.SS220.Photocopier;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Server.Paper
{
    [NetworkedComponent, RegisterComponent]
    public sealed class PaperComponent : SharedPaperComponent, IPhotocopyableComponent<PaperPhotocopiedData, PaperComponent>
    {
        public PaperAction Mode;
        [DataField("content")]
        public string Content { get; set; } = "";

        /// <summary>
        ///     Allows to forbid to write on paper without using stamps as a hack
        /// </summary>
        [DataField("writable")]
        public bool Writable { get; set; } = true;

        [DataField("contentSize")]
        public int ContentSize { get; set; } = 6000;

        [DataField("stampedBy")]
        public List<string> StampedBy { get; set; } = new();
        /// <summary>
        ///     Stamp to be displayed on the paper, state from beauracracy.rsi
        /// </summary>
        [DataField("stampState")]
        public string? StampState { get; set; }

        public PaperPhotocopiedData GetPhotocopiedData()
        {
            return new PaperPhotocopiedData()
            {
                Content = Content,
                Writable = Writable,
                ContentSize = ContentSize,
                StampedBy = StampedBy,
                StampState = StampState
            };
        }
    }

    [Serializable, NetSerializable]
    public sealed class PaperPhotocopiedData : PhotocopiedComponentData<PaperComponent>
    {
        public string? Content;
        public bool? Writable;
        public int? ContentSize;
        public List<string>? StampedBy;
        public string? StampState;

        public override void RestoreComponentFields(PaperComponent component)
        {
            if (Content is not null)
                component.Content = Content;

            if (Writable is { } writable)
                component.Writable = writable;

            if (ContentSize is { } contentSize)
                component.ContentSize = contentSize;

            if (StampedBy is not null)
                component.StampedBy = StampedBy;

            if (StampState is not null)
                component.StampState = StampState;
        }
    }
}
