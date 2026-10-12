using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Solution.ModelBinding;

/// <summary>
/// Accepte les séparateurs décimaux français et informatique pour les montants.
/// Ainsi, 9,91 et 9.91 produisent tous les deux la valeur décimale 9,91.
/// </summary>
public sealed class FlexibleDecimalModelBinder : IModelBinder
{
    private const NumberStyles DecimalStyles =
        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var valeurSaisie = valueProviderResult.FirstValue;
        var typeNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) != null;

        if (string.IsNullOrWhiteSpace(valeurSaisie))
        {
            if (typeNullable)
                bindingContext.Result = ModelBindingResult.Success(null);
            else
                AjouterErreur(bindingContext, valeurSaisie);

            return Task.CompletedTask;
        }

        if (TryParse(valeurSaisie, out var valeur))
            bindingContext.Result = ModelBindingResult.Success(valeur);
        else
            AjouterErreur(bindingContext, valeurSaisie);

        return Task.CompletedTask;
    }

    internal static bool TryParse(string value, out decimal result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalisee = value.Trim()
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("\u00a0", string.Empty, StringComparison.Ordinal)
            .Replace("\u202f", string.Empty, StringComparison.Ordinal);

        var derniereVirgule = normalisee.LastIndexOf(',');
        var dernierPoint = normalisee.LastIndexOf('.');

        if (derniereVirgule >= 0 && dernierPoint >= 0)
        {
            if (derniereVirgule > dernierPoint)
            {
                // Exemple européen/français : 1.234,56
                normalisee = normalisee.Replace(".", string.Empty, StringComparison.Ordinal)
                    .Replace(',', '.');
            }
            else
            {
                // Exemple informatique/nord-américain : 1,234.56
                normalisee = normalisee.Replace(",", string.Empty, StringComparison.Ordinal);
            }
        }
        else if (derniereVirgule >= 0)
        {
            normalisee = normalisee.Replace(',', '.');
        }

        return decimal.TryParse(
            normalisee,
            DecimalStyles,
            CultureInfo.InvariantCulture,
            out result);
    }

    private static void AjouterErreur(ModelBindingContext bindingContext, string valeurSaisie)
    {
        var nomChamp = bindingContext.ModelMetadata.GetDisplayName();
        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            $"La valeur « {valeurSaisie} » n'est pas un montant valide pour le champ « {nomChamp} ». Utilisez, par exemple, 9,91 ou 9.91.");
    }
}

public sealed class FlexibleDecimalModelBinderProvider : IModelBinderProvider
{
    private static readonly IModelBinder Binder = new FlexibleDecimalModelBinder();

    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType)
            ?? context.Metadata.ModelType;

        return type == typeof(decimal) ? Binder : null;
    }
}
