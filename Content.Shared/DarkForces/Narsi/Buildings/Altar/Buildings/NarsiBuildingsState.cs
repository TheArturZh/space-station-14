using Robust.Shared.Serialization;

namespace Content.Shared.SecretStation.DarkForces.Narsi.Buildings.Altar.Buildings;

[Serializable, NetSerializable]
public record NarsiBuildingsState(List<NarsiBuildingUIModel> LearnedBuildings, List<NarsiBuildingUIModel> ToLearnBuildings);
