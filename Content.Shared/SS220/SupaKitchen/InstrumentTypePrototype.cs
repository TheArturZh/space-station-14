using Robust.Shared.Prototypes;

namespace Content.Shared.SS220.SupaKitchen;

[Prototype("instrumentType")]
public sealed class InstrumentTypePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("name")]
    private string _name = string.Empty;

    public string Name => Loc.GetString(_name);
}
