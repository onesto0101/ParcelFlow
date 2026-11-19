using ParcelFlow.Modules.Shipments.Domain;

namespace ParcelFlow.Modules.Shipments.UnitTests;

public class AddressTests
{
    [Fact]
    public void Constructor_TrimsAndNormalizes()
    {
        var address = new Address("  Calle 1  ", " Monterrey ", "NL", "64000", "mx");

        Assert.Equal("Calle 1", address.Street);
        Assert.Equal("Monterrey", address.City);
        Assert.Equal("MX", address.Country);
    }

    [Theory]
    [InlineData("", "Monterrey", "NL", "64000", "MX", nameof(Address.Street))]
    [InlineData("Calle 1", " ", "NL", "64000", "MX", nameof(Address.City))]
    [InlineData("Calle 1", "Monterrey", "", "64000", "MX", nameof(Address.State))]
    [InlineData("Calle 1", "Monterrey", "NL", "64", "MX", nameof(Address.PostalCode))]
    [InlineData("Calle 1", "Monterrey", "NL", "64#00", "MX", nameof(Address.PostalCode))]
    [InlineData("Calle 1", "Monterrey", "NL", "64000", "MEX", nameof(Address.Country))]
    [InlineData("Calle 1", "Monterrey", "NL", "64000", "M1", nameof(Address.Country))]
    public void Constructor_InvalidField_ThrowsWithParamName(
        string street, string city, string state, string postalCode, string country, string expectedParam)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new Address(street, city, state, postalCode, country));

        Assert.Equal(expectedParam, exception.ParamName);
    }

    [Fact]
    public void Constructor_StreetTooLong_ThrowsException()
    {
        var longStreet = new string('a', Address.StreetMaxLength + 1);

        Assert.Throws<ArgumentException>(() => new Address(longStreet, "Monterrey", "NL", "64000", "MX"));
    }

    [Fact]
    public void TwoAddresses_WithSameData_AreEqual()
    {
        Assert.Equal(TestData.Address(), TestData.Address());
    }
}

public class ContactTests
{
    [Theory]
    [InlineData("81 1234 5678", "8112345678")]
    [InlineData("(81) 1234-5678", "8112345678")]
    [InlineData("+52 81 1234 5678", "+528112345678")]
    public void Constructor_Phone_IsNormalized(string input, string expected)
    {
        var contact = new Contact("Ana López", input, TestData.Address());

        Assert.Equal(expected, contact.Phone);
    }

    [Theory]
    [InlineData("123456")]              // 6 dígitos: muy corto
    [InlineData("1234567890123456")]    // 16 dígitos: muy largo
    [InlineData("81-ABCD-5678")]        // letras
    [InlineData("81+12345678")]         // "+" en medio
    public void Constructor_InvalidPhone_ThrowsException(string phone)
    {
        Assert.Throws<ArgumentException>(() => new Contact("Ana López", phone, TestData.Address()));
    }

    [Fact]
    public void Constructor_NameTooShort_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Contact("A", "8112345678", TestData.Address()));
    }

    [Fact]
    public void Constructor_NullAddress_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => new Contact("Ana López", "8112345678", null!));
    }

    [Fact]
    public void TwoContacts_WithSameData_AreEqual()
    {
        // Contact contiene un Address (otro record): la igualdad compara también por valor "en profundidad".
        Assert.Equal(TestData.Contact(), TestData.Contact());
    }
}
