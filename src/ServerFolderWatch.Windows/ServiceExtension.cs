using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.DependencyInjection;

namespace ServerFolderWatch.Windows;

[MarkupExtensionReturnType(typeof(object))]
public sealed class ServiceExtension : MarkupExtension
{
    public Type ServiceType { get; set; } = null!;

    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            return null;

        if (ServiceType is null)
            throw new InvalidOperationException(
                $"{nameof(ServiceType)} must be specified.");

        if (Application.Current is not App app)
            throw new InvalidOperationException(
                "The service extension requires the application DI container.");

        return app.Services.GetRequiredService(ServiceType);
    }
}
