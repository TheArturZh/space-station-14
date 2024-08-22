using Robust.Shared.GameObjects;

namespace Content.SecretStationServer.DarkForces.Narsi.Progress.Objectives.Rituals;

[ByRefEvent]
public record struct NarsiRitualCompletedEvent(string Ritual);
