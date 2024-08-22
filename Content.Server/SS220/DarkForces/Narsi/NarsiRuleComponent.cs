using Robust.Shared.Audio;

namespace Content.Server.SS220.DarkForces.Narsi;

[RegisterComponent]
[Access(typeof(NarsiRuleSystem))]
public sealed partial class NarsiRuleComponent : Component
{
    [DataField]
    public WinState WinStateStatus = WinState.Idle;

    [DataField]
    public EntityUid RuneSource = EntityUid.Invalid;

    [DataField]
    public SoundSpecifier NarsiExileSound = new SoundPathSpecifier("/Audio/DarkStation/Narsi/narsi_destroy.ogg");

    [DataField]
    public SoundSpecifier NarsiSummonSound = new SoundPathSpecifier("/Audio/DarkStation/Narsi/narsi_summon.ogg");

    public TimeSpan NarsiRepeatSoundAt = TimeSpan.Zero;
    public TimeSpan RoundEndAt = TimeSpan.Zero;
}

public enum WinState
{
    Idle,
    NarsiSummoning,
    NarsiLastStand,
    CultistWon
}
