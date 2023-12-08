using Content.Server.Nutrition.EntitySystems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.SS220.SupaKitchen;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server.SS220.SupaKitchen;

public sealed class FoodIngredientSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly OpenableSystem _openable = default!;
    [Dependency] private readonly CookingSystem _cooking = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FoodIngredientComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FoodIngredientComponent, InteractUsingEvent>(OnInteract);
    }

    private void OnInit(Entity<FoodIngredientComponent> entity, ref ComponentInit args)
    {
        entity.Comp.IngredientContainer = _container.EnsureContainer<Container>(entity, FoodIngredientComponent.StorageId);
        UpdateIngredientName(entity);
    }

    public string GetIngredientName(EntityUid entity, bool includeSubIngredients, FoodIngredientComponent? component = null)
    {
        var proto = MetaData(entity).EntityPrototype;
        if (proto is null)
            return "";

        var name = proto.Name;

        if (component is null)
        {
            if (!TryComp(entity, out component))
                return name;
        }

        if (!includeSubIngredients)
            return name;

        var ingredientListString = "";
        var goodIngredientNames = 0;

        foreach (var ingredient in component.IngredientContainer.ContainedEntities)
        {
            var ingredientName = GetIngredientName(ingredient, false);
            if (string.IsNullOrEmpty(ingredientName))
                continue;

            if (goodIngredientNames > 0)
                ingredientListString += ", ";

            ingredientListString += ingredientName;
            goodIngredientNames++;
        }

        if (string.IsNullOrWhiteSpace(ingredientListString))
            return name;

        return name + " с " + ingredientListString;
    }

    public void UpdateIngredientName(Entity<FoodIngredientComponent> entity)
    {
        var newName = GetIngredientName(entity, true);

        if (!string.IsNullOrWhiteSpace(newName))
            _metaData.SetEntityName(entity, newName);
    }

    public bool TryAddIngredient(Entity<FoodIngredientComponent> addTo, Entity<FoodIngredientComponent> add, EntityUid? user = null)
    {
        if (_openable.IsClosed(add, user) || _openable.IsClosed(addTo, user))
            return false;

        _container.Insert(add.Owner, addTo.Comp.IngredientContainer);

        // TODO: Assembly support for cooking methods (adding main item to dict via method should automatically add ingredients)
        // TODO: "Cook item" method, that focuses on singular item instead of container
        // TODO: Extra item support (add as ingredients, ensure FoodIngredientComponent)
        // Recipe check
        var solidsDict = new Dictionary<string, int>();
        var reagentDict = new Dictionary<string, FixedPoint2>();

        // Add main item to solids
        _cooking.TryAddIngredientToDicts(solidsDict, reagentDict, addTo);

        foreach (var ingredient in addTo.Comp.IngredientContainer.ContainedEntities)
        {
            _cooking.TryAddIngredientToDicts(solidsDict, reagentDict, ingredient);
        }

        var portionedRecipe = _cooking.GetSatisfiedPortionedRecipe(
            string.Empty, solidsDict, reagentDict, null);

        if (portionedRecipe.Item1 != null)
        {
            // swap entity and transfer ingredients
        }
        else
        {
            UpdateIngredientName(addTo);
        }

        return true;
    }

    private void OnInteract(Entity<FoodIngredientComponent> entity, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<FoodIngredientComponent>(args.Used, out var otherIngredient))
            return;

        TryAddIngredient(entity, new(args.Used, otherIngredient));
        args.Handled = true;
    }
}
