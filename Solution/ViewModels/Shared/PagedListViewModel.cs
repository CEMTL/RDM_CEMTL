
using System;
using System.Collections.Generic;

namespace Solution.ViewModels.Shared
{
    public class PagedListViewModel<T>
    {
        public IList<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages
        {
            get
            {
                if (PageSize <= 0) return 1;
                return (int)Math.Ceiling((double)TotalItems / PageSize);
            }
        }

        public bool HasPreviousPage { get { return Page > 1; } }
        public bool HasNextPage { get { return Page < TotalPages; } }

        public PagedListViewModel()
        {
            Items = new List<T>();
            Page = 1;
            PageSize = 25;
        }
    }
}
