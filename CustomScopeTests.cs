using Ninject;
using Ninject.Syntax;
using NUnit.Framework;

namespace CustomScope
{
    internal interface IService { }

    internal class Service : IService { }

    internal class ComplexClass
    {
        public ServiceUser InjectedServiceUser { get; private set; }
        public ServiceUser ResolvedServiceUser { get; private set; }

        public ComplexClass(ServiceUser serviceUser, IResolutionRoot resRoot)
        {
            InjectedServiceUser = serviceUser;
            ResolvedServiceUser = resRoot.Get<ServiceUser>();
        }
    }

    internal class ServiceUser
    {
        public IService Service { get; private set; }

        public ServiceUser(IService service)
        {
            Service = service;
        }
    }

    [TestFixture]
    public class CustomScopeTests
    {
        private StandardKernel _mainKernel;

        [SetUp]
        public void SetUp()
        {
            _mainKernel = new StandardKernel();
            _mainKernel.Bind<IService>().To<Service>().InCustomScope();
            _mainKernel.Bind<IResolutionRoot>().ToMethod(CustomScope.ResolutionRootFromRequest);
        }

        [Test]
        public void ParentKernelReturnsSingletonWhenBoundToKernelScope()
        {
            var a = _mainKernel.Get<IService>();
            var b = _mainKernel.Get<IService>();

            Assert.That(a, Is.SameAs(b));
        }

        [Test]
        public void SameScopeSameService()
        {
            IResolutionRoot customScope = new CustomScope(_mainKernel);

            var a = customScope.Get<IService>();
            var b = customScope.Get<IService>();

            Assert.That(a, Is.SameAs(b));
        }

        [Test]
        public void ScopeServiceDifferentFromDefaultService()
        {
            IResolutionRoot customScope = new CustomScope(_mainKernel);

            var a = _mainKernel.Get<IService>();
            var b = customScope.Get<IService>();

            Assert.That(a, Is.Not.SameAs(b));
        }

        [Test]
        public void DifferentExplicitScopes()
        {
            IResolutionRoot customScope1 = new CustomScope(_mainKernel);
            IResolutionRoot customScope2 = new CustomScope(_mainKernel);

            var a = customScope1.Get<IService>();
            var b = customScope2.Get<IService>();

            Assert.That(a, Is.Not.SameAs(b));
        }

        [Test]
        public void DifferentExplicitScopesResolvedTwice()
        {
            IResolutionRoot customScope1 = new CustomScope(_mainKernel);
            IResolutionRoot customScope2 = new CustomScope(_mainKernel);

            var a = customScope1.Get<IService>();
            var b = customScope2.Get<IService>();

            Assert.That(customScope1.Get<IService>(), Is.SameAs(a));
            Assert.That(customScope2.Get<IService>(), Is.SameAs(b));
        }

        [Test]
        public void ScopesReturnSelfAsResolutionRoot()
        {
            IResolutionRoot customScope = new CustomScope(_mainKernel);

            var resRoot = customScope.Get<IResolutionRoot>();

            Assert.That(resRoot, Is.SameAs(customScope));
        }        
        
        [Test]
        public void KernelReturnsSelfAsResolutionRoot()
        {
            IResolutionRoot resRoot = _mainKernel.Get<IResolutionRoot>();

            Assert.That(resRoot, Is.SameAs(_mainKernel));
        }

        [Test]
        public void InjectedAndResolvedChildrenGetSameService()
        {
            IResolutionRoot kernelScope = new CustomScope(_mainKernel);

            var a = kernelScope.Get<ComplexClass>();

            Assert.That(a.InjectedServiceUser.Service,
                Is.SameAs(a.ResolvedServiceUser.Service));
        }

        [Test]
        public void ChildrenGetSameServiceButDifferFromResultFromOtherKernels()
        {
            var a = _mainKernel.Get<ComplexClass>();

            Assert.That(a.InjectedServiceUser.Service,
                Is.SameAs(a.ResolvedServiceUser.Service));
            
            IResolutionRoot kernelScope = new CustomScope(_mainKernel);

            var b = kernelScope.Get<ComplexClass>();

            Assert.That(b.InjectedServiceUser.Service,
                Is.SameAs(b.ResolvedServiceUser.Service));

            Assert.That(a.ResolvedServiceUser.Service,
                        Is.Not.SameAs(b.ResolvedServiceUser.Service));

            Assert.That(a.InjectedServiceUser.Service,
                Is.Not.SameAs(b.InjectedServiceUser.Service));
        }
    }
}