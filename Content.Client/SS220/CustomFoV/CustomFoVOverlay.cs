// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using System.Numerics;
using Content.Shared.SS220.CustomFoV;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.SS220.CustomFoV;

public sealed class CustomFoVOverlay : Overlay
{
    private readonly IEntityManager _entMan;
    private readonly IPrototypeManager _prototype;
    private readonly SpriteSystem _sprite;
    private readonly SharedTransformSystem _transform;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    private readonly SpriteSpecifier _fovCorner;
    private readonly ShaderInstance _shader;

    internal CustomFoVOverlay(EntityManager entMan, IPrototypeManager prototype)
    {
        _entMan = entMan;
        _prototype = prototype;

        _fovCorner = new SpriteSpecifier.Texture(new("SS220/Misc/fovside.png"));
        _sprite = _entMan.EntitySysManager.GetEntitySystem<SpriteSystem>();
        _transform = _entMan.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
        _shader = _prototype.Index<ShaderPrototype>("unshaded").InstanceUnique();

        ZIndex = (int) Shared.DrawDepth.DrawDepth.WallTops;
    }

    private Dictionary<EntityUid, Dictionary<Vector2i, Entity<TransformComponent>>> _entMapDict = new();

    protected override void Draw(in OverlayDrawArgs args)
    {
        _entMapDict.Clear();

        if (args.Viewport.Eye is not { } eye || !eye.DrawFov)
            return;

        var eyeAngle = eye.Rotation;
        var texture = _sprite.Frame0(_fovCorner);
        var handle = args.WorldHandle;

        var customFovQuery = _entMan.AllEntityQueryEnumerator<FoVOverlayComponent>();
        var occluderQuery = _entMan.GetEntityQuery<OccluderComponent>();
        var xformQuery = _entMan.GetEntityQuery<TransformComponent>();

        while (customFovQuery.MoveNext(out var entity, out var comp))
        {
            if (!occluderQuery.TryGetComponent(entity, out var occluder) || !occluder.Enabled)
                continue;

            if (!xformQuery.TryGetComponent(entity, out var xform) || !xform.Anchored || !xform.GridUid.HasValue)
                continue;

            var gridComp = _entMan.GetComponent<MapGridComponent>(xform.GridUid.Value);
            var tile = gridComp.CoordinatesToTile(xform.Coordinates);

            // Create lookup maps for grids so neighbors can be found quickly
            if (!_entMapDict.TryGetValue(xform.GridUid.Value, out var gridDict))
            {
                gridDict = new();
                _entMapDict.Add(xform.GridUid.Value, gridDict);
            }

            var entityEntry = new Entity<TransformComponent>(entity, xform);
            gridDict.Add(tile, entityEntry);
        }

        handle.UseShader(_shader);

        foreach (var (_, objMap) in _entMapDict)
        {
            foreach (var (pos, entityEntry) in objMap)
            {
                var direction = entityEntry.Comp.WorldPosition - eye.Position.Position;
                var directionSign = new Vector2(MathF.Sign(direction.X), MathF.Sign(direction.Y));

                var (_, worldRot, worldMatrix) = _transform.GetWorldPositionRotationMatrix(entityEntry.Comp, xformQuery);

                if (directionSign.Y != 0)
                {
                    var additionalAngle = directionSign.Y > 0 ? 180 : 0;
                    //worldMatrix.Add(Matrix3.CreateRotation(Angle.FromDegrees(additionalAngle)), out var rotatedMatrix);

                    handle.SetTransform(worldMatrix);
                    handle.DrawTexture(texture, new Vector2(-0.5f, -0.5f));
                }
            }
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3.Identity);
        _entMapDict.Clear();
    }
}
