/**************************************************************************
 *                                                                        *
 *  Description: UserRepository                                           *
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
using Persistence.DTO.User;
using Persistence.Mappers;

namespace Persistence.DAO.Repositories;

internal class UserRepository(DatabaseContext dbContext) : IUserRepository
{
    public Result<VoidResult, DaoErrorType> RegisterUser(UserRegisterDto userDtoRegisterDto)
    {
        try
        {
            if (GetUser(userDtoRegisterDto.Username).IsSuccess)
                return Result<VoidResult, DaoErrorType>.Fail(
                    DaoErrorType.Duplicate,
                    $"User {userDtoRegisterDto.Username} already used."
                );

            var user = MapperDto.MapToUser(userDtoRegisterDto);
            dbContext.Users.Add(user);
            dbContext.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            return Result<VoidResult, DaoErrorType>.Fail(
                DaoErrorType.DatabaseError,
                $"User {userDtoRegisterDto.Username} could not be registered: {e.Message} "
            );
        }

        return Result<VoidResult, DaoErrorType>.Success(VoidResult.Get(),
            $"User {userDtoRegisterDto.Username} successfully registered.");
    }

    public Result<VoidResult, DaoErrorType> UpdateUser(string username, UserRegisterDto userDtoRegisterDto)
    {
        try
        {
            var existingUser = dbContext.Users.Include(user => user.BillDetails)
                .FirstOrDefault(user => user.Username == username);

            if (existingUser == null)
            {
                return Result<VoidResult, DaoErrorType>.Fail(
                    DaoErrorType.NotFound,
                    $"User {userDtoRegisterDto.Username} not found."
                );
            }
            
            if (userDtoRegisterDto.Username != "") existingUser.Username = userDtoRegisterDto.Username;
            if (userDtoRegisterDto.Password != "") existingUser.Password = userDtoRegisterDto.Password;
            if (userDtoRegisterDto.Email != "") existingUser.Email = userDtoRegisterDto.Email;
            if (userDtoRegisterDto.FirstName != "") existingUser.FirstName = userDtoRegisterDto.FirstName;
            if (userDtoRegisterDto.LastName != "") existingUser.LastName = userDtoRegisterDto.LastName;

            dbContext.Update(existingUser);
            dbContext.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            return Result<VoidResult, DaoErrorType>.Fail(
                DaoErrorType.DatabaseError,
                $"User {userDtoRegisterDto.Username} cannot be updated: {e.Message}."
            );
        }

        return Result<VoidResult, DaoErrorType>.Success(VoidResult.Get(),
            $"User {username} updated successfully: {userDtoRegisterDto}");
    }

    public Result<VoidResult, DaoErrorType> DeleteUser(string username)
    {
        try
        {
            var existingUser = dbContext.Users
                .Include(user => user.BillDetails)
                .Include(user => user.OrderSessions)
                .FirstOrDefault(user => user.Username == username);

            if (existingUser == null)
            {
                return Result<VoidResult, DaoErrorType>.Fail(
                    DaoErrorType.NotFound,
                    $"User {username} could not be deleted because is not registered. "
                );
            }

            dbContext.Users.Remove(existingUser);
            dbContext.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            return Result<VoidResult, DaoErrorType>.Fail(
                DaoErrorType.DatabaseError,
                $"User {username} cannot be updated: {e}."
            );
        }

        return Result<VoidResult, DaoErrorType>.Success(VoidResult.Get(), $"User {username} updated successfully.");
    }

    public Result<List<UserInfoDto>, DaoErrorType> GetAllUsers()
    {
        var users = new List<UserInfoDto>();
        dbContext.Users.Include(user => user.BillDetails).ToList()
            .ForEach(u =>
            {
                dbContext.Entry(u).Reload();
                users.Add(MapperDto.MapToBillUserDto(u)!);
            });

        return users.Count != 0
            ? Result<List<UserInfoDto>, DaoErrorType>.Success(users, "Users list returned.")
            : Result<List<UserInfoDto>, DaoErrorType>.Fail(DaoErrorType.ListIsEmpty, "No user found.");
    }

    public Result<UserInfoDto, DaoErrorType> GetUser(string username)
    {
        var billUserDto = MapperDto.MapToBillUserDto(
            dbContext.Users
                .Include(user => user.BillDetails)
                .FirstOrDefault(u => u.Username == username)
        );

        return billUserDto == null
            ? Result<UserInfoDto, DaoErrorType>.Fail(DaoErrorType.NotFound, $"User {username} not found")
            : Result<UserInfoDto, DaoErrorType>.Success(billUserDto, $"User {username} found");
    }


    public Result<string, DaoErrorType> GetUserPassword(string username)
    {
        var password = dbContext.Users.FirstOrDefault(u => u.Username == username)?.Password;
        return password == null
            ? Result<string, DaoErrorType>.Fail(DaoErrorType.NotFound,
                $"Password for user {username} not found")
            : Result<string, DaoErrorType>.Success(password, $"User {username} password found");
    }

    public Result<string, DaoErrorType> GetUserType(string username)
    {
        var userType = dbContext.Users.FirstOrDefault(u => u.Username == username)?.UserType;
        return userType == null
            ? Result<string, DaoErrorType>.Fail(DaoErrorType.NotFound,
                $"User type for user {username} not found")
            : Result<string, DaoErrorType>.Success(userType, $"UserType for {username} returned.");
    }
}