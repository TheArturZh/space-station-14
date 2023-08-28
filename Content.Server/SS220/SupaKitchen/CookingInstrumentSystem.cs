// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Server.Chemistry.Components.SolutionManager;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.SS220.SupaKitchen;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server.SS220.SupaKitchen;
public sealed class CookingInstrumentSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CookingInstrumentComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, CookingInstrumentComponent component, ComponentInit ags)
    {
        component.Storage = _container.EnsureContainer<Container>(uid, "cooking_instrument_entity_container");
    }

    public static (CookingRecipePrototype, int) CanSatisfyRecipe(
        CookingInstrumentComponent component,
        CookingRecipePrototype recipe,
        Dictionary<string, int> solids,
        Dictionary<string, FixedPoint2> reagents,
        uint cookingTimer
        )
    {
        var portions = 0;

        if (
            cookingTimer % recipe.CookTime != 0
            && !component.IgnoreTime
            )
        {
            //can't be a multiple of this recipe
            return (recipe, 0);
        }

        foreach (var solid in recipe.IngredientsSolids)
        {
            if (!solids.ContainsKey(solid.Key))
                return (recipe, 0);

            if (solids[solid.Key] < solid.Value)
                return (recipe, 0);

            portions = portions == 0
                ? solids[solid.Key] / solid.Value.Int()
                : Math.Min(portions, solids[solid.Key] / solid.Value.Int());
        }

        foreach (var reagent in recipe.IngredientsReagents)
        {
            if (!reagents.ContainsKey(reagent.Key))
                return (recipe, 0);

            if (reagents[reagent.Key] < reagent.Value)
                return (recipe, 0);

            portions = portions == 0
                ? reagents[reagent.Key].Int() / reagent.Value.Int()
                : Math.Min(portions, reagents[reagent.Key].Int() / reagent.Value.Int());
        }

        //cook only as many of those portions as time allows
        if (!component.IgnoreTime)
            portions = (int) Math.Min(portions, cookingTimer / recipe.CookTime);


        return (recipe, portions);
    }

    private void SubtractContents(CookingInstrumentComponent component, Container container, CookingRecipePrototype recipe)
    {
        var totalReagentsToRemove = new Dictionary<string, FixedPoint2>(recipe.IngredientsReagents);

        // this is spaghetti ngl
        foreach (var item in container.ContainedEntities)
        {
            if (!TryComp<SolutionContainerManagerComponent>(item, out var solMan))
                continue;

            // go over every solution
            foreach (var (_, solution) in solMan.Solutions)
            {
                foreach (var (reagent, _) in recipe.IngredientsReagents)
                {
                    // removed everything
                    if (!totalReagentsToRemove.ContainsKey(reagent))
                        continue;

                    if (!solution.ContainsReagent(reagent))
                        continue;

                    var quant = solution.GetReagentQuantity(reagent);

                    if (quant >= totalReagentsToRemove[reagent])
                    {
                        quant = totalReagentsToRemove[reagent];
                        totalReagentsToRemove.Remove(reagent);
                    }
                    else
                    {
                        totalReagentsToRemove[reagent] -= quant;
                    }

                    _solutionContainer.TryRemoveReagent(item, solution, reagent, quant);
                }
            }
        }

        foreach (var recipeSolid in recipe.IngredientsSolids)
        {
            for (var i = 0; i < recipeSolid.Value; i++)
            {
                foreach (var item in container.ContainedEntities)
                {
                    var metaData = MetaData(item);
                    if (metaData.EntityPrototype == null)
                    {
                        continue;
                    }

                    if (metaData.EntityPrototype.ID == recipeSolid.Key)
                    {
                        component.Storage.Remove(item);
                        EntityManager.DeleteEntity(item);
                        break;
                    }
                }
            }
        }
    }
}
