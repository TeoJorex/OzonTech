using FluentValidation;
using Homework2.Controllers.DTO.Requests;


namespace Homework2.Validators
{
    public class AddProductRequestValidator : AbstractValidator<AddProductRequest>
    {
        public AddProductRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Имя не должно быть пустым");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Цена должна быть больше 0");
            RuleFor(x => x.Weight).GreaterThan(0).WithMessage("Вес должен быть больше 0");
            RuleFor(x => x.ProductType).IsInEnum().WithMessage("Неверно выбран тип продукта");
            RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("Id склада должен быть больше 0");
        }
    }
}
