namespace AWAQ_Web
{
    public class PaginatedList
    {
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public int StartPage { get; set; }
        public int EndPage { get; set; }

        public PaginatedList() { }
        public PaginatedList(int totalItems, int page, int pageSize=10)
        {
            int totalPages = (int)Math.Ceiling((decimal)totalItems/(decimal)pageSize);
            int currentPage = page;

            int startPage = currentPage - 3;
            int endPage = currentPage + 2;

            if (startPage <= 0 ) {
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }

            if(endPage > totalPages)
            {
                endPage = totalPages;
                if(endPage > 4)
                {
                    startPage = endPage - 2;
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            StartPage = startPage;
            EndPage = endPage;
            PageSize = pageSize;
            TotalPages = totalPages;

        }
    }

}
