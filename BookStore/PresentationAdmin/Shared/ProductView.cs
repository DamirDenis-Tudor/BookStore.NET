﻿using Microsoft.AspNetCore.Components;
using Persistence.DTO.Product;

namespace PresentationAdmin.Shared
{
    public partial class ProductView
    {
        [Parameter]
        public ProductStatsDto? Product { get; set; }
    }
}