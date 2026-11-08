using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using SonNuage.Mobile.Services;
using Xunit;

namespace SonNuage.Mobile.Tests;

public class PageJsRuntimeTests
{
    [Fact]
    public async Task Call_made_before_the_page_opens_waits_for_it()
    {
        var runtime = new PageJsRuntime();
        var call = runtime.InvokeAsync<string>("player.state", []).AsTask();
        Assert.False(call.IsCompleted);

        runtime.Attach(new FakePage("page 1"));

        Assert.Equal("page 1:player.state", await call.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Reloaded_page_receives_the_next_calls()
    {
        var runtime = new PageJsRuntime();
        runtime.Attach(new FakePage("page 1"));
        runtime.Attach(new FakePage("page 2"));

        Assert.Equal("page 2:player.play", await runtime.InvokeAsync<string>("player.play", CancellationToken.None, []));
    }

    private sealed class FakePage(string name) : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, object?[]? args) =>
            ValueTask.FromResult((TValue)(object)$"{name}:{identifier}");

        public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);
    }
}
