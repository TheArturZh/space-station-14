// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.SS220.SupaKitchen;

[RegisterComponent]
public sealed partial class CookingInstrumentComponent : Component
{
    [DataField("instrumentType", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<CookingInstrumentTypePrototype>))]
    [ViewVariables]
    public string? InstrumentType;

    [DataField("failureResult", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string FailureResult = "FoodBadRecipe";

    [ViewVariables]
    [DataField("ignoreTime")]
    public bool IgnoreTime = false;
}
