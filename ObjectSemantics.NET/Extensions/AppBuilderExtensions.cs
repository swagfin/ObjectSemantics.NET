using ObjectSemantics.NET;
using ObjectSemantics.NET.Logic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ObjectSemanticsCollectionExtensions
    {
        public static void AddObjectSemantics(this IServiceCollection services, ObjectSemanticsOptions objectSemanticsOptions)
        {
            services.AddSingleton<IObjectSemantics>(x => new ObjectSemanticsLogic(objectSemanticsOptions));
        }
    }
}
