using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoesOnContainer.Web.WebMvc.Models
{
    public class CatalogItem
    {
        //Foi decidido usar uma classe só para as 3 entidades do banco de dados
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string PictureUrl { get; set; }

        public int CatalogBrandId { get; set; }
        public string CatalogBrand { get; set; }

        public int CatalogTypeId { get; set; }
        public string CatalogType { get; set; }

    }
}
