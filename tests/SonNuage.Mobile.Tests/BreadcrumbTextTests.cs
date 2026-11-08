using SonNuage.Mobile.Services;
using Xunit;

namespace SonNuage.Mobile.Tests;

public class BreadcrumbTextTests
{
    [Fact]
    public void Short_message_is_kept_whole() => Assert.Equal("ok", BreadcrumbText.Shorten("ok"));

    [Fact]
    public void Message_at_the_limit_is_kept_whole()
    {
        var message = new string('a', BreadcrumbText.MaxLength);
        Assert.Equal(message, BreadcrumbText.Shorten(message));
    }

    [Fact]
    public void Long_message_is_cut_with_an_ellipsis()
    {
        var shortened = BreadcrumbText.Shorten(new string('a', BreadcrumbText.MaxLength + 1));
        Assert.Equal(new string('a', BreadcrumbText.MaxLength) + "…", shortened);
    }
}
