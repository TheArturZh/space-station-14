using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Shared.SS220.SupaKitchen;

public abstract class SharedCookingSystem : EntitySystem
{
    [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SupaRecipeManager _recipeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public static (CookingRecipePrototype, int) CanSatisfyRecipe(
        CookingInstrumentComponent component,
        CookingRecipePrototype recipe,
        Dictionary<string, int> solids,
        Dictionary<string, FixedPoint2> reagents,
        uint cookingTimer
        )
    {
        return CanSatisfyRecipe(component.InstrumentType, component.IgnoreTime ? 0 : cookingTimer, recipe, solids, reagents);
    }

    public bool TryAddIngredientToDicts(
        Dictionary<string, int> solidsDict,
        Dictionary<string, FixedPoint2> reagentDict,
        EntityUid item,
        bool recursive)
    {
        var metaData = MetaData(item); //this still begs for cooking refactor
        if (metaData.EntityPrototype == null)
            return false;

        var id = metaData.EntityPrototype.ID;

        if (solidsDict.ContainsKey(id))
            solidsDict[id]++;
        else
            solidsDict.Add(id, 1);

        if (TryComp<SolutionContainerManagerComponent>(item, out var solMan))
        {
            foreach (var (_, solution) in solMan.Solutions)
            {
                foreach (var (reagent, quantity) in solution.Contents)
                {
                    if (reagentDict.ContainsKey(reagent.Prototype))
                        reagentDict[reagent.Prototype] += quantity;
                    else
                        reagentDict.Add(reagent.Prototype, quantity);
                }
            }
        }

        if (recursive && TryComp<FoodIngredientComponent>(item, out var ingredientComponent))
        {
            foreach (var ingredient in ingredientComponent.IngredientContainer.ContainedEntities)
            {
                TryAddIngredientToDicts(solidsDict, reagentDict, ingredient, true);
            }
        }

        return true;
    }

    public bool TryCookContainerByRecipe(Container storage, CookingRecipePrototype recipe, [NotNullWhen(true)] out EntityUid? result)
    {
        if (storage.ContainedEntities.Count == 0)
        {
            result = null;
            return false;
        }

        var anyEnt = storage.ContainedEntities[0];
        var coords = Transform(anyEnt).Coordinates;

        SubtractContents(storage, recipe);
        result = Spawn(recipe.Result, coords);
        return true;
    }

    public bool TryCookEntityByRecipe(EntityUid entityToCook, CookingRecipePrototype recipe, [NotNullWhen(true)] out EntityUid? result)
    {
        var coords = Transform(entityToCook).Coordinates;

        EntityManager.DeleteEntity(entityToCook);
        result = Spawn(recipe.Result, coords);
        return true;
    }

    public bool TryCookEntity(EntityUid entityToCook, string? instrumentType, [NotNullWhen(true)] out EntityUid? result)
    {
        result = null;

        var solidsDict = new Dictionary<string, int>();
        var reagentDict = new Dictionary<string, FixedPoint2>();
        if (!TryAddIngredientToDicts(solidsDict, reagentDict, entityToCook, true))
            return false;

        var portionedRecipe = GetSatisfiedPortionedRecipe(
            instrumentType, solidsDict, reagentDict, null);
        if (portionedRecipe.Item1 == null)
            return false;

        var coords = Transform(entityToCook).Coordinates;
        EntityManager.DeleteEntity(entityToCook);
        result = Spawn(portionedRecipe.Item1.Result, coords);
        return true;
    }

    public static (CookingRecipePrototype, int) CanSatisfyRecipe(
        string? instrumentType,
        uint? cookingTimer,
        CookingRecipePrototype recipe,
        Dictionary<string, int> solids,
        Dictionary<string, FixedPoint2> reagents
        )
    {
        var portions = 0;

        if (instrumentType != recipe.InstrumentType)
            return (recipe, 0);

        if (
            cookingTimer is not null &&
            cookingTimer % recipe.CookTime != 0
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
        if (cookingTimer is uint cookingTimerValid)
            portions = (int) Math.Min(portions, cookingTimerValid / recipe.CookTime);


        return (recipe, portions);
    }

    public void SubtractContents(Container container, CookingRecipePrototype recipe)
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

                    var quant = solution.GetTotalPrototypeQuantity(reagent);

                    if (quant >= totalReagentsToRemove[reagent])
                    {
                        quant = totalReagentsToRemove[reagent];
                        totalReagentsToRemove.Remove(reagent);
                    }
                    else
                    {
                        totalReagentsToRemove[reagent] -= quant;
                    }

                    _solutionContainer.RemoveReagent(item, solution, reagent, quant);
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
                        container.Remove(item);
                        EntityManager.DeleteEntity(item);
                        break;
                    }
                }
            }
        }
    }

    public (CookingRecipePrototype, int) GetSatisfiedPortionedRecipe(
        CookingInstrumentComponent component,
        Dictionary<string, int> solidsDict,
        Dictionary<string, FixedPoint2> reagentDict,
        uint cookingTimer
        )
    {
        return _recipeManager.Recipes.Select(r =>
            CanSatisfyRecipe(component, r, solidsDict, reagentDict, cookingTimer)).FirstOrDefault(r => r.Item2 > 0);
    }

    public (CookingRecipePrototype, int) GetSatisfiedPortionedRecipe(
        string? instrumentType,
        Dictionary<string, int> solidsDict,
        Dictionary<string, FixedPoint2> reagentDict,
        uint? cookingTimer
        )
    {
        return _recipeManager.Recipes.Select(r =>
            CanSatisfyRecipe(instrumentType, cookingTimer, r, solidsDict, reagentDict)).FirstOrDefault(r => r.Item2 > 0);
    }
}
