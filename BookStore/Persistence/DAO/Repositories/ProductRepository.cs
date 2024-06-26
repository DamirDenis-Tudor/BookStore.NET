/**************************************************************************
 *                                                                        *
 *  Description: ProductRepository                                        *
 *  Website:     https://github.com/DamirDenis-Tudor/PetShop-ProiectIP    *
 *  Copyright:   (c) 2024, Damir Denis-Tudor                              *
 *                                                                        *
 *  This code and information is provided "as is" without warranty of     *
 *  any kind, either expressed or implied, including but not limited      *
 *  to the implied warranties of merchantability or fitness for a         *
 *  particular purpose. You are free to use this source code in your      *
 *  applications as long as the original copyright notice is included.    *
 *                                                                        *
 **************************************************************************/

using Common;
using Microsoft.EntityFrameworkCore;
using Persistence.DAL;
using Persistence.DAO.Interfaces;
using Persistence.DTO.Product;
using Persistence.Entity;
using Persistence.Mappers;


namespace Persistence.DAO.Repositories;

internal class ProductRepository(DatabaseContext dbContext) : IProductRepository
{
    public Result<VoidResult, DaoErrorType> RegisterProduct(ProductDto productDto)
    {
        try
        {
            var productInfo = dbContext.ProductInfos
                .Include(p => p.Product)
                .FirstOrDefault(p => p.Name == productDto.ProductInfoDto.Name);

            if (productInfo is { Product: not null })
                return Result<VoidResult, DaoErrorType>.Fail(DaoErrorType.AlreadyRegistered,
                    $"Product {productDto.ProductInfoDto.Name} already registered.");

            var mappedProduct = MapperDto.MapToProduct(productDto);

            dbContext.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;
            dbContext.Products.Add(mappedProduct);
            dbContext.SaveChanges();
            dbContext.Entry(mappedProduct).Reload();
        }
        catch (DbUpdateException e)
        {
            return Result<VoidResult, DaoErrorType>.Fail(DaoErrorType.DatabaseError,
                $"Failed to register product {productDto.ProductInfoDto.Name}: {e}");
        }

        return Result<VoidResult, DaoErrorType>.Success(VoidResult.Get(),
            $"Product {productDto.ProductInfoDto.Name} registered successfully.");
    }

    public Result<IList<string>, DaoErrorType> GetCategories()
    {
        var categories = new List<string>();
        dbContext.Products.Include(product => product.ProductInfo).ToList().ForEach(product =>
        {
            categories.Add(product.ProductInfo.Category);
        });

        return categories.Count != 0
            ? Result<IList<string>, DaoErrorType>.Success(categories.Distinct().ToList(),
                "Successfully fetched categories.")
            : Result<IList<string>, DaoErrorType>.Fail(DaoErrorType.ListIsEmpty, "Fail to fetch categories.");
    }

    public Result<ProductDto, DaoErrorType> GetProduct(string name)
    {
        var product = dbContext.Products
            .Include(p => p.ProductInfo)
            .FirstOrDefault(p => p.ProductInfo.Name == name && p.ProductInfo.Product != null!);

        if (product == null)
            return Result<ProductDto, DaoErrorType>
                .Fail(DaoErrorType.NotFound, $"Product {name} not found.");

        dbContext.Entry(product).Reload();

        var productDto = MapperDto.MapToProductDto(product);

        if (productDto == null)
            return Result<ProductDto, DaoErrorType>
                .Fail(DaoErrorType.NotFound, $"Product {name} not found.");
        return Result<ProductDto, DaoErrorType>
            .Success(productDto, $"Product {name} found.");
    }


    public Result<IList<ProductDto>, DaoErrorType> GetAllProducts()
    {
        var products = new List<ProductDto>();

        dbContext.Products
            .Include(p => p.ProductInfo)
            .ToList()
            .ForEach(p =>
            {
                dbContext.Entry(p).Reload();
                products.Add(MapperDto.MapToProductDto(p)!);
            });

        return products.Count != 0
            ? Result<IList<ProductDto>, DaoErrorType>.Success(products, "Products fetched succesfully.")
            : Result<IList<ProductDto>, DaoErrorType>.Fail(DaoErrorType.ListIsEmpty, "No products registered.");
    }

