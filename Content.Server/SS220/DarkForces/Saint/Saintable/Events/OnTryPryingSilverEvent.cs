﻿using Content.Shared.Damage;
using Robust.Shared.GameObjects;

namespace Content.SecretStationServer.DarkForces.Saint.Saintable.Events;

public sealed class OnTryPryingSilverEvent : HandledEntityEventArgs, ISaintEntityEvent
{
    public DamageSpecifier DamageOnCollide { get; set; }
    public bool PushOnCollide { get; set; }

    public bool IsHandled
    {
        get => Handled;
        set => Handled = value;
    }

    public OnTryPryingSilverEvent(DamageSpecifier damageOnCollide, bool pushOnCollide)
    {
        DamageOnCollide = damageOnCollide;
        PushOnCollide = pushOnCollide;
    }
}
