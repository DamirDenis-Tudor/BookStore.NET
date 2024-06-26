/**************************************************************************
 *                                                                        *
 *  Description: OrderProductDto                                          *
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

using Persistence.DTO.Product;

namespace Persistence.DTO.Order;

public record OrderProductDto
{
    public decimal Price { get; init; }
    public required string SessionCode { get; init; }
    public required int OrderQuantity { get; init; }
    
    public required ProductInfoDto ProductInfoDto { get; init; }
}