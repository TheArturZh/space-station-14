// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using System.Numerics;
using Content.Shared.SS220.BloomLight;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client.SS220.BloomLight;

public sealed class BloomLightOverlay : Overlay
{
    private EntityManager _entity;
    private SharedTransformSystem _transform;
    private SpriteSystem _sprite;
    private IPrototypeManager _prototype = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;
    public override bool RequestScreenTexture => true;

    private readonly ShaderInstance _shader;

    public BloomLightOverlay(EntityManager entMan, IPrototypeManager protoMan)
    {
        _entity = entMan;
        _transform = entMan.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
        _sprite = entMan.EntitySysManager.GetEntitySystem<SpriteSystem>();
        _prototype = protoMan;
        _shader = _prototype.Index<ShaderPrototype>("BlurryLighting").InstanceUnique();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;

        var lightQuery = _entity.GetEntityQuery<PointLightComponent>();
        var xformQuery = _entity.GetEntityQuery<TransformComponent>();

        var bounds = args.WorldAABB.Enlarged(5f);

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        var query = _entity.AllEntityQueryEnumerator<BloomLightMaskComponent, TransformComponent, MetaDataComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform, out var meta))
        {
            if (xform.MapID != args.MapId)
                continue;

            if (!comp.Enabled)
                continue;

            var worldPos = _transform.GetWorldPosition(xform, xformQuery);
            if (!bounds.Contains(worldPos))
                continue;

            Color color = Color.White;
            if (lightQuery.TryGetComponent(uid, out var lightComp))
            {
                if (!lightComp.Enabled)
                    continue;

                if (comp.UseLightColor)
                    color = lightComp.Color;
            }

            var (_, worldRot, worldMatrix) = xform.GetWorldPositionRotationMatrix(xformQuery);
            handle.SetTransform(worldMatrix);
            handle.UseShader(comp.UseShader ? _shader : null);

            foreach (var mask in comp.LightMasks)
            {
                var texture = _sprite.Frame0(mask);
                var offsetX = -0.5f - (texture.Width / 2) / EyeManager.PixelsPerMeter;
                var offsetY = 0.5f - (texture.Height / 2) / EyeManager.PixelsPerMeter;
                handle.DrawTexture(texture, new Vector2(offsetX, offsetY), color);
            }
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3.Identity);
    }
}
