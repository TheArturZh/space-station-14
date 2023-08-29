// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SupaKitchen;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

namespace Content.Server.SS220.SupaKitchen;
public sealed partial class CookingMachineComponent : Component
{
    #region  stats
    [DataField("maxCookingTimer"), ViewVariables(VVAccess.ReadWrite)]
    public uint MaxCookingTimer = 30;
    #endregion

    #region  state
    [ViewVariables]
    public uint CookingTimer = 0;

    [ViewVariables(VVAccess.ReadWrite)]
    public float CookTimeRemaining;

    [ViewVariables]
    public (CookingRecipePrototype?, int) CurrentlyCookingRecipe;

    [ViewVariables]
    public int CurrentCookTimeButtonIndex;

    [DataField("broken"), ViewVariables]
    public bool Broken = false;

    [DataField("active"), ViewVariables]
    public bool Active = false;
    #endregion

    #region  audio
    [DataField("beginCookingSound")]
    public SoundSpecifier StartCookingSound = new SoundPathSpecifier("/Audio/Machines/microwave_start_beep.ogg");
    [DataField("foodDoneSound")]
    public SoundSpecifier FoodDoneSound = new SoundPathSpecifier("/Audio/Machines/microwave_done_beep.ogg");
    [DataField("clickSound")]
    public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");
    [DataField("ItemBreakSound")]
    public SoundSpecifier ItemBreakSound = new SoundPathSpecifier("/Audio/Effects/clang.ogg");

    public IPlayingAudioStream? PlayingStream { get; set; }
    [DataField("loopingSound")]
    public SoundSpecifier LoopingSound = new SoundPathSpecifier("/Audio/Machines/microwave_loop.ogg");
    #endregion

    public Container Storage = default!;
}
