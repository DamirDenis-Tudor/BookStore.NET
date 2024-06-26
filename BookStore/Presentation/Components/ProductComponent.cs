﻿/**************************************************************************
 *                                                                        *
 *  File:        ProductView.cs                                           *
 *  Copyright:   (c) 2024, Asmarandei Catalin                             *
 *  Website:     https://github.com/DamirDenis-Tudor/BookStore.NET        *
 *  Description: Component that will be displayed for viewing             *
 *      the products                                                      *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/

using Microsoft.AspNetCore.Components;
using Persistence.DTO.Product;

namespace Presentation.Components
{
    /// <summary>
    /// Used for dispalying the products
    /// </summary>
    public partial class ProductComponent
    {
        /// <summary>
        /// The product received form the parent page to be displayed
        /// </summary>
        [Parameter]
        public ProductDto? Product { get; set; }

        /// <summary>
        /// The action sidebar of the product that changes for every case
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }
    }
}