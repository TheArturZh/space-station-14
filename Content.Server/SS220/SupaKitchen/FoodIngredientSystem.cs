using Content.Server.Nutrition.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.SS220.SupaKitchen;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server.SS220.SupaKitchen;

public sealed class FoodIngredientSystem : EntitySystem
{
    // limit ingrdient name getter recursion depth just in cause someone decides to do a funny.
    const int MAX_INGREDIENT_NAME_DEPTH = 10;

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

    private string GetIngredientNameWithDepth(EntityUid entity, bool includeSubIngredients, FoodIngredientComponent? component = null, int depth = 1)
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

        var ingredientListString = "";
        var goodIngredientNames = 0;

        if (includeSubIngredients && depth < MAX_INGREDIENT_NAME_DEPTH)
        {
            foreach (var ingredient in component.IngredientContainer.ContainedEntities)
            {
                var ingredientName = GetIngredientName(ingredient, includeSubIngredients);
                if (string.IsNullOrEmpty(ingredientName))
                    continue;

                if (goodIngredientNames > 0)
                    ingredientListString += ", ";

                ingredientListString += ingredientName;
                goodIngredientNames++;
            }
        }

        if (string.IsNullOrWhiteSpace(ingredientListString))
            return name;

        return name + " с " + ingredientListString;
    }

    public string GetIngredientName(EntityUid entity, bool includeSubIngredients, FoodIngredientComponent? component = null)
    {
        return GetIngredientNameWithDepth(entity, includeSubIngredients, component);
    }

    public void UpdateIngredientName(Entity<FoodIngredientComponent> entity)
    {
        var newName = GetIngredientName(entity, true);

        if (!string.IsNullOrWhiteSpace(newName))
            _metaData.SetEntityName(entity, newName);
    }

    public bool TryAddIngredient(Entity<FoodIngredientComponent> addTo, Entity<FoodIngredientComponent> add, out EntityUid result, bool tryCook = true, EntityUid? user = null)
    {
        result = addTo;

        if (!EntityManager.EntityExists(addTo) || !EntityManager.EntityExists(add))
            return false;

        if (!addTo.Comp.CanAddIngredients)
            return false;

        if (_openable.IsClosed(add, user) || _openable.IsClosed(addTo, user))
            return false;

        _container.Insert(add.Owner, addTo.Comp.IngredientContainer);

        // TODO: Extra item support (add as ingredients, ensure FoodIngredientComponent)
        // Recipe check
        var solidsDict = new Dictionary<string, int>();
        var reagentDict = new Dictionary<string, FixedPoint2>();

        _cooking.TryAddIngredientToDicts(solidsDict, reagentDict, addTo, true);

        if (tryCook)
        {
            if (_cooking.TryCookEntity(addTo, null, out var cookResult))
            {
                result = cookResult.Value;
            }
            else
            {
                UpdateIngredientName(addTo);
            }
        }

        return true;
    }

    private void OnInteract(Entity<FoodIngredientComponent> entity, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<FoodIngredientComponent>(args.Used, out var otherIngredient))
            return;

        TryAddIngredient(entity, new(args.Used, otherIngredient), out _, true, args.User);
        args.Handled = true;
    }
}
