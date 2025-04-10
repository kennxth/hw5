using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ChenKennethHW5.Models
{
    public enum ProductType { NewHardback, NewPaperback, UsedHardback, UsedPaperback, Other }
    public class Product
    {
        public int ProductID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
        public ProductType ProductType { get; set; }

        //Navigational Property: a product belongs to many suppliers
        public List<Supplier>? Suppliers { get; set; }

        //Navigational Property: a product has many order details
        public List<OrderDetail>? OrderDetails { get; set; }

    }

}