    public Result<IList<ProductDto>, DaoErrorType> GetAllProductsByCategory(string category)
    {
        var orderSessions = dbContext.Products
            .Include(p => p.ProductInfo)
            .Where(p => p.ProductInfo.Category == category)
            .ToList();

        var orderSessionDtos = orderSessions
            .Select(MapperDto.MapToProductDto)
            .ToList();

        return orderSessions.Count != 0 && orderSessionDtos.Count != 0
            ? Result<IList<ProductDto>, DaoErrorType>.Success(orderSessionDtos!, "Products list returned.")
            : Result<IList<ProductDto>, DaoErrorType>.Fail(DaoErrorType.ListIsEmpty, "No order session found.");
    }

    public Result<IList<ProductStatsDto>, DaoErrorType> GetAllProductsStats()
    {
        var products = new List<ProductStatsDto>();
        dbContext.ProductInfos
            .Include(p => p.OrderProducts)
            .ToList()
            .ForEach(
                p =>
                {
                    var totalRevenue = 0.0m;
                    var totalItemsSold = 0;
                    p.OrderProducts.ToList()
                        .ForEach(
                            o =>
                            {
                                totalRevenue += o.OrderTimePrice;
                                totalItemsSold += o.Quantity;
                            }
                        );

                    var productStatsDto = new ProductStatsDto
                    {
                        TotalRevenue = totalRevenue, TotalItemsSold = totalItemsSold,
                        ProductInfoDto = MapperDto.MapToProductStatDto(p)!
                    };

                    var existingProduct = products.FirstOrDefault(stats =>
                        stats.ProductInfoDto.Name.Equals(productStatsDto.ProductInfoDto.Name));
                    
                    if (existingProduct != null)
                    {
                        existingProduct.TotalRevenue += totalRevenue;
                        existingProduct.TotalItemsSold += totalItemsSold;
                    }else
                    {
                        products.Add(productStatsDto);
                    }
                });

        return Result<IList<ProductStatsDto>, DaoErrorType>
            .Success(products, "Products found.");
    }

    public Result<VoidResult, DaoErrorType> UpdatePrice(string name, decimal newPrice)
    {
        var existingProduct = dbContext.Products
            .Include(p => p.ProductInfo)
            .FirstOrDefault(p => p.ProductInfo.Name == name);

        if (existingProduct == null)
        {
            return Result<VoidResult, DaoErrorType>.Fail(DaoErrorType.NotFound, $"Product '{name}' not found.");
        }

        existingProduct.Price = newPrice;

        try
        {
            dbContext.Update(existingProduct);
            dbContext.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            return Result<VoidResult, DaoErrorType>.Fail(DaoErrorType.DatabaseError,
                $"Failed to update product price: {e.Message}");
        }

        return Result<VoidResult, DaoErrorType>.Success(VoidResult.Get(),
            $"Product '{name}' price updated successfully.");
    }

    public Result<VoidResult, DaoErrorType> UpdateQuantity(string name, int quantity)
    {
        var existingProduct = dbContext.Products
            .Include(p => p.ProductInfo)
            .FirstOrDefault(p => p.ProductInfo.Name == name);

        if (existingProduct == null)
            return Result<VoidResult, DaoErrorType>.Fail(DaoErrorType.NotFound, $"Product '{name}' not found.");

        existingProduct.Quantity = quantity;
        try
        {
            dbContext.Update(existingProduct);
            dbContext.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            return Result<VoidResult, DaoErrorType>.Fail(DaoErrorType.DatabaseError,
                $"Failed to update product quantity: {e.Message}");
        }

        return Result<VoidResult, DaoErrorType>.Success(VoidResult.Get(),
            $"Product '{name}' quantity updated successfully.");
    }

    public Result<VoidResult, DaoErrorType> DeleteProduct(string name)
    {
        try
        {
            var existingProduct = dbContext.Products
                .Include(p => p.ProductInfo)
                .FirstOrDefault(p => p.ProductInfo.Name == name);
            if (existingProduct == null)
                return Result<VoidResult, DaoErrorType>.Fail(DaoErrorType.NotFound, $"Product '{name}' not found.");

            dbContext.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;
            dbContext.Products.Remove(existingProduct);
            dbContext.SaveChanges();
            dbContext.Products.Entry(existingProduct).Reload();
        }
        catch (DbUpdateException e)
        {
            return Result<VoidResult, DaoErrorType>.Fail(DaoErrorType.DatabaseError,
                $"Failed to delete product {name}: {e}");
        }

        return Result<VoidResult, DaoErrorType>.Success(VoidResult.Get(),
            $"Product '{name}' deleted successfully.");
    }
}