using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrack.Dto.Product
{
    public class RestoreProductDto
    {
        public int Id { get; set; }            
        public string Name { get; set; }        
        public string? Description { get; set; } 
        public string? Code { get; set; }        
        public string? ImageUrl { get; set; }        
        public int TotalQuantity { get; set; }  
    }
}
