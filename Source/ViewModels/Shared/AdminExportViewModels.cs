using System.Collections.Generic;

namespace Solution.ViewModels.Shared
{
    public sealed class AdminTableExportRequest
    {
        public string Format { get; set; }
        public string Titre { get; set; }
        public string Filtres { get; set; }
        public string PageSource { get; set; }
        public List<string> Entetes { get; set; } = new();
        public List<List<string>> Lignes { get; set; } = new();
    }
}
