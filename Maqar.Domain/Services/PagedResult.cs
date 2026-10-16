namespace Maqar.Domain.Services
{
    /// <summary>One page of a filtered list plus the total match count — what a pager needs.</summary>
    public sealed class PagedResult<T>
    {
        public PagedResult(int pageIndex, int pageSize, int count, IReadOnlyList<T> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Count = count;
            Data = data;
        }

        /// <summary>1-based.</summary>
        public int PageIndex { get; }
        public int PageSize { get; }
        /// <summary>Total matches across every page.</summary>
        public int Count { get; }
        public IReadOnlyList<T> Data { get; }

        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Count / (double)PageSize);
        public bool HasPrevious => PageIndex > 1;
        public bool HasNext => PageIndex < TotalPages;
    }
}
