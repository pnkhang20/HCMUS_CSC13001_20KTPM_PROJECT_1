namespace BookstoreApi.Models
{
    public class BookStoreDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string BooksCollectionName { get; set; } = null!;
        public string UsersCollectionName { get; set; } = null!;
        public string CategoriesCollectionName { get; set; } = null!;
        public string OrdersCollectionName { get; set; } = null!;
    }
}
