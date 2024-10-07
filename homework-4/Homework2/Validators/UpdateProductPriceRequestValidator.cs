using FluentValidation;
using Homework2.Controllers.DTO.Requests;

namespace Homework2.Validators
{
    public class UpdateProductPriceRequestValidator : AbstractValidator<UpdateProductPriceRequest>
    {
        public UpdateProductPriceRequestValidator()
        {
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Цена должна быть больше 0");
        }
    }
}
