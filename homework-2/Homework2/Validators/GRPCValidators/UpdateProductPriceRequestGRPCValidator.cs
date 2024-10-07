using FluentValidation;
using GrpcService;

namespace Homework2.Validators.GRPCValidators
{
    public class UpdateProductPriceRequestGRPCValidator : AbstractValidator<UpdatePricetByIdRequest>
    {
        public UpdateProductPriceRequestGRPCValidator()
        {
            RuleFor(x => x.NewPrice).GreaterThan(0).WithMessage("Цена должна быть больше 0"); 
        }
    }
}
