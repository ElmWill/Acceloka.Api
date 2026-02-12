namespace Acceloka.Api.Common.ModelBinders;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using NodaTime;

public class LocalDateTimeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(LocalDateTime) ||
            context.Metadata.ModelType == typeof(LocalDateTime?))
        {
            return new LocalDateTimeModelBinder();
        }

        return null;
    }
}
