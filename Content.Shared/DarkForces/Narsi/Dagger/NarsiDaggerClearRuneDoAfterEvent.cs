using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.SecretStation.DarkForces.Narsi.Dagger;

[Serializable, NetSerializable]
public sealed partial class NarsiDaggerClearRuneDoAfterEvent : DoAfterEvent
{
    public NarsiDaggerClearRuneDoAfterEvent()
    {

    }

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
