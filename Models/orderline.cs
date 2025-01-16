namespace RawnaqProject.Models
{
    public class orderline
    {
        public int Id { get; set; }
        public string itemname { get; set; }
        public int itemquant { get; set; }
        public decimal itemprice { get; set; }
        public int orderid { get; set; }

    }
}
