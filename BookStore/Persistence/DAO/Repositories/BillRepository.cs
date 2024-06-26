/**************************************************************************
 *                                                                        *
 *  Description: BillRepository                                           *
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
using Persistence.DTO.Bill;
using Persistence.Mappers;

namespace Persistence.DAO.Repositories;

internal class BillRepository(DatabaseContext dbContext) : IBillRepository
{
    public Result<VoidResult, DaoErrorType> UpdateBillByUsername(string username, BillDto billDto)
    {
        try
        {
            var billDetails = dbContext.Bills.Include(b => b.User)
                .FirstOrDefault(b => b.User != null && b.User.Username == username);

            if (billDetails == null)
                return Result<VoidResult, DaoErrorType>.Fail(DaoErrorType.NotFound, $"BillDetails not found for {username}.");

            if (!string.IsNullOrEmpty(billDto.Address)) billDetails.Address = billDto.Address;
            if (!string.IsNullOrEmpty(billDto.Country)) billDetails.Country = billDto.Country;
            if (!string.IsNullOrEmpty(billDto.PostalCode)) billDetails.PostalCode = billDto.PostalCode;
            if (!string.IsNullOrEmpty(billDto.Telephone)) billDetails.Telephone = billDto.Telephone;
            if (!string.IsNullOrEmpty(billDto.City)) billDetails.City = billDto.City;

            dbContext.Entry(billDetails.User!).State = EntityState.Detached;
            dbContext.Update(billDetails);
            dbContext.SaveChanges();

            return Result<VoidResult, DaoErrorType>.Success(VoidResult.Get());
        }
        catch (DbUpdateException e)
        {
            return Result<VoidResult, DaoErrorType>.Fail(DaoErrorType.DatabaseError,
                $"Database error occurred while updating bill: {e.Message}");
        }
    }

    public Result<BillDto, DaoErrorType> GetBillingDetails(string username)
    {
        var userBillDetails = MapperDto.MapToBillDto(
            dbContext.Users
                .Include(user => user.BillDetails)
                .FirstOrDefault(u => u.Username == username)?.BillDetails);

        return userBillDetails == null
            ? Result<BillDto, DaoErrorType>.Fail(DaoErrorType.NotFound,
                $"Billing details for user {username} not found")
            : Result<BillDto, DaoErrorType>.Success(userBillDetails, $"User {username} has billing details.");
    }
}