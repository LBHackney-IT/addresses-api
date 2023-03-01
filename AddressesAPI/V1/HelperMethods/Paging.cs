namespace AddressesAPI.V1.HelperMethods
{
    public static class Paging
    {
        public static int CalculatePageCount(this int totalResultsCount, int pageSize)
        {
            if (totalResultsCount == 0)
                return 1;
            //eg 100 / 10 = 10
            if (totalResultsCount % pageSize == 0)
                return totalResultsCount / pageSize;
            //eg 101 / 10 = 10.1 so we cast to 10 and add 1 (11)
            var pageCount = (int)(totalResultsCount / pageSize) + 1;
            if (pageCount == 0)
                pageCount = 1;
            return pageCount;
        }
    }
}
