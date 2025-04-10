﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ChenKennethHW5.Models
{
    public class OrderDetail
    {
        public int OrderDetailID { get; set; }

        [Required]
        public Int32 Quantity { get; set; }

        [Required]
        [Display(Name = "Product Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal ProductPrice { get; set; }

        [Display(Name = "Extended Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal ExtendedPrice { get; set; }

        // Navigation Properties
        [Required]
        public Order Order { get; set; }

        [Required]
        public Product Product { get; set; }
    }
}
