namespace BookstoreApi.Models
{
    public class Revenue
    {
        public string OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime Date { get; set; }
    }
}
