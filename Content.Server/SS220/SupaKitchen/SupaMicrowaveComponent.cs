// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
namespace Content.Server.SS220.SupaKitchen;

[RegisterComponent]
public sealed partial class SupaMicrowaveComponent : Component
{
    [DataField("temperatureUpperThreshold")]
    public float TemperatureUpperThreshold = 373.15f;
}
