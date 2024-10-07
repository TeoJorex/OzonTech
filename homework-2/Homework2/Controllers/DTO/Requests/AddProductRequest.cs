using Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Homework2.Controllers.DTO.Requests
{
    [SwaggerSchema]
    public class AddProductRequest
    {
        [SwaggerSchema("Наименование товара")]
        public string Name { get; set; }

        [SwaggerSchema("Цена товара")]
        public double Price { get; set; }

        [SwaggerSchema("Вес товара")]
        public double Weight { get; set; }

        [SwaggerSchema("Вид товара : Общий, Бытовая химия, Техника, Продукты;")]
        public ProductType ProductType { get; set; }

        [SwaggerSchema("Дата создания")]
        public DateTime CreatedDate { get; set; }

        [SwaggerSchema("Номер Склада")]
        public int WarehouseId { get; set; }
    }
}
