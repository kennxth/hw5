using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;


namespace ChenKennethHW5.Models
{
    public class Order
    {
        public int OrderID { get; set; }

        // Order Numbers start at 70001
        public static int nextOrderNumber = 70001;
        public string Email { get; set; }

        [Display(Name = "Order Number")]
        public int OrderNumber { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }

        public string OrderNotes { get; set; }

        public const decimal Tax_Rate = 0.0825m;

        // Navigation Property: one Order to many OrderDetails
        public List<OrderDetail> OrderDetails { get; set; }

        // Navigational Property: one Order belongs to one Customer
        public AppUser Customer { get; set; }

        // Read-only calculated properties
        [NotMapped]
        public decimal Subtotal
        {
            get
            {
                return OrderDetails?.Sum(od => od.ExtendedPrice) ?? 0m;
            }
        }

        [NotMapped]
        public decimal SalesTax
        {
            get
            {
                return Subtotal * Tax_Rate;
            }
        }

        [NotMapped]
        public decimal Total
        {
            get
            {
                return Subtotal + SalesTax;
            }
        }


        //Auto-generate OrderNumber
        public Order()
        {
            this.OrderNumber = nextOrderNumber;
            nextOrderNumber++;
            this.OrderDate = DateTime.Now;
        }
    }
}
