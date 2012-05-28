using System;
using System.Collections.Generic;
using System.Linq;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using Ninject.Syntax;

namespace CustomScope
{
    public class CustomScope : IResolutionRoot
    {
        private class ScopeParameter : Parameter
        {
            private readonly WeakReference _scope;
            public IResolutionRoot Scope { get { return _scope.Target as IResolutionRoot; } }

            public ScopeParameter(IResolutionRoot scope) 
                : base("£$%^", (c,t) => null, true)
            {
                _scope = new WeakReference(scope);
            }
        }

        private readonly IResolutionRoot _parent;

        public CustomScope(IResolutionRoot parent)
        {
            _parent = parent;
        }

        public bool CanResolve(IRequest request)
        {
            return _parent.CanResolve(request);
        }

        public IEnumerable<object> Resolve(IRequest request)
        {
            return _parent.Resolve(request);
        }

        public IRequest CreateRequest(Type service, Func<IBindingMetadata, bool> constraint, IEnumerable<IParameter> parameters, bool isOptional, bool isUnique)
        {
            var scopeParameter = new ScopeParameter(this);
            return new Request(service, constraint, new [] { scopeParameter }.Concat(parameters), null, isOptional, isUnique);
        }

        public static IResolutionRoot ResolutionRootFromRequest(IContext context)
        {
            var scopeParameter = context.Parameters.OfType<ScopeParameter>().FirstOrDefault();
            if(scopeParameter != null)
            {
                return scopeParameter.Scope;
            }
            return context.Kernel;
        }
    }
}