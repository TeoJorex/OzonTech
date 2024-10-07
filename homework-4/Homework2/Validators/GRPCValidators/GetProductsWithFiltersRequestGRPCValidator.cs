using FluentValidation;
using GrpcService;

namespace Homework2.Validators.GRPCValidators
{
    public class GetProductsWithFiltersRequestGRPCValidator : AbstractValidator<GetProductsByFiltersRequest>
    {
        public GetProductsWithFiltersRequestGRPCValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Номер страницы должен быть больше 0");
            RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Размер страницы не должен быть больше 0");
        }
    }
}
