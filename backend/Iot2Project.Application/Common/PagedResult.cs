using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot2Project.Application.Common
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public long TotalItems { get; init; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}
