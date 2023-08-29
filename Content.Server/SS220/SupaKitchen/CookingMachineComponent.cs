// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.Construction.Prototypes;
using Content.Shared.SS220.SupaKitchen;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;


namespace Content.Server.SS220.SupaKitchen;

[RegisterComponent]
public sealed partial class CookingMachineComponent : Component
{
    #region  upgrades
    [DataField("cookTimeMultiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float CookTimeMultiplier = 1;
    [DataField("machinePartCookTimeMultiplier", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    public string MachinePartCookTimeMultiplier = "Capacitor";
    [DataField("cookTimeScalingConstant")]
    public float CookTimeScalingConstant = 0.5f;
    #endregion

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
    public (CookingRecipePrototype?, int) CurrentlyCookingRecipe = (null, 0);

    [ViewVariables]
    public int CurrentCookTimeButtonIndex;

    [DataField("broken"), ViewVariables]
    public bool Broken = false;

    [DataField("active"), ViewVariables, Access(typeof(CookingMachineSystem))]
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

public sealed class ProcessedInCookingMachineEvent : HandledEntityEventArgs
{
    public EntityUid MachineEntity;
    public CookingMachineComponent CookingMachine;
    public EntityUid? User;

    public ProcessedInCookingMachineEvent(EntityUid machineEntity, CookingMachineComponent cookingMachine, EntityUid? user = null)
    {
        MachineEntity = machineEntity;
        CookingMachine = cookingMachine;
        User = user;
    }
}

public sealed class BeforeCookingMachineFinished : EntityEventArgs
{
    public CookingMachineComponent CookingMachine;

    public BeforeCookingMachineFinished(CookingMachineComponent component)
    {
        CookingMachine = component;
    }
}
