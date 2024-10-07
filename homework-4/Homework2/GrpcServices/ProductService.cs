using Domain.DTO.Requests;
using Domain.Entities;
using Domain.Exeptions;
using Domain.Services.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService;
using static GrpcService.ProductService;

namespace Homework2.GRPCServices
{
    public class ProductService : ProductServiceBase
    {
        private readonly IProductService _productService;

        public ProductService(IProductService productService)
        {
            _productService = productService;
        }

        public override Task<AddProductResponse> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            try
            {
                var productModel = new AddProductModel() 
                {
                    Name = request.Name,
                    Price = request.Price,
                    Weight = request.Weight,
                    ProductType = (Domain.Entities.ProductType)request.ProductType,
                    CreatedDate = request.CreatedDate.ToDateTime(),
                    WarehouseId = request.WarehouseId
                };

                return Task.FromResult(new AddProductResponse
                {
                    Id = _productService.Add(productModel)
                });

            }
            catch (Exception ex) 
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    
        public override Task<GetProductByIdResponse> GetProductById(GetProductByIdRequest request, ServerCallContext context)
        {
            try
            {
                var productModel = _productService.GetById(new GetProductModel()
                {
                    Id = request.Id
                });
                return Task.FromResult(new GetProductByIdResponse
                {
                    Name = productModel.Name,
                    Price = productModel.Price,
                    Weight = productModel.Weight,
                    ProductType = (PRODUCT_TYPE_GENERAL)productModel.ProductType,
                    CreatedDate = Timestamp.FromDateTime(productModel.CreatedDate),
                    WarehouseId = productModel.WarehouseId,
                });
            }
            catch (NotFoundExeption ex)
            {
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        } 
        
        public override Task<UpdatePricetByIdResponse> UpdatePricetById(UpdatePricetByIdRequest request, ServerCallContext context)
        {
            try
            {
                var updateProductPriceRequest = new UpdateProductPriceModel()
                {
                    Id = request.Id,
                    Price = request.NewPrice
                };               
                return Task.FromResult(new UpdatePricetByIdResponse { Message = _productService.UpdatePrice(updateProductPriceRequest)});

            }
            catch (NotFoundExeption ex)
            {
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    
        public override Task<GetProductsByFiltersResponse> GetProductsByFilters(GetProductsByFiltersRequest filterRequest, ServerCallContext context)
        {
            try
            {
                var getProductsWithFiltersRequestDTO = new GetProductsWithFiltersModel()
                {
                    ProductType = (ProductType?)filterRequest.ProductType,
                    DateTime = filterRequest.DateTime?.ToDateTime(),
                    WarehouseId = filterRequest.WarehouseId,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize
                };

                getProductsWithFiltersRequestDTO.ProductType = null;
                if (filterRequest.ProductTypeOptionCase == GetProductsByFiltersRequest.ProductTypeOptionOneofCase.ProductType)
                {
                    getProductsWithFiltersRequestDTO.ProductType = (ProductType)filterRequest.ProductType;
                }

                if (filterRequest.WarehouseId == 0)
                    getProductsWithFiltersRequestDTO.WarehouseId = null;

                var products = _productService.GetProductsByFilter(getProductsWithFiltersRequestDTO);

                var productsGRPCModel = new List<GrpcService.Product>();
                foreach (var product in products)
                {
                    productsGRPCModel.Add(new GrpcService.Product()
                    {
                        Name = product.Name,
                        Price = product.Price,
                        Weight = product.Weight,
                        ProductType = (GrpcService.PRODUCT_TYPE_GENERAL)product.ProductType,
                        CreatedDate = Timestamp.FromDateTime(product.CreatedDate),
                        WarehouseId= product.WarehouseId
                    });
                }
                var response = new GetProductsByFiltersResponse();
                response.Products.AddRange(productsGRPCModel);
                return Task.FromResult(response);

            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }

        }
    }
}
