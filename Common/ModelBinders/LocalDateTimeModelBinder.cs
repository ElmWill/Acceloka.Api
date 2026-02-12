namespace Acceloka.Api.Common.ModelBinders;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using NodaTime;
using System.Globalization;

public class LocalDateTimeModelBinder : IModelBinder
{
    private readonly string[] _formats =
    {
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd HH:mm:ss",
        "dd-MM-yyyy HH:mm",
        "dd-MM-yyyy HH:mm:ss"
    };

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider
            .GetValue(bindingContext.ModelName)
            .FirstValue;

        if (string.IsNullOrWhiteSpace(value))
            return Task.CompletedTask;

        if (DateTime.TryParseExact(
            value,
            _formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dateTime))
        {
            var local = LocalDateTime.FromDateTime(dateTime);
            bindingContext.Result = ModelBindingResult.Success(local);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"Invalid date format. Use yyyy-MM-dd HH:mm");
        }

        return Task.CompletedTask;
    }
}
