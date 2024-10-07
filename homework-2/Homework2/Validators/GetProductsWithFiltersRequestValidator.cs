using FluentValidation;
using Homework2.Controllers.DTO.Requests;

namespace Homework2.Validators
{
    public class GetProductsWithFiltersRequestValidator : AbstractValidator<GetProductsWithFiltersRequest>
    {
        public GetProductsWithFiltersRequestValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Номер страницы должен быть больше 0");
            RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Размер страницы не должен быть больше 0");           
        }
    }
}
