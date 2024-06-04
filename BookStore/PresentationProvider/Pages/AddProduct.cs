/**************************************************************************
 *                                                                        *
 *  File:        AddProduct.cs                                            *
 *  Copyright:   (c) 2024, Asmarandei Catalin                             *
 *  Website:     https://github.com/DamirDenis-Tudor/BookStore.NET        *
 *  Description: The page where the provider can add new products         *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/

using Business.BAL;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Persistence.DTO.Product;
using System.ComponentModel.DataAnnotations;
using PresentationProvider.Services;
using Presentation.Services;

namespace PresentationProvider.Pages
{
    /// <summary>
    /// Add new product to the system
    /// </summary>
    public partial class AddProduct
    {
        /// <summary>
        /// The user login service for getting the token of the user
        /// </summary>
        [Inject]
        public IUserLoginService UserData { get; set; } = null!;

        /// <summary>
        /// The navigation manager for redirecting the user to the home page
        /// </summary>
        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        /// <summary>
        /// The business facade singleton
        /// </summary>
        [Inject]
        public BusinessFacade Business { get; set; } = null!;

        /// <summary>
        /// The informations introduced by the provider for adding a new product
        /// </summary>
        public record ProductData
        {
            /// <summary>
            /// The name of the product
            /// </summary>
            [Required(ErrorMessage = "Completeaza campul \"Nume produs\""),
             MinLength(10, ErrorMessage = "Namele trebuie sa aiba minim 10 caractere")]
            public string Name { get; set; } = null!;

            /// <summary>
            /// The price of the product
            /// </summary>
            [Required(ErrorMessage = "Completeaza campul \"Pret\"")]
            [Range(0, int.MaxValue, ErrorMessage = "Invalid Quantity")]
            public decimal Price { get; set; }

            /// <summary>
            /// The quantity of the product
            /// </summary>
            [Required(ErrorMessage = "Completeaza campul \"Cantitate\"")]
            [Range(0, int.MaxValue, ErrorMessage = "Invalid Quantity")]
            public int Quantity { get; set; }

            /// <summary>
            /// The category of the product
            /// </summary>
            [Required(ErrorMessage = "Completeaza campul \"Categorie produs\""),
             MinLength(5, ErrorMessage = "Descrierea trebuie sa aiba minim 30 caractere")]
            public string Category { get; set; } = null!;

            [Required(ErrorMessage = "Completeaza campul \"Descriere produs\""),
             MinLength(5, ErrorMessage = "Descrierea trebuie sa aiba minim 50 caractere")]
            public string Description { get; set; } = null!;

            /// <summary>
            /// The link to the photo of the product
            /// </summary>
            [Required(ErrorMessage = "Completeaza campul \"Descriere produs\""),
             MinLength(5, ErrorMessage = "Descrierea trebuie sa aiba minim 50 caractere")]
            public string? Photo { get; set; }

            /// <summary>
            /// Convert the object to a <see cref="ProductDto"/>
            /// </summary>
            /// <returns>The resulted object</returns>
            public ProductDto ConverseToDto()
            {
                return new ProductDto()
                {
                    Price = Price,
                    Quantity = Quantity,
                    ProductInfoDto = new ProductInfoDto
                    {
                        Name = Sanitizer.SanitizeString(Name),
                        Category = Sanitizer.SanitizeString(Category),
                        Description = Sanitizer.SanitizeString(Description),
                        Link = Sanitizer.SanitizeString(Photo ?? "")
                    }
                };
            }
        }

        /// <summary>
        /// The product that is mapped to the form
        /// </summary>
        private readonly ProductData _product = new ProductData();

        /// <summary>
        /// Event called when the provider submits the form
        /// Validates the form and if it is valid, the product is added to the system
        /// If the process was successful, the provider is redirected to the home page
        /// </summary>
        /// <param name="editContext"></param>
        /// <returns></returns>
        private async Task PlaceOrder(EditContext editContext)
        {
            if (editContext.Validate())
            {
                var sessionToken = await UserData.GetToken();
                if (sessionToken != null)
                {
                    var username = Business.AuthService.GetUsername(sessionToken);

                    Business.InventoryService.RegisterProduct(username.SuccessValue, _product.ConverseToDto());
                    NavigationManager.NavigateTo("/home");
                }
            }
        }
    }
}