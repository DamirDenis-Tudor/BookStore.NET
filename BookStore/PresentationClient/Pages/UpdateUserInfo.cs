﻿/**************************************************************************
 *                                                                        *
 *  File:        PaymentDetails.cs                                        *
 *  Copyright:   (c) 2024, Asmarandei Catalin                             *
 *  Website:     https://github.com/DamirDenis-Tudor/BookStore.NET        *
 *  Description: The page where the user can modify his account           *
 *      informations                                                      *
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
using Persistence.DTO.User;
using System.ComponentModel.DataAnnotations;
using PresentationClient.Services;
using Presentation.Services;

namespace PresentationClient.Pages
{
    /// <summary>
    /// Take the infroamtions about the account from the database, check for changes and update them
    /// </summary>
    public partial class UpdateUserInfo
    {
        /// <summary>
        /// The navigation manager for redirecting the user
        /// </summary>
        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        /// <summary>
        /// The business facade singleton
        /// </summary>
        [Inject]
        public BusinessFacade Business { get; set; } = null!;

        /// <summary>
        /// The user login service for getting the token of the user
        /// </summary>
        [Inject]
        public IUserLoginService UserData { get; set; } = null!;

		/// <summary>
		/// The error message that will be displayed if the loggin fails
		/// </summary>
		private string _loggingError = "";
		/// <summary>
		/// The success message that will be displayed if the loggin is successful
		/// </summary>
		private string _loggingSuccess = "";


		/// <summary>
		/// The account information that the user will get in the form
		/// </summary>
		public class UserInfoData
        {
            [Required] public string FirstName { get; set; } = null!;

            [Required] public string LastName { get; set; } = null!;

            [Required, MinLength(4, ErrorMessage = "Username-ul trebuie sa fie mai lung de 4 caractere")]
            public string Username { get; set; } = null!;

            public string Password { get; set; } = "";

            [Required] public string Email { get; set; } = null!;

            /// <summary>
            /// Mappes the user informations from a DTO to the object properties
            /// <see cref="UserInfoDto"/>
            /// </summary>
            /// <param name="dto">The DTO object that will be mapped from</param>
            public void Deserialize(UserInfoDto dto)
            {
                FirstName = dto.FirstName;
                LastName = dto.LastName;
                Username = dto.Username;
                Email = dto.Email;
            }

            /// <summary>
            /// Mappes the user informations from the object properties to a DTO
            /// </summary>
            /// <returns>The resulted <see cref="UserInfoDto"/> object</returns>
            public UserRegisterDto ConverToDto()
            {
                return new UserRegisterDto()
                {
                    Email = Sanitizer.SanitizeString(Email),
                    FirstName = Sanitizer.SanitizeString(FirstName),
                    LastName = Sanitizer.SanitizeString(LastName),
                    Password = Sanitizer.SanitizeString(Password),
                    Username = Sanitizer.SanitizeString(Username),
                    UserType = "CLIENT"
                };
            }
        }

        /// <summary>
        /// The user information that is mapped to the form
        /// </summary>
        private readonly UserInfoData _user = new();

        /// <summary>
        /// Gets the user informations from the database and mappes them to the form
        /// </summary>
        /// <param name="firstRender">If the page is rendered for the first time</param>
        /// <returns>Async Task</returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            var sessionToken = await UserData.GetToken();

            if (sessionToken == null) return;

            var username = Business.AuthService.GetUsername(sessionToken);
            if (!username.IsSuccess) return;

            var result = Business.UsersService.GetUserInfo(username.SuccessValue);
            if (!result.IsSuccess) return;

            _user.Deserialize(result.SuccessValue);
            StateHasChanged();
        }

        /// <summary>
        /// Event called when the user submits the update account information form
        /// If there is any difference between the information stored in the database and the ones entered by the user, 
        /// the information is updated.
        /// The user is logged out (in case he updates his username or password) and redirected to the login page
        /// </summary>
        /// <param name="editContext"></param>
        private async void RegisterSubmit(EditContext editContext)
        {
            if (!editContext.Validate()) return;

            var sessionToken = await UserData.GetToken();

            if (sessionToken == null) return;

            var username = Business.AuthService.GetUsername(sessionToken);
            var remoteInfo = Business.UsersService.GetUserInfo(username.SuccessValue);

            if (!DifferenceBillDetails(remoteInfo.SuccessValue, _user)) return;

            var result = Business.UsersService.UpdateUser(username.SuccessValue, _user.ConverToDto());

            if(!result.IsSuccess)
            {
				_loggingError = result.Message;
                return;
			}
			_loggingSuccess = result.Message;
			Business.AuthService.Logout(sessionToken);
            NavigationManager.NavigateTo("/", true);
        }

        /// <summary>
        /// Checks if two <see cref="UserInfoDto"/> objects have different between informations
        /// </summary>
        /// <param name="remote">The first object for check</param>
        /// <param name="local">The secound object for check</param>
        /// <returns>true If there is any difference</returns>
        private static bool DifferenceBillDetails(UserInfoDto remote, UserInfoData local)
        {
            return remote.Email != local.FirstName ||
                   remote.FirstName != local.FirstName ||
                   remote.LastName != local.LastName ||
                   remote.Username != local.Username;
        }

		/// <summary>
		/// If the user has a invalidation message of the loggin and he changes the value of the field the message will be cleared
		/// </summary>
		/// <param name="sender">The form</param>
		/// <param name="e">The event raised for field changed</param>
		private void OnFieldChange(object? sender, FieldChangedEventArgs e)
		{
			_loggingError = "";
			_loggingSuccess = "";
		}
	}
}