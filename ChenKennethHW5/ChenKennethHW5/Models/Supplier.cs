using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChenKennethHW5.Models
{
    [Authorize(Roles = "Admin")]
    public class Supplier
    {
        public int SupplierID { get; set; }

        public string SupplierName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        //Navigational Property: a supplier provides many products
        public List<Product>? Products { get; set; }

    }
}
