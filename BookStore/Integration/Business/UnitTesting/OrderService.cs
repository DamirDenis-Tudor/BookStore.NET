using Business.BAL;
using Business.BTO;
using Business.Mappers;
using Persistence.DAL;
using Persistence.DTO.Product;
using Persistence.DTO.User;

namespace UnitTesting.Business.UnitTesting;

public class OrderService
{
    private BusinessFacade _businessFacade;
    private OrderBto _validOrderBto;
    private UserRegisterDto _clientUser;
    private UserRegisterDto _adminUser;
    private UserRegisterDto _providerUser;

    [OneTimeSetUp]
    public void PrepareDatabase()
    {
        PersistenceFacade.Instance.SetIntegrationMode(IntegrationMode.Testing);
        _businessFacade = BusinessFacade.Instance;

        _adminUser = new UserRegisterDto
        {
            FirstName = "AdminUser", LastName = "AdminUser", Username = "AdminUser_123",
            Password = "adminUser_123", Email = "admin@admin.com", UserType = "ADMIN"
        };
        var gdprAdmin = GdprMapper.DoUserInfoDtoGdpr(_adminUser);
        Assert.That(PersistenceFacade.Instance.UserRepository.RegisterUser(gdprAdmin).IsSuccess, Is.EqualTo(true));

        _clientUser = new UserRegisterDto
        {
            FirstName = "ClientUser", LastName = "ClientUser", Username = "clientUser_123",
            Password = "client_123", Email = "client@client.com", UserType = "CLIENT"
        };
        Assert.That(_businessFacade.UsersService.RegisterClient(_clientUser).IsSuccess, Is.EqualTo(true));

        _providerUser = new UserRegisterDto
        {
            FirstName = "ProviderUser", LastName = "ProviderUser", Username = "provider_123",
            Password = "provider_123", Email = "provider@provider.com", UserType = "PROVIDER"
        };
        Assert.That(_businessFacade.UsersService.RegisterProvider(_adminUser.Username, _providerUser).IsSuccess,
            Is.EqualTo(true));

        _businessFacade.InventoryService.RegisterProduct(_providerUser.Username,
            new ProductDto
            {
                ProductInfoDto = new ProductInfoDto { Name = "TEST", Description = "TEST", Category = "TEST" },
                Price = 1, Quantity = 1,
            });
        _businessFacade.InventoryService.RegisterProduct(_providerUser.Username,
            new ProductDto
            {
                ProductInfoDto = new ProductInfoDto { Name = "TEST1", Description = "TEST1", Category = "TEST1" },
                Price = 1, Quantity = 1,
            });
        _businessFacade.InventoryService.RegisterProduct(_providerUser.Username,
            new ProductDto
            {
                ProductInfoDto = new ProductInfoDto { Name = "TEST2", Description = "TEST2", Category = "TEST2" },
                Price = 1, Quantity = 1,
            });

        _validOrderBto = new OrderBto
        {
            Username = _clientUser.Username,
            OrderItemBtos =
            [
                new OrderItemBto { ProductName = "TEST", OrderQuantity = 1 },
                new OrderItemBto { ProductName = "TEST1", OrderQuantity = 1 },
                new OrderItemBto { ProductName = "TEST2", OrderQuantity = 1 },
            ]
        };
    }

    [Test, Order(1)]
    public void PlaceOrder()
    {
        var placeOrder = _businessFacade.OrderService.PlaceOrder(_validOrderBto);
        Assert.That(placeOrder.IsSuccess, Is.EqualTo(true));

        _businessFacade.OrderService.GetUserOrders(_clientUser.Username).SuccessValue.ToList()
            .ForEach(Console.WriteLine);
    }

    [Test, Order(2)]
    public void GetOrders()
    {
        var orders = _businessFacade.OrderService.GetUserOrders(_clientUser.Username);
        Assert.That(orders.SuccessValue.Count, Is.EqualTo(1));
    }
}