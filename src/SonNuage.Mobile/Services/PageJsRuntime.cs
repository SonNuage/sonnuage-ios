using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SonNuage.Mobile.Services;

/// <summary>
/// IJSRuntime de la page pour les singletons (le lecteur) : la webview ne crée le sien qu'à l'ouverture de la page.
/// Les appels attendent donc <see cref="Attach"/> ; une page rechargée remplace le runtime. Même principe que l'appli de bureau.
/// </summary>
public sealed class PageJsRuntime : IJSRuntime
{
    private TaskCompletionSource<IJSRuntime> _page = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Attach(IJSRuntime js)
    {
        if (_page.Task.IsCompleted) _page = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _page.SetResult(js);
    }

    public async ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, object?[]? args) =>
        await (await _page.Task).InvokeAsync<TValue>(identifier, args);

    public async ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
        await (await _page.Task).InvokeAsync<TValue>(identifier, cancellationToken, args);
}

/// <summary>Composant racine invisible : donne l'IJSRuntime de la page à <see cref="PageJsRuntime"/>.</summary>
public sealed class JsBridge : ComponentBase
{
    [Inject] private IJSRuntime Js { get; set; } = default!;
    [Inject] private PageJsRuntime Page { get; set; } = default!;

    protected override void OnInitialized() => Page.Attach(Js);
}
