using SonNuage.Mobile.Services;
using Xunit;

namespace SonNuage.Mobile.Tests;

public class ServerAddressTests
{
    [Theory]
    [InlineData("http://192.168.1.10:8080", "http://192.168.1.10:8080/")]
    [InlineData("https://music.example.org/sonnuage", "https://music.example.org/sonnuage/")]
    [InlineData("https://music.example.org/sonnuage/", "https://music.example.org/sonnuage/")]
    public void Base_address_ends_with_a_slash(string typed, string expected) =>
        Assert.Equal(new Uri(expected), ServerAddress.AsBase(new Uri(typed)));

    [Fact]
    public void Relative_api_path_resolves_under_the_server_path()
    {
        var api = new Uri(ServerAddress.AsBase(new Uri("https://music.example.org/sonnuage")), "api/tracks");
        Assert.Equal("https://music.example.org/sonnuage/api/tracks", api.ToString());
    }
}
