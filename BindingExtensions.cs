using Ninject.Syntax;

namespace CustomScope
{
    public static class BindingExtensions
    {
        public static IBindingNamedWithOrOnSyntax<T> InCustomScope<T>(this IBindingInSyntax<T> bindingSyntax)
        {
            return bindingSyntax.InScope(CustomScope.ResolutionRootFromRequest);
        }
    }
}