namespace Content.Shared.SS220.Photocopier;

[RegisterComponent]
public sealed class TonerCartridgeComponent : Component
{
    /// <summary>
    /// Maximum amount of charges of toner that cartridge contains.
    /// 1 charge is needed to print 1 sheet of paper.
    /// </summary>
    [DataField("capacity")]
    public int Capacity = 60;

    /// <summary>
    /// Amount of charges of toner that cartridge contains.
    /// </summary>
    [DataField("charges")]
    public int Charges = 60;
}
