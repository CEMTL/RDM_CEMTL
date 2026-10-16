using Microsoft.AspNetCore.Http;

namespace Solution.Helpers;

/// <summary>
/// Paramètres de pagination DataTables normalisés. Les valeurs reçues du client
/// sont considérées comme non fiables afin d'éviter les erreurs de conversion et
/// les requêtes non bornées sur le registre.
/// </summary>
public readonly record struct DataTableRequest(int Draw, int Start, int Length)
{
    public const int DefaultPageSize = 10;
    public const int MaximumPageSize = 100;
    public const int MaximumExportRows = 50000;

    public static DataTableRequest FromForm(IFormCollection form)
    {
        var draw = ParseNonNegative(form["draw"].FirstOrDefault());
        var start = ParseNonNegative(form["start"].FirstOrDefault());
        var requestedLength = ParseNonNegative(form["length"].FirstOrDefault());
        var exportAll = bool.TryParse(form["exportAll"].FirstOrDefault(), out var requestedExportAll) &&
                        requestedExportAll;
        var maximumLength = exportAll ? MaximumExportRows : MaximumPageSize;
        var length = requestedLength == 0
            ? DefaultPageSize
            : Math.Min(requestedLength, maximumLength);

        return new DataTableRequest(draw, start, length);
    }

    private static int ParseNonNegative(string value)
    {
        return int.TryParse(value, out var parsed) && parsed > 0
            ? parsed
            : 0;
    }
}
