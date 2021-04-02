# Castle Windsor Changelog

## 5.1.1 (2020-12-08)

- Upgrade minimum Castle.Core version to 4.4.1 (@generik0, #576)

Bugfixes:
- Fix CollectionResolver to allow propagation of inline dependencies (@dvdwouwe, #562)
- Allow DefaultNamingSubSystem derivatives to invalidate the cache which was accidentally removed in 5.1.0 (@nativenolde, #569)
- Replace usage of obsolete Castle.Core.Internal.Lock (@generik0, #576)
- Fix dictionary bug when using XML configuration; A reference to list components inside a dictionary didn't work (@ni-mi, #575)

## 5.1.0 (2020-11-16)

Bugfixes:
- .NET Extensions' DependencyInjection:
  - Change WindsorServiceProviderFactory to follow SOLID behaviour (@generik0, #540)
  - Fix "An item with the same key has already been added" exception related to scoped lifestyle (@generik0, #547)
  - Fix issue using existing container with ASP.NET (@robertcoltheart, #548)
  - Extra extensions for BasedOnDescriptor (@ltines, #554)
  - Use generic registration instead of reflection (@robertcoltheart, #555)
  - Use the container from the method call, not the root container (@generik0, #558)
  - Add InvalidateCache to DependencyInjectionNamingSubsystem (@generik0, @twenzel, #556)

## 5.1.0-beta001 (2020-06-17)

Enhancements:
- .NET Extensions' DependencyInjection support via new `Castle.Windsor.Extensions.DependencyInjection` package (@ltines, #517)
- Enable explicitly specified null values to satisfy `System.Nullable<>` dependencies (@jnm2, #521)
- Embed icon in NuGet packages (@generik0, #520)

Bugfixes:
- Typed Factory: handle multiple calls to Dispose and Release after Dispose (@ivan-danilov. #458)

## 5.0.1 (2019-09-18)

Bugfixes:
- Fix `ProxyOptions` equality with additional interfaces (@DamirAinullin, #477)
- WCF Facility: Fix exception message (@DamirAinullin, #476)
- ASP.NET MVC Facility: Fix controller lookup to be case insensitive (@yitzchok, #480)
- ASP.NET Core Facility: `FrameworkDependencyResolver` must not throw NRE if dependency has no type (e.g. depending on a named component) (@dariuslf, #489)
- ASP.NET Core Facility: Register ViewComponents and TagHelpers correctly (@dariuslf, #489)
- ASP.NET Core Facility: Allow crosswiring multiple implementations of the same service (@dariuslf, #489)
- ASP.NET Core Facility: Treat TagHelper classes with `__Generated__` in their name (e.g. TagHelpers generated for ViewComponents) as framework classes (@dariuslf, #489)

## 5.0.0 (2019-02-12)

Bugfixes:
- Fixed first-chance HandlerException for optional parameters (@jnm2, #450)

## 5.0.0-beta001 (2018-10-26)

Enhancements:
- Upgraded to Castle.Core 4.2.0 to 4.3.1 (@fir3pho3nixx, #413)
- Created Castle.Facilities.AspNetCore facility to support ASP.NET Core web applications on .NET Core and .NET Framework (@fir3pho3nixx, #120)
- Created Castle.Facilities.AspNet.Mvc facility to support ASP.NET MVC web applications on .NET Framework (@fir3pho3nixx, #283)
- Created Castle.Facilities.AspNet.WebApi facility to support ASP.NET Web API IIS and self hosted applications on .NET Framework (@fir3pho3nixx, #283)
- Added XML documentation to BeginScope and RequireScope lifetime extensions (@jonorossi)
- Upgraded build to use NUnit Adapters (@fir3pho3nixx, #243)
- Make formatting of type names with `TypeUtil.ToCSharpString` (and hence in diagnostic messages) resemble C# more closely (@stakx, #404, #406)

Breaking Changes:
- Built-in System.Web support has been moved to the new Castle.Facilities.AspNet.SystemWeb facility (@fir3pho3nixx, #283)
- Removed obsolete ActAs, Parameters, Properties and ServiceOverrides methods from component registration (@fir3pho3nixx, #338)
- Removed obsolete indexer, AddComponent*, AddFacility and Resolve methods from IKernel and IWindsorContainer (@fir3pho3nixx, #338)
- Facility XML configuration specifying an 'id' attribute will now throw, it has been ignored since v3.0 (@fir3pho3nixx, #338)
- Removed deprecated classes `AllTypes` and `AllTypesOf` (@fir3pho3nixx, #338)
- Removed deprecated `BasedOn` methods that reset registrations when fluently chained (@fir3pho3nixx, #338)
- Removed deprecated member `LifestyleHandlerType` on `CustomLifestyleAttribute` (@fir3pho3nixx, #338)
- Removed Event Wiring, Factory Support and Synchronize facilities (@jonorossi, #403)
- Arguments class and Resolve overloads refactor (@fir3pho3nixx, @jonorossi, #444)
  - Removed `WindsorContainer.Resolve(object/IDictionary)` overloads in favour of new `WindsorContainer.Resolve(Arguments)`
  - Reworked `Arguments` class, including to no longer implement `IDictionary`
  - Removed `IArgumentsComparer[]` constructors from `Arguments`
  - Added `WindsorContainer.Resolve(IEnumerable<KeyValuePair<string, object>>)` extension methods
  - Changed `CreationContext.AdditionalArguments` to use `Arguments` instead of `IDictionary`
  - Replaced `ComponentDependencyRegistrationExtensions(Insert, InsertAnonymous, InsertTyped, InsertTypedCollection)` with `Add`, `AddNamed` and `AddTyped` `Arguments` instance methods
  - Changed `ComponentRegistration.DependsOn` and `ComponentRegistration.DynamicParameters` to use `Arguments` via `DynamicParametersDelegate`
  - Added `ComponentRegistration.DependsOn(Arguments)` overload
  - Changed `ComponentModel` `CustomDependencies` and `ExtendedProperties` to use `Arguments` instead of `IDictionary`
  - Changed `IComponentModelBuilder.BuildModel` to use `Arguments` instead of `IDictionary`
  - Changed `ILazyComponentLoader.Load` to use `Arguments` instead of `IDictionary`

## 4.1.1 (2018-10-15)

Bugfixes:
- Fixed components resolved from typed factories being disposed along with unrelated objects (@jnm2, #439)

## 4.1.0 (2017-09-28)

Bugfixes:
- Fix warnings regarding non-existent `System.ComponentModel.TypeConverter` NuGet package by updating minimum Castle Core version to 4.1.0 (#321)
- Fix disposal of faulted WCF client channels (@jberezanski, #322)
- Fix binding errors because assembly version had too much detail, assembly version is now x.0.0.0 (@fir3pho3nixx, #329)
- Update Castle Core to 4.2.0 to resolve assembly version problems because Castle Core also had too much detail
- Explicit package versioning applied within solution to avoid maligned NuGet upgrades for lock step versioned packages (@fir3pho3nixx, https://github.com/castleproject/Core/issues/292)
- Fix open generic handler state issues where wrong constructor gets chosen for open generic service types (@fir3pho3nixx, #136)
- Fixed typed factory out of order disposal (@jnm2, #344)

Deprecations:
- Logging Facility's `LoggerImplementation` enum, `UseLog4Net` and `UseNLog` methods are deprecated in favour of `LogUsing<T>`, this includes the `loggingApi` property for XML configuration (@jonorossi, #327)

## 4.0.0 (2017-07-12)

Breaking Changes:
- Remove .NET 3.5, .NET 4.0 and .NET 4.0 Client Profile support (@fir3pho3nixx, #173, #180, #177, #185)
- Update Castle.Core dependency to 4.0.0 (@alinapopa, #235)
- Removed ActiveRecord, NHibernate and Remoting facilities (@jonorossi, #205)

Enhancements:
- Add .NET Standard and .NET Core support (@alinapopa, @fir3pho3nixx, @jonorossi, #145)

Bugfixes:
- Fix IL interpretation of `Ldarg_N` from LOCAL 0 to LOCAL [0,1] in OpCodes so test `FluentRegistrationTestCase.Can_publish_events_via_AllTypes` could publish events again on Windows 10 Home (build 14393.693) VS 2015 Update 3 using .NET 4.x (@fir3pho3nixx, #168)
- Fix race condition in PoolableLifestyleManager creating a pool (@krinmik, #72)
- Fix race condition in WindsorContainer not generating unique names (#301)

## 3.4.0 (2017-01-23)

- Fix case sensitivity issue that can cause UsingFactoryMethod to fail (@dohansen, #116)
- Fix project and icon URLs in NuGet packages
- Add PDB source indexing (@ivan-danilov, #137)
- Fix unit test with weak reference broken by garbage collector changes in .NET 4.6.x (@ivan-danilov, #138)
- Fix performance counter instances hanging around after the process using Windsor has ended (@mackenzieajudd, #146, #149)
- Fix version of Castle.Core dependency in NuGet packages to indicate Castle.Core 4.0 is incompatible (#161)

## 3.3.0 (2014-05-18)

- implemented #57 - build NuGet and Zip packages from TeamCity - contributed by Blair Conrad (@blairconrad)
- implemented #53 - Remove dependency on IKernel/IWindsorContainer from CallContextLifetimeScope
- implemented #52 - Add option to start startable components manually - based on pull request #37 from @jfojtl
- implemented #51 - CollectionResolved should support read-only collections in .NET 4.5+
- implemented #45 - use HttpApplication.RegisterModule in .NET 4.5 for PerWebRequest lifestyle - contributed by @BredStik
- fixed #59 - Fixed missing DuplexChannelBuilder GetChannel methods. - contributed by David Welch (@djwelch)
- fixed #50 - XML Config Array Parameters not populating when registering multiple dependencies - contributed by Dale Francis (@dalefrancis88)
- fixed #47 - Fixed 'consut' typo to 'consult'. - contributed by Mads Tjørnelund Toustrup (@madstt)
- fixed #38 - Pooled items not being disposed properly - contributed by @mvastarelli
- fixed #34 - Deadlock - DefaultNamingSubsystem.GetHandlers() vs DefaultGenericHandler.type2SubHandler - contributed by Anton Iermolenko (@anton-iermolenko)
- fixed #30 - Attempting to resolve a non-generic type with a backing generic implementation causes an exception to be thrown

## 3.2.1 (2013-07-22)

- fixed IOC-349 - SerializationException - Type is not resolved for member "Castle.MicroKernel.Lifestyle.Scoped.CallContextLifetimeScope+SerializationReference, ...

## 3.2.0 (2013-02-16)

- implemented IOC-375 - List duplicate registrations that are registered through convention
- implemented IOC-366 - support scoped lifestyle in XML config
- implemented IOC-365 - Provide convenience methods in the registration API to bind to nearest subgraph (in addition to existing - widest)
- implemented IOC-362 - New container diagnostic - duplicated dependencies
- implemented IOC-360 - Ability to register types based on one or several of multiple bases
- implemented IOC-358 - add ability to specify dependency on an embedded resource
- implemented IOC-357 - Provide some internal logging in Windsor to be able to see non-critical errors
- implemented IOC-355 - Add a mechanism to mark constructors as unselectable for injection, much like DoNotWireAttribute is for property injection
- implemented IOC-353 - Add Classes.FromAssemblyInThisApplication() for parity with FromAssembly class
- implemented IOC-348 - Explict sort order for convention-based configuration would be nice
- fixed FACILITIES-160 - Wcf Facility doesn't support multiple IErrorHandlers
- fixed IOC-374 - Container.Register causing NotSupportedException in ASP .NET MVC 4.
- fixed IOC-373 - Open Generics won't resolve with LateBoundComponent Implementation
- fixed IOC-372 - Performance Counters not updated on releasing a typed factory
- fixed IOC-371 - AssemblyFilter cannot find assemblies on case-sensitive filesystems
- fixed IOC-370 - Deadlock
- fixed IOC-369 - Probably bug with generic interceptors
- fixed IOC-364 - It is impossible to use bound lifestyle with chain of components binding to innermost one
- fixed IOC-359 - Property filtering API is confusing and buggy
- fixed IOC-356 - Specifying a hook should be enough to create an implementation-less proxy
- fixed IOC-354 - Deadlock in pooled lifestyle under heavy load
- fixed IOC-334 - FacilityConfig is null in facility Init()
- fixed IOC-321 - TypedFactory with singleton lifestyle and child containers
- fixed IOC-300 - OnCreate does not work for generic components

Breaking Changes:

change - the following methods were removed:
	IHandler IHandlerFactory.Create(ComponentModel model, bool isMetaHandler)
	IHandler IKernelInternal.AddCustomComponent(ComponentModel model, bool isMetaHandler)
	void IKernelInternal.RegisterHandler(String key, IHandler handler, bool skipRegistration)
	IHandler DefaultKernel.AddCustomComponent(ComponentModel model, bool isMetaHandler)
		the following methods were added:
	IHandler IKernelInternal.CreateHandler(ComponentModel model)
	void IKernelInternal.RaiseEventsOnHandlerCreated(IHandler handler)
reason - In order to avoid potential deadlocks that were possible when the container was
	starting and certain combination of open generic components was involved the API was
	modified to allow limiting the scope of locking when using open generic components.
issue - IOC-370 (http://issues.castleproject.org/issue/IOC-370)
fix - the changes occur in internal API and should not impact users. If you are impacted ask for
	help on the castle-users group on Google Groups.

change - .Properties() methods in registration API have changed behavior and are obsolete now.
	When calling .Properties() on a component multiple times, subsequent calls with now only
	be passed properties for which previuos calls returned false.
reason - The API was not behaving the way most users expected and the way it was structured
	it was hard to use.
issue - IOC-359 (http://issues.castleproject.org/issue/IOC-359)
fix - use either overload taking PropertyFilter enum, or one of the two new methods:
	PropertiesIgnore() and PropertiesRequire().

change - AbstractComponentActivator constructor takes IKernelInternal instead of IKernel now
reason - IKernelInternal exposes a logger which allows activators to log information about their behavior.
issue - IOC-359 (http://issues.castleproject.org/issue/IOC-357)
fix - update the signature of your custom activator to take IKernelInternal instead of IKernel.

## 3.1.0 (2012-08-05)

- fixed IOC-347 - WithServiceAllInterfaces throws exception (regression)

Breaking Changes:

change - Windsor will no longer allow components from parent container to have dependencies from
	child container when resolving via child container.
	Class ParentHandlerWithChildResolver was renamed to ParentHandlerWrapper
impact - low
fixability - medium
description - Previously in some cases, when resolving from child container Windsor would allow
	component from the parent container to depend on components from a child container.
	This would lead to all sorts of problems (child coomponents leaking to parent scope, parent
	components being released prematurely when disposing of the child container etc.
	Overall this behavior was a mess, and was removed.
	See http://issues.castleproject.org/issue/IOC-345 for more details
fix - If you were depending on the old behavior it is best to restructure your dependencies so
	you don't have to have those inverted dependencies.
	Since each scenario is different it's best to discuss any questions you may have on the user
	group.

change - IHandler.SupportsAssignable(Type) method has been added
impact - low
fixability - easy
description - This was added to better support IGenericServiceStrategy on generic handlers when
	calling IKernel.GetAssignableHandlers(Type). Now the handler can decide whether it wants to
	consider itself assigmable to given service.
fix - This change affects you only if you're implementing custom IHandler. Implementation is
	dependent on your usage and semantics you want to support for this scenario. When in doubt
	ask on castle-users-group on Google Groups.

change - System.String, and some other types can no longer be registered as a service
	in the container
impact - low
fixability - easy
description - This is something that probably should never have made it into the codebase. Now
	if you try to register String, a collection of strings or collection of value types Windsor
	will throw an ArgumentException and not allow you to do that.
fix - If you did register those types in the container change them from being components
	to being parameters on the components that were depending on them.

change - DependencyModel.IsValueType is renamed to DependencyModel.IsPrimitiveTypeDependency.
impact - low
fixability - easy
description - This is part of unification of how types that can not be registered as valid
	services are found and treated in Windsor.
	Also the property now returns true if TargetItemType is null. Previously it returned false.
fix - Change usages of IsValueType to IsPrimitiveTypeDependency if you depended on behavior when
	TargetItemType is null, you might also need to check its value to preserve the old behavior.

## 3.1.0 RC (2012-07-08)

- Refined WCF Discovery Load Balancing approach
- Added WCF behavior to specify an IDataContractSurrogate
- Import Bindings instead of endpoints for WCF Discovery
- Improved concurrency during WCF channel failover
- Refactored WCF proxying to support all channels (not just ones based on RealProxy)
- Added additional WCF Discovery support for managed Discovery Proxies
- Exposes notifications when channels are refreshed
- Added exponential backoff client policy for WCF Facility
- implemented IOC-343 - Add ability to specify fallback components that should never take precedence over non-fallback ones
- implemented IOC-339 - Add extension point to decide whether open generic component wants to support particular closed version
- implemented FACILITIES-159 - Add option to skip configuring log4net/nlog in LoggingFacility
- fixed IOC-345 - When using child containers, parent components are released with child container in certain cases
- fixed IOC-342 - Should error when trying to use PerWebRequest lifestyle when linked against the client profile
- fixed IOC-341 - IHandlerFilter returning empty array ignored
- fixed IOC-338 - SerializationException thrown when using remoting within default lifetime scope (Failed to load expression host assembly. Details: Type '[...]CallContextLifetimeScope[...]' is not marked as serializable.)
- fixed IOC-336 - Failing resolution of proxied components implementing multiple generic service interfaces
- fixed IOC-332 - UsingFactoryMethod resolving a proxy without a target throws NullReferenceException instead of a better exception
- fixed IOC-331 - TypedFactoryFacility should ignore Func<string>
- fixed IOC-328 - Hard-to-understand comment
- fixed IOC-326 - Component is Disposed before the OnDestroy delegate is invoked
- fixed IOC-325 - ParentHandlerWithChildResolver.TryResolve throws exception
- fixed IOC-241 - .NET 4 security transparency and APTCA
- fixed FACILITIES-155 - PerWcfSession throws NullReferenceException when not in a session

## 3.0.0 (2011-12-13)

no major changes

Breaking Changes:

change - Typed factory using DefaultTypedFactoryComponentSelector when resolving component 
	by name will not fallback to resolving by type if component with that name can not be found
	and will throw an exception instead.
id - typedFactoryFallbackToResolveByTypeIfNameNotFound
impact - medium
fixability - easy
description - Original behavior from v2.5 could lead to bugs in cases when named component was
	not registered or the name was misspelleed and a wrong component would be picked leading to
	potentially severe issues in the application. New version adapts fail-fast approach in those
	cases to give dvelopers immediate feedback the configuration is wrong.
fix - Actual fix depends on which part of the behavior you want:
	- If you do care about the fallback behavior, that is get the component by name and if
	not present fallback to resolve by type, you can specify it explicitly when registering your
	factory:
	.AsFactory(
		new DefaultTypedFactoryComponentSelector(fallbackToResolveByTypeIfNameNotFound: true));
	- if you don't care about the fallback and what you really want is a 'GetSomeFoo' method
	that resolves by type, either rename the method so that its name doesn't start with 'get'
	or disable the "'get' methods resolve by name" behavior explicitly when registering your
	factory:
	.AsFactory(new DefaultTypedFactoryComponentSelector(getMethodsResolveByName: false))

change - Referencing interceptors by type will not work if the interceptor has custom name.
impact - medium
fixability - easy
description - We unified how referencing components by type works all across Windsor and that
	introduced change for some areas like referencing interceptors. Now referencing component
	by type means "component implemented by given type with default name". This is how it worked
	for service overrides and is now adapted all across the framework.
fix - Remove Name (id in XML registration) from the referenced components if you're not using it
	or reference the component by its name.

change - .Service method on mixing registration has been removed and replaced with .Component.
impact - low
fixability - easy
description - The method had misleading name and behavior inconsistent with the rest of Windsor.
	As such it's been replaced with .Component method which is more explicit about what argument
	passed to it means
fix - Replace with .Component method:
Container.Register(Component.For<ICalcService>()
			        .ImplementedBy<CalculatorService>()
			        .Proxy.MixIns(m => m.Component<A>()));
	Notice the new method is behaving consistently with how referencing interceptors and service
	overrides works. So you may need to adjust generic argument to point to other component's
	implementation type rather than its exposed service.

change - Generic overloads of .Insert(this IDictionary dictionary, otherarguments) extension
	method have been removed.
impact - low
fixability - easy
description - The overload could cause unexpected behavior when the generic parameter was being
	inferred, and as such it is removed to make the type always explicit. 
fix - Use overload that specifies type explicitly:
	d.Insert(typeof(IFoo), new MyFoo()) instead of d.Insert<IFoo>(new MyFoo()) or new, explicit
	d.InsertTyped<IFoo>(new MyFoo())

change - Method object Generate(IProxyBuilder, ProxyGenerationOptions, IInterceptor[]) on type
	IProxyFactoryExtension changed signature.
impact - low
fixability - easy
description - To handle new scenarios two additional arguments were introduced:
	ComponentModel model and CreationContext context.
fix - If you were implementing IProxyFactory and calling down to IProxyFactoryExtension pass your
	own arguments down to IProxyFactoryExtension. If you're implementing IProxyFactoryExtension
	adjust your signature and if that makes sense in your context use the arguments.

change - ProxyUtil class was split and part moved to Castle.Core.dll and other was renamed
impact - low
fixability - easy
description - ProxyUtil contained logic useful not just in the context of Windsor. As such
	it was moved to be part of DynamicProxy and most methods are now part of the other assembly.
	The only method specific to Windsor: ObtainProxyOptions was left and is now an extension
	method in class ProxyOptionsUtil.
fix - If you were using ObtainProxyOptions use it either as extension method or update its type
	name to ProxyOptionsUtil. Remining methods are now part of ProxyUtil class which was moved
	to Castle.DynamicProxy namespaces and lives in Castle.Core.dll

change - CreateLifestyleManager method was moved from handlers to IKernelInternal
impact - low
fixability - easy
description - That behavior belongs in the kernel.
fix - You shouldn't be using this method unless you're implementing custom handlers. If you do
	call back to the kernel instead of implementing it in yoru handler.

change - Removed interface Castle.Core.ILifecycleConcern
impact - low
fixability - easy
description - This change was made because with this base interface it was impossible to
	implement Castle.Core.ICommisssionConcern and Castle.Core.IDecommissionConcers in single class
	Additionaly ILifecycleConcern had no meaning, only the ICommisssionConcern and
	IDecommissionConcers have
fix - If you have code using directly ILifecycleConcern (but what for?) you need to
	migrate to either ICommisssionConcern or IDecommissionConcers. For code that use
	ICommisssionConcern and IDecommisssionConcern you can recompile it to be extra save, but it
	is not required.

change - Removed overloads of Configure and ConfigureFor<> methods of the fluent registration
	API that had ConfigureDelegate parameter
impact - high
fixability - easy
description - This change was made to simplify the API and remove ambiguity in cases where a 
	private method is used to provide the configuration.
fix - This change breaks scenarios where a property was being used as the last element of the
	chain in the nested deledate, like:
	Configure(c => c.LifeStyle.Transient)
	This code will no longer compile. To fix it switch to the new methods exposing lifestyle:
	Configure(c => c.LifestyleTransient()) or simply::
	LifestyleTransient()

change - ITypedFactoryComponentResolver interface was removed and ITypedFactoryComponentSelector
	now returns Func<IKernelInternal, IReleasePolicy, object> from SelectComponent method
impact - low
fixability - easy
description - This change was made to simplify coding of advanced custom selectors which means
	now only one type needs to be created instead of two and change is much more localized.
fix - If you were using DefaultTypedFactoryComponentSelector this change does not affect you.
	otherwise return delegate pointing to Resolve method of your ITypedFactoryComponentResolver
	class or inline it altogether.

change - Add() methods on PropertySetCollection and ConstructorCandidateCollection are no longer
	publicly accessible
impact - low
fixability - easy
description - This change was made to ensure and encapsulate the fact that as constructor or
	property dependency is added the dependency is also added to Dependencies collection on
	ComponentModel.
fix - Use new AddProperty or AddConstructor methods respectively.

rename -  WithService.DefaultInterface() -> WithService.DefaultInterfaces()
description - changed to plural to emphasize more than one interface may be matched.

change - ResolveAll methods have now different bahaviour.
impact - high
fixability - medium
description - Previously Windsor when ResolveAll was called would try to resolve all components
	with implementation type assignable to the type requirested and silently ignore those it
	could not resolve. This behavior was introduced before Windsor had ability to support multi
	service components and at the time it was the only way to support certain scenarios.
	Currently this behavior is no longer required and is indeed leading to issues when dealing
	with code that doesn't strictly follow good OOP principles. Also by silently ignoring 
	unresolvable components it may mask registration issues, that's why it was changed.
fix - Now ResolveAll<Foo>() will only resolve components that explicitly expose Foo as their
	service. If you were depending on the implicit behavior previously, make sure you add all
	types you resolve via this method as service to the desired components.
	Also Windsor now will throw exception if any of the components can't be resolved. If you
	have a legitimate reason to have unresolvable component use IHandlersFilter to filter that
	components out.

change - The following methods were removed:
	IHandler.AddCustomDependencyValue
	IHandler.HasCustomParameter
	IHandler.RemoveCustomDependencyValue
	IHandler.OnHandlerStateChanged event
	IKernelInternal.RaiseHandlerRegistered
	IKernelInternal.RaiseHandlersChanged
	IKernelInternal.RegisterCustomDependencies (all 4 overloads)
impact - low
fixability - easy
description - Those members were remainings from the old era and there's no longer any point in
	having them.
fix - Pass the dependencies directly to the ComponentModel using DependsOn method on the fluent
	registration API. The OnHandlerStateChanged event would no longer be raised so there was no
	point in keeping it around either. Use HandlersChanged event on kernel instead.

change - IReference<out T>.Attach and .Detach method have now ComponentModel as their parameter.
impact - low
fixability - easy
description - To accomodate changes in DependencyModel and ParameterModel it was required to
	have access to both of them hence ComponentModel is being passed as a more generic object
	exposing access to all required elements.
fix - Pass in full ComponentModel, not just it's .Dependencies property. In the reference
	use component's properties to do all you require

change - IDependencyAwareActivator has new method: bool IsManagedExternally(ComponentModel);
impact - low
fixability - easy
description - To implement feature IOC-277 this new customization point was introduced which
	allows custom activators to specify whether the instance they activate shoud be managed
	by the container. If true is returned this signifies to the container that the component
	should not be tracked by the release policy. The activator should in that case also not
	invoke any lifecycle steps. Notice that lifestyle manager can override the choice and that
	this method will not be called in all cases.
fix - Implement the method however makes sense to you. By default you should just return false.

change - IExposeDependencyInfo.ObtainDependencyDetails method signature has changed
impact - low
fixability - easy
description - To move the code for constructing the exception when dependencies are missing
	out of handlers and open way for different scenarios a new interface was introduced:
	IDependencyInspector and it is now used by IExposeDependencyInfo to provide the same
	functionality as before.
fix - Adjust the calls to the new signature. If you have custom handler type take a look at
	how built in handlers are now implemented.

change - type attribute is now required and id is ignored in facility XML configuration
impact - low
fixability - easy
description - Since type is uniquely identifying facilities there was no point in keeping the id
	around anymore.
fix - This change can affect you in two ways. If you were using facilities node in the XML and
	not specifying the type it is now mandatory. Notice Windsor's ability to apply short type
	names works here as well, so often just type name is enough - no need to specify assembly
	qualified name. Also the assembly will now be instantiated by the container, so if you were
	adding it in code later on, this is no longer required (in fact it will throw an exception
	saying the assembly was already added).
	The other thing that may affect you is if you were looking up facility config namnually via
	IConfigurationStore.GetFacilityConfiguration method. It now expects full name of the type
	as the key, so you should be calling it like this:
	store.GetFacilityConfiguration(typeof(YourFacility).FullName);

change - EventWiringFacility, FactorySupportFacility and RemotingFacility are extracted to their
	own assemblies
impact - low
fixability - easy
description - These facilities are rarely used and two of them (FactorySupportFacility and 
	RemotingFacility) are mostly considered legacy. As such there's no point in keeping them
	in Windsor's assembly, especially in Silverlight version.
fix - Reference the new assemblies and update your references in XML if you use it.

change - Component.For(ComponentModel) overload was removed.
impact - low
fixability - medium
description - To simplify internal structure of fluent registration API and bring it more in 
	line with standard registration the overload was removed.
fix - If you really need this overload you can create custom IRegistration that exposes this
	functionality. Or better rethink why you need it in the first place.

change - Adding more than a single facility of any given type is not legal anymore
impact - none (I hope)
fixability - easy
description - Doing so is a bug. Why would you do it in the first place?
fix - Stop doing it.

change - RegisterCustomDependencies methods were moved from IKernel to IKernelInternal.
impact - low
fixability - easy
description - Those methods are hardly ever used these days so there was no point in polluting
	the public API with them
fix - Are you really using those methods? Perhaps you should be using the fluent API? If not
	just cast the kernel to IKernelInternal and you can access them.

change - IWindsorContainer.AddFacility and IKernel.AddFacility overloads that were taking
	Func<TFacility,object> were removed.
impact - low
fixability - easy
description - Those overloads were only cluttering the API and confusing users. There was no
	point in keeping them
fix - You should not have to fix that at all. C# compiler (in version 3 or higher) should be
	smart enough to pick the Action<TFacility> overload automatically if you're using lambda
	syntax. If you aren't, please do, or adjust the call to match the Action<TFacility> overload

change - IComponentModelBuilder.BuildModel and ComponentModel constructor take ComponenName now
	instead of string for 'name' parameter
impact - low
fixability - easy
description - Most of the time name given to components is automatically generated and user does
	not care what it is and never interacts with it. To be able to tell apart cases when user
	did set the name manually, and when it was auto-generated a new type ComponenName has been
	introduced which in addition to the name value keeps track of whether the name was provided
	by user or autogenerated.
fix - Update your calls accordingly, creating the ComponentName and passing right values in.
	Also in the fluent API the method NamedAutomatically was introduced for use by facilities
	and such to register their own components with some name that the user will not care about.

change - IConfigurationInterpreter.ProcessResource now takes an additional argument: IKernel
impact - low
fixability - easy
description - To accomodate ability not to specify id when configuring components or facilities
	in XML config in conjunction with simple type name support in Windsor (this feature that
	lets you specify just simple type name like Foo, instead of assembly qualified name like
	Acme.Crm.Foo, Acme.Crm) access to conversion subsystem was required and it made sense to
	grab entire kernel as some other things could be taken advantage of.
fix - Pass the kernel in.

change - Release policies have now slightly different semantics.
impact - medium
fixability - medium
description - To limit unnecessary tracking of components, which unnecessarily consumes memory
	and causes contention in multithreaded scenarios the following change was made to release
	policy semantics:
	- only objects whose decommission is managed by the policy (ie which are released by call to
	policy.Release, or indirectly: container.Release) can now be Tracked. This is determined by
	the 'RequiresPolicyRelease' flag on Burden. If the flag is not set the policy can throw.
fix - The change is likely to affect code using custom lifetime managers. It is now up to the
	manager to decide if it will release the object itself (then it should pass 'true' to
	'public Burden CreateBurden(bool trackedExternally)' method on CreationContext). Tracking
	happens also for objects that require it ('RequiresDecommission' on burden is 'true').
	If lifestyle manager wants to make sure the object will be tracked it can set this flag.
	Otherwise it is up to Windsor to decide if it needs to track the object or not.
	Another side-effect of the change is that calling 'container.Kernel.ReleasePolicy.HasTrack'
	may now return 'false', when it previously would return 'true', if the object does not meet
	the criteria mentioned above. If you were using this method, make sure you review your code
	that depends on it, and adjust it to the new requirements. The semantics of 'HasTrack' is 
	'does the release policy track this object', not 'does anything in the container track it'
	anymore.

change - IReleasePolicy interface has a new method: IReleasePolicy CreateSubPolicy(); usage of
	sub-policies changes how typed factories handle out-of-band-release of components (see
	description)
impact - medium
fixability - easy
description - This was added as an attempt to enable more fine grained lifetime scoping (mostly
	for per-typed-factory right now, but in the future also say - per-window in client app).
	As a side-effect of that (and change to release policy behavior described above) it is no
	longer possible to release objects resolved via typed factories, using container.Release.
	As the objects are now tracked only in the scope of the factory they will be released only
	if a call to factory releasing method is made, or when the factory itself is released.
fix - Method should return new object that exposes the same behavior as the 'parent' usually it
	is just best to return object of the same type (as the built-in release policies do).

change - IHandler.Release now takes Burden, not object as its parameter. Burden.Release now has
	no arguments (used to take IReleasePolicy)
impact - low
fixability - easy
description - The method used to take component instance to release. Now it takes Burden which
	has some additional information and behavior. Also to decouple Burden from IReleasePolicy
	it now uses callback (via Released event) as notification mechanism.
fix - Adjust calls appropriately

change - AllComponentsReleasePolicy was removed, ILifestyleManager.Resolve has different
	signature now, and additional responsibilities.
impact - medium
fixability - medium
description - Handling of decision regarding tracking is now happening in two steps. First step
	happens in the lifestyle manager, which gets to decide if the instance should be tracked
	at all (which should be chosen when a new instance is created) and if IReleasePolicy should
	own (trigger) the release process.
fix - If you implement custom lifestyle consult the implementation of standard lifestyles for
	examples how to handle each aspect of component lifestyle management. Broadly speaking the
	behavior should be the following (*do* inherit from AbstractLifestyleManager for your own
	convenience):
	- if your lifestyle employs caching, it should cache Burdens, not the objects resolved
	directly. Look up its cache, and if you find matching burden return object it manages 
	(accessed via 'Instance' property)
	- on cache miss call base.CreateInstance to obtain new instance from activator. This method
	will not return the managed object directly but rather a Burden instance. The 2nd argument
	'trackedExternally' should be set to true if the lifestyle manager uses some external mecha-
	nism to track end of life for components. If not, (when set to true) releasePolicy will take
	the responsibility.
	- inspect burden's RequiresDecommission property. If its value is true that means either
	the intsance obtained or at least one of its dependencies can not be released out of band
	and will require to be released explicitly. If the property is set to true you are required
	to track the componetn obtained with releasePolicy provided (you can use base.Track method 
	to acheave that). If the property is false, release policy will ignore the component when 
	container's Release method is called, and rely on your out of band handling).
	- cache your newly obtained instance if needed.
	- return the intance, (burden.Instance)

rename -  CreationContext.Empty -> CreationContext.CreateEmpty()
description - readability change to make it obvious that new instance is created each time.

change - IServiceProviderEx was removed as base interface for IWindsorContainer and IKernel
impact - low
fixability - easy
description - To make the interface for the container more compact the functionality was 
	extracted to external class - WindsorServiceProvider.
fix - Use WindsorServiceProvider instead.

rename -  INamingSubSystem.GetHandlers -> INamingSubSystem.GetAllHandlers
description - readability change. No affect on behavior

change - Removed the following methods:
	GraphNode.RemoveDepender,
	GraphNode.RemoveDependent,
	IKernel.RemoveComponent,
	IKernelEvents.ComponentUnregistered,
	INamingSubSystem.this[Type service],
	INamingSubSystem.GetHandler,
	INamingSubSystem.GetService2Handler,
	INamingSubSystem.GetKey2Handler,
	INamingSubSystem.UnRegister(String key),
	INamingSubSystem.UnRegister(Type service)
Also INamingSubSystem.Register now takes only IHandler as its argument
impact - low
fixability - none
description - The methods were implementation of "remove component from the container" feature
	which was flawed and problematic, hecen was scraped.
fix - Working around is quite dependant on your specific usage. Try utilizing IHandlerSelectors.
	For changed Register method, just update your calling code not to pass the name.
	handler.ComponentModel.Name is now used as the key, as it was happening in all places so far
	anyway, so this change should have no real impact.

change - Removed the following types: ContainerAdapter, ContainerWrapper, IContainerAdapter,
	IContainerAdapterSite
impact - low
fixability - none
description - These types require ability to remove components from a container. This ability
	was removed and since these types are hardly ever used, they were removed as well.
fix - No quick fix is possible. If you are depending on this functionality proaly your best shot
	is to replicate it, espeicially catering for the removal of components which is no longer
	available in Windsor.

change - Removed ComponentRegistration.If and ComponentRegistration.Until methods, as well as
	Component.ServiceAlreadyRegistered method, and replaced their most common usage with
	ComponentRegistration.OnlyNewServices method
impact - medium
fixability - easy/hard
description - To make the API simpler easier to discover as well as to allow changes in internal
	architecture, the aforementioned changes were made.
fix - Most of the time the removed methods were used in the following combination:
	Component.For<Foo>().Unless(Component.ServiceAlreadyRegistered)
	In this case the fix is simple. Just replace the .Unless(Component.ServiceAlreadyRegistered)
	with .OnlyNewServices()
	If you were using the method in some other way, the fix may be more complicated and depend
	on your particular scenario. In those cases it's best to consult Castle users group for
	advice on how to proceed.

change - Rebuilt how components exposing multiple services are handled internally. This includes
	several changes to the API:
	ForwardingHandler class and IHandlerFactory.CreateForwarding method were removed.
	ComponentModel.Service property was removed replaced with ClassService and InterfaceServices
	properties. Also AddService method was added. Constructor's argument for service was changed
	to be Type[] instead of single Type.
	IHandler.Service property was removed, replaced by Services property.
	IComponentModelBuilder.BuildModel method takes now ICollection<Type> isntead of single Type
	as services.
	ComponentRegistration.For(Type serviceType, params Type[] forwaredTypes) method was removed.
	ComponentFilter delegate type was removed as no longer needed
impact - low
fixability - easy
description - As part of improvement to internal architecture changed how components exposing 
	more than one service are handled.
fix - This change should not affect most users, unless extending internals of the container. If
	that's the case, adjust your calls to the new signatures, and change code anticipating
	ForwardedHandlers to use Services collection from the solve IHandler for any given component

change - Proxies no longer implicitly implement all interfaces of component implementation type.
impact - medium
fixability - medium
description - This original behavior was actually a bug and would produce unpredictible behavior
	for components exposing several services including their class.
fix - if you were depending on the additional non-service intrfaces being forwarded to the proxy
	specify them explicitly as addtional interfaces to proxy:
	container.Register(Component.For<CountingInterceptor>()
						.Named("a"),
					Component.For<ICommon>()
						.ImplementedBy<TwoInterfacesImpl>()
						.Interceptors("a")
						.Proxy.AdditionalInterfaces(typeof(ICommon2))
						.LifeStyle.Transient);

change - NamingPartsSubSystem, KeySearchNamingSubSystem, ComponentName, BinaryTreeComponentName
	and TreeNode types were removed.
impact - medium
fixability - medium
description - As part of internal cleanup these esoteric, alternative implementations of naming
	subsystem were removed.
fix - behavior of these implementations of naming subsystem can be easily emulated with default
	naming subsystem and custom IHandlerSelectors, which is the recommended way to go.

change - UseSingleInterfaceProxy option was removed
impact - low
fixability - easy
description - As part of clean up of the obsolete API the option was removed to enable certain
	internal changes for the release.
fix - if you were using this option and you have to use it, use a IProxyGenerationHook impl
	and choose to only proxy members of that single interface.

## 3.0.0 RC 1 (2011-11-20)

- implemented IOC-318 - Provide more high level API for fitering (and ignoring/requiring) properties at registration time
- implemented IOC-317 - Add ability to reference AppSettings values in XML using #{property} syntax
- implemented IOC-316 - Add attribute to specify default selector for a typed factory interface/delegate
- implemented IOC-313 - Add event to be raised by the container whenever empty collection is being resolved
- implemented IOC-312 - Add shortcut methods to API to register types from given namespace
- fixed IOC-320 - System.ArgumentNullException at Castle.MicroKernel.Burden.Release(IReleasePolicy policy)
- fixed IOC-319 - Concurrency problem when child container is used 
- fixed IOC-315 - ResolveAll should not ignore generic constraint violations on dependencies of resolved component
- fixed IOC-314 - Parsing container configuration uses the current culture
- fixed IOC-311 - OptimizeDependencyResolutionDisposable eats exceptions thrown during installation
- fixed IOC-310 - Add ability to disable performance counters

## 3.0.0 beta 1 (2011-08-14)

- implemented IOC-306 - Add ability to provide fine-grained filtering of properties
- implemented IOC-303 - Support proxying for components registered using factory method
- implemented IOC-302 - Support open generic components where implementation has more generic parameters than service if they can be figured out based on generic constraints
- implemented IOC-301 - Add ConfigureIf method to be used with custom predicate when configuring components registered via convention
- implemented IOC-298 - Add a method to FromAssembly that will scan all assemblies in the application for installers
- implemented IOC-292 - Add fluent registration entry point that passes through truly "all types" that is interfaces, or abstract classes can be registered too
- implemented IOC-291 - Add alias class to AllTypes that is better named, like 'Classes'
- implemented IOC-287 - Add overloads to OnCreate and OnDestroy that only take the instance and leave the container out as it is often superfluous
- implemented IOC-285 - Add abilitty to make a component the default for a service without ensuring it's the first component exposed that service registered
- implemented IOC-284 - Optimize fluent registration API for less typing
- implemented IOC-283 - Ability to create custom lifestyle attribute with custom LifestyleManager
- implemented IOC-281 - Provide out of the box support for Lazy<T>
- implemented IOC-279 - WindsorContainer constructor taking string should accept not only file path but also other supported locations, like UNC, config section and embedded resource
- implemented IOC-277 - Add ability for components activated in a custom way to opt out of container lifetime management
- implemented IOC-275 - Exception message thrown when dependencies are missing is not always very clear and should be improved
- implemented IOC-271 - Support open generic components where implementing class has more generic parameters than the service but the missing ones can be somehow inferred
- implemented IOC-270 - Add OnDestroy method, symertical to OnCreate
- implemented IOC-269 - Windsor Performance Counters
- implemented IOC-268 - Hook that allows for filtering handlers during ResolveAll
- implemented IOC-263 - Add new debugger diagnostics - tracked objects
- implemented IOC-257 - Same as in code, specifying type for facility in XML should be enough - Id should be optional
- implemented IOC-256 - Same as in code, specifying type for component in XML should be enough - Id should be optional
- implemented IOC-255 - Specifying custom lifestyle type in XML should be enough, for it to be picked up
- implemented IOC-249 - Remove aility to remove components from the Container
- implemented IOC-246 - Remove alternative naming subsystems
- implemented IOC-243 - Remove obsolete UseSingleInterfaceProxy option
- fixed IOC-305 - GenericListConverter throwing NotImplementedException
- fixed IOC-299 - ResolveAll ignores services for open version of generic service requested
- fixed IOC-297 - Container should throw an exception if a "primitive type" is registered as a service, since it will not be resolved
- fixed IOC-295 - registration via XML ignores service specofied in attribute
- fixed IOC-286 - Custom logger config in XML is broken
- fixed IOC-282 - Windsor should be able to register generic typed factories as open generics
- fixed IOC-280 - ResolveAll should respect services and fail hard when a component can't be resolved
- fixed IOC-278 - Optional Dependencies should also be satisfied from ILazyComponentLoaders if possible
- fixed IOC-273 - Auto register PerWebRequestLifestyleModule using PreApplicationStartMethodAttribute at runtime
- fixed IOC-267 - Register() on a System.ValueType (like an Int32 or an Enum) instance should throw an exception
- fixed IOC-265 - In certain cases of cyclic dependencies debugger view times out because of stack overflow in MismatchedLifestyleDependencyViewBuilder
- fixed IOC-262 - objects created via UsingFactoryMethod are always tracked, even if they could safely not be
- fixed IOC-260 - Generic Typed Factories no longer working in trunk
- fixed IOC-254 - Optional non-primitive .ctor parameters don't work
- fixed IOC-250 - Dispose not being called on open generic registrations
- fixed IOC-248 - Open generic components with multiple services, some of which are generic fail to properly instantiate in certain cases
- fixed IOC-247 - Make ComponentModel/IHandler expose all services for given component, instead of piggybacking them via ForwardedHandlers
- fixed IOC-245 - Proxies (for interface services) should not implicitly proxy all interfaces that the service implementation type happens to implement
- fixed IOC-240 - Castle Windsor ArrayResolver ServiceOverrides Not Respected
- fixed FACILITIES-153 - Issue with setting the inital log level for the ConsoleLogger
- EventWiringFacility, FactorySupportFacility and RemotingFacility are extracted to their own assemblies
- fixed bug with NullReferenceException when TypedFactoryFacility is used and disposed
- IServiceProviderEx was removed as base interface for IWindsorContainer and IKernel
- Removed the following types: ContainerAdapter, ContainerWrapper, IContainerAdapter, IContainerAdapterSite

## 2.5.4 (2011-10-01)

- fixed issue causing IndexOutOfRangeException in heavy load multithreaded scenarios when releasing typed factories or components using DynamicParameters method
- fixed issue causing transient objects being dependencies of per web request objects being resolved multiple times during a single request to still be tracked by the container after the web request ended
- fixed issue causing typed factory to unnecessarily accumulate referenced to tracked singletons resolved via the factory
- fixed issue causing per web request objects to still be tracked by the container after being relesed in heavy load multithreaded scenarios

## 2.5.3 (2011-02-02)

- fixed IOC-266 - Dependency of a generic service is not disposed in Windsor 2.5.x 
- fixed IOC-261 - Disposing of typed-factory can throw argument null exception
- fixed IOC-254 - Optional non-primitive .ctor parameters don't work
- fixed IOC-250 - Dispose not being called on open generic registrations

## 2.5.2 (2010-11-15)

- implemented IOC-243 - Unseal the InterceptorAttribute class
- fixed IOC-239 - ArrayResolver attempts to instantiate an unresolvable array dependency
- fixed IOC-238 - Resolving Composite depending on a Decorator may fire up cycle detection fuse
- fixed IOC-237 - Castle Windsor : Possible bug with Startable Facility and "decorator pattern" dependencies
- fixed IOC-236 - Typed Factory Facility causes memory leak because it keeps a reference after releasing object in list 'trackedComponents'
- fixed IOC-235 - TypedFactoryFacility with inherited interfaces throws an exception
- fixed IOC-234 - StackOverflow causing inability to use debugger view when there are components with dependency cycles in the container
- fixed IOC-232 - Exception when using delegate based factories can throw when registered implicitly and used as dependencies of generic component
- fixed IOC-231 - Boilerplate methods on facilities should be hidden from IntelliSense when configuring a facility
- fixed IOC-230 - Missing Mixins/InterceptorSelectors/ProxyGenerationHooks and TypedFactoryFacility's component selectors are not detected until resolution time
- fixed IOC-229 - Qurerying for subsystem is case sensitive
- implemented IOC-228 - Chicken and egg problem when trying to inherit from DefaultDependencyResolver
- fixed IOC-227 - ResolveAll fails for generic forwarded registrations
- fixed IOC-224 - Obsolete message on some members of old obsolete API don't compile
- fixed IOC-223 - Fluent registration registers components with System.Object service when no BasedOn discriminator is provided

Breaking Changes:

change - One of CreationContext constructors has now additional argument; parent CreationContext
	Method public IDisposable ParentResolutionContext(...) on CreationContext was removed
	Method protected CreationContext CreateCreationContext(...) has now additional argument;
	parent CreationContext
impact - low
fixability - medium
description - To fix issue with false positive cycle detection (see issue IOC-238) changes had
	to be made to how parent creation context gets propagated in certain situation (when call
	to kernel.Resolve/ResolveAll is performed as part of resolution process, for example when
	CollectionResolver is being used).
fix - If you override CreateCreationContext method on DefaultKernel pass the additional argument
	as new constructor parameter to CreationContext.
	If you were using ParentResolutionContext method it should be fairly safe to remove the call
	if it was preceded by call to updated CreationContext constructor and the CreationContext is
	not used outside of local scope. In other cases it's best to consult Castle users group for
	advice on how to proceed.

change - IReference<> interface has two new methods
impact - low
fixability - easy
description - To make it possible to statically analyze dynamic dependencies provided by 
	the IReference interface two new methods were added:
			void Attach(DependencyModelCollection dependencies);
			void Detach(DependencyModelCollection dependencies);
fix - if you're providing dependencies on a component from the container call Attach so that 
	reference gets a chance to create and add DependencyModel for that dependency so that
	it can be statically analyzed by the container.

change - Method IDependencyResolver.Initialize change signature
impact - low
fixability - easy
description - To make it possible to use custom DependencyResolver inheriting from 
	DefaultDependencyResolver initialization of DefaultDependencyResolver was moved out of its
	constructor and to IDependencyResolver.Initialize method which now takes IKernel as its
	additional parameter
fix - if you're implementing the interface adjust signature of the overriding method to
	public void Initialize(IKernel kernel, DependencyDelegate dependencyDelegate)
	The method is called by the kernel at the end of its constructor.

change - Changed visibility of members on AbstractFacility to protected and implementation of
	interface members to explicit.
impact - low
fixability - easy
description - To make it less confusing to users when fluently configuring facilities (via 
	AddFacility<SomeFacility>(f => f.ConfigureSomething()) method) visibility of certain members
	of AbstractFacility class was changed. Public properties FacilityConfig and Kernel are now
	protected, and all methods from IFacility interface are implemented explicitly. Additionally
	protected Dispose method was introduced to allow inheriting classes to still be disposed.
fix - If you were using FacilityConfig and/or Kernel properties outside of inherited classes
	refactor your code accordingly not to do so. If you were overriding Dispose method change
	its signature from
	public override void Dispose() to
	protected override void Dispose()

## 2.5.1 (2010-09-21)

- added "Potential lifestyle mismatches" debugger view item, that will detect and list situations where Singleton depends on Transient or PerWebRequest component (which is usually a bug)
- fixed issue where forwarding main type would create additional, superfluous handler
- WebLogger/WebLoggerFactory was removed from Castle.Core so all references to that are removed from Windsor as well
- obseleted UseSingleProxyInterface in preference over IProxyGenerationHook
- fixed IOC-220 Composite pattern with CollectionResolver should be properly supported without throwing "cycle detected" exception
- fixed IOC-218 Enable methods that take arguments as anonymous objects in Silverlight version. This works in SL, but requires [assembly: InternalsVisibleTo(Castle.Core.Internal.InternalsVisible.ToCastleCore)]
- fixed IOC-217 Enable ISupportInitialize support as lifecyclecle concern in Silverlight 4
- implemented IOC-216 Make it possible to specify service overrides in DependsOn, either via Property, or ServiceOverride entry class
- implemented IOC-215 Hide obsolete members from IntelliSense (in basic view. By default in VB they won't be showed, but will in C# :( )
- fixed IOC-214 Missing bracket in obsolete warning text
- implemented IOC-212 Add ability to make IProxyGenerationHooks and IInterceptoSelectors IOnBehalfAware
- fixed IOC-211 Resolve doesn't work with constructor's ref argument
- fixed IOC-210 Typed Factory Facility treats constructor dependency as non-optional if resolved as a TFF component
- fixed IOC-209 Bug in constructor selection when resolving - Windsor would pick unresolvable constructor
- reverted back (to the way it was in v2.1) conditional registration of helper components used by TypedFactoryFacility as it would cause issues when used with nested containers (see the new test and thread "Typed Factories in sub Container (differences between 2.5 and 2.1)" on users group)
- added framework information the assembly was built for to the AssemblyTitle attribute
- improved how late bound types are displayed in debugger
- fixed bug where count of potentially misconfigured components would show invalid value
- added raw handler access to default component view in debugger
- changed how status message is displayed for potentially misconfigured components so that an actual visualizer for strings can be used to view this potentially long piece of text

Breaking Changes:

change - ILazyComponentLoader.Load now accepts a third argument for additional arguments.
impact - medium
fixability - easy
description - To allow maximum flexibility and usage with Resolve, any additional arguments
   are now passed to the lazy loader.

change - LifecycleStepCollection class was removed. Instaed LifecycleConcernsCollection class
	was introduced. ILifecycleConcern has now two innerited interfaces for commission and
	decommission. LifecycleSteps property of ComponentModel was renamed to Lifecycle.
	LifecycleStepType type was removed.
impact - medium
fixability - easy
description - To improve strongly typed nature and decrease probability of mistake and improve
	general usability of the type LifecycleStepCollection was removed. In it place similar type
	was introduced - LifecycleConcernsCollection. Instead of using untyped Objects and enums
	it works with two new interfaces : ICommissionConcern and IDecommissionConcern.
fix - have your lifecycle steps implement one of the new lifecycle interfaces. Use appropriate
	overload of Add/AddFirst to add them.

change - Typed Factories will not implicitly pick default ITypedFactoryComponentSelector 
	registered in the container anymore
impact - low
fixability - easy
description - In version 2.1 where ITypedFactoryComponentSelectors were introduced, when you had
	a selector registered in the container that selector would be implicitly picked for every
	factory you had. Since the behavior of a selector tends to be fine grained and targetet for
	a specific factories, this behavior was removed. You have to explicitly associate the selector
	with a factory (using .AsFactory(f => f.SelectUsing("MySelector")); or via xml configuration)
	to override selection behavior.
fix - using either fluent API .AsFactory(f => f.SelectUsing("MySelector")), or XML configuration
	selector="${MySelector}" specify the selector explicitly for each of your factories.

change - ServiceSelector delegate (used in WithService.Select calls) changed signature
impact - low
fixability - easy
description - To fix a bug which would occur if type implemented multiple closed version of base
	open generic interface the signature of the delegate was changed from
	public delegate IEnumerable<Type> ServiceSelector(Type type, Type baseType);
	to
	public delegate IEnumerable<Type> ServiceSelector(Type type, Type[] baseTypes);
	so that multiple base types are possible (they would be closed versions of the same open
	generic interface)
fix - depending on the scenario. You would either ignore it, or wrap your current method's body
	in foreach(var baseType in baseTypes)

change - moved IWindsorInstaller to Castle.MicroKernel.Registration namespace
impact - very low
fixability - easy
description -In order to improve developer experience when writing installers the interface
	was moved so that Component and AllTypes entry types for registration are already in scope.
fix - add using Castle.MicroKernel.Registration directive.

change - Added two new overloads to ITypeConverter.PerformConversion
impact - very low
fixability - easy
description - To reduce casting in the most common scenario where converted value is casted to
	the type it's been converted to, ITypeConverter.PerformConversion has now generic overloads
	for handling this case.
fix - If you're implementing ITypeConverter via AbstractTypeConverter you don't have to do
	anything as the base class will handle the conversion for you. Otherwise implement it like
	in AbstractTypeConverter.

change - AddCustomComponent method were moved from IKernel to IKernelInternal interface
impact - very low
fixability - easy
description - This method constitute internally used contract of kernel and is not intended
	for external usage. As such it was moved to internal interface to declutter public
	interface of IKernel.
fix - You should not have been using this method so it should not affect you in any way. If
	you did, cast the IKernel to IKernelInternal to invoke the method.

change - IModelInterceptorsSelector.SelectInterceptors method changed its signature and how it
	is used.
impact - medium
fixability - medium
description - To accomodate additional scenarios that were impossible (or hard to achieve
	with previous design the method now has additional parameter, an array of references to
	interceptors, which contains either default interceptors for the component, or interceptors
	selected by previous interceptors in line). Also, Windsor will now never call
	IModelInterceptorsSelector.SelectInterceptors without calling 
	IModelInterceptorsSelector.HasInterceptors before it, or when the latter returns false.
fix - When adjusting your implementation remember that model's interceptors are the default value
	passed as methods second parameter, so you don't need to merge them again manually (otherwise
	they'll be invoked twice).

change - CreateComponentActivator, RaiseHandlerRegistered, RaiseHandlersChanged and
	 RegisterHandlerForwarding methods were moved from IKernel to IKernelInternal interface
impact - very low
fixability - easy
description - These methods constitute internally used contract of kernel and are not intended
	for external usage. As such they were moved to internal interface to declutter public
	interface of IKernel.
fix - You should not have been using these methods so it should not affect you in any way. If
	you did, cast the IKernel to IKernelInternal to invoke the methods.

change - IProxyHook interface was removed
impact - very low
fixability - easy
description - Since MicroKernel was merged with Windsor and now depends on DynamicProxy directly
	there's no need to provide additional abstraction on top of IProxyGenerationHook.
fix - Make types that were implementing IProxyHook to implement IProxyGenerationHook. Change all
	usages of IProxyHook to IProxyGenerationHook.

change -  AddInstallerConfiguration and GetComponents methods were added to IConfigurationStore.
impact - very low
fixability - easy
revision - 3bf716cc6fc218601dab92a6dd75fe269bcb63d0
description - To enable installers to be exposed via configuration the interface has been 
	extended by addition of the two methods.
fix - Implement the methods accordingly to your situation.

change - Multiple types were moved between namespaces
impact - low
fixability - trivial
revision - 3bf716cc6fc218601dab92a6dd75fe269bcb63d0
description - To improve the internal structure several types were moved to other namespaces.
fix - When compilation error occurs adjust namespace imports as suggested by Visual Studio

change - Assembly Castle.MicroKernel.dll was merged into Castle.Windsor.dll
impact - high
fixability - easy
revision - 730b202b0ed23a6b42258a6ffd6a3e63f89501fc
description - Since vast majority of users used Windsor, as opposed to bare MicroKernel it was
	decided it didn't make sense to maintain two containers. As result of that their assemblies
	were merged, as first step of integration between Windsor and MicroKernel.
fix - In your projects remove reference to Castle.MicroKernel.dll. If you weren't using Windsor
	add reference to Castle.Windsor.dll
	In all places where your were referencing types from Castle.MicroKernel.dll via string
	(like xml configuration when registering facilities, or <httpModules> section on your 
	web.config) update references from Castle.MicroKernel to Castle.Windsor.

change - `ComponentRegistration<S>.Startable` public method has been removed.
	`ComponentRegistration<S>.StartUsingMethod` public method was moved to extension method.
	`ComponentRegistration<S>.StopUsingMethod` public method was moved to extension method.
impact - low
fixability - trivial
revision - 6710
description - StartUsingMethod/StopUsingMethod belong to StartableFacility and do not make sense
	as part of generic API. Startable method was superfluous.
fix - Remove calls to Startable(). Import namespace Castle.Facilities.Startable to use
	StartUsingMethod and StopUsingMethod as extension methods.

change - DefaultProxyFactory.CreateProxyGenerationOptionsFrom protected method  and
	DefaultProxyFactory.CustomizeProxy protected virtual method have changed signature
impact - very low
fixability - easy
revision - 6691
description - the methods now also takes IKernel and CreationContext, to be used by IReferences
	to do resolution of components they reference
fix - pass required parameters to the methods.

change - ProxyOption's properties changed types: 
	Selector, from IInterceptorSelector to IReference<IInterceptorSelector>
	Hook from IProxyHook to IReference<IProxyHook>
	MixIns from object[] to IEnumerable<IReference<object>>
impact - very low
fixability - easy
revision - 6691
description - the properties now use IReferences instead of live objects to allow for
	resolution of their values from the container, as required in case of usage from xml.
fix - wherever used, adjust types appropriately. To obtain actual objects, use Resolve method.

## 2.5.0 (2010-08-21)

- debugger view support has been extracted to a separate subsystem (IContainerDebuggerExtensionHost) and can be extended by users code via IContainerDebuggerExtension and IComponentDebuggerExtension
- calling IHandler.TryStart will no longer silently ignore all the exceptions.
- added CollectionResolver which is a more general version of ArrayResolver and ListResolver and supports in addition ICollection<Foo> and IEnumerable<Foo>
- fixed issue where dependencies would not be cleaned up when component creation failed
- fixed issue where startable component would be created twice when property dependency could not be resolved
- passing arguments to ILazyComponentLoader (see breakingchanges.txt)
- fixed bug that caused exception when proxied component and it's proxied property dependency shared interceptor

## 2.5.0 beta2 (2010-07-21)

- added support for selecting components based on custom attributes and their properties. See Component.HasAttribute<T>() methods
- added WithService.DefaultInterface() to fluent API.IT matches Foo to IFoo, SuperFooExtended to IFoo and IFooExtended etc
- added support for CastleComponentAttribute in fluent Api. Also added helper filter method Component.IsCastleComponent
- added ability to specify interceptors selector as a service, not just as instance
- added ability to specify proxy hook in fluent API: 
- indexers on IKernel are now obsolete.
- added WithAppConfig() method to logging facility to point to loging configuration in AppDomain's config file (web.config or app.config)
- Restructured lifecycle concerns - introduced ICommissionConcern and IDecommissionConcern and favors them over old enum driven style.
- Fixed how contextual arguments are handled. Null is no longer considered a valid value (That would cause an exception later on, now it's ignored).
- Changed method DeferredStart on StartableFacility. It now does not take a bool parameter. A DeferredTryStart() method was introduced instead.

## 2.5.0 beta1 (2010-07-05)

- Typed Factories will not implicitly pick default ITypedFactoryComponentSelector registered in the container anymore
- Obsoleted all the AddComponent* methods in favor of using Installers and fluent registration API
- ServiceSelector delegate (used in WithService.Select calls) changed signature to fix a bug: http://3.ly/eP5Q
- moved IWindsorInstaller to Castle.MicroKernel.Registration namespace
- typed factories will now obey container release policy, that is if the container does not track the component, so won't the factory.
- added helper methods to fluently configure logging facility using: container.AddFacility<LoggingFacility>( f = > f.Fluent().Magic().Here() );
- added overload for UsingFactoryMethod which exposees ComponentModel of component to the factory
- added generic overloads for ITypeConverter.PerformConversion to reduce casting.
- it is now possible to call WithService.Foo().WithService.Bar() and both services will be used. Also more methods were added: WithService.Self() and WithService.AllInterfaces()
- added simple debugger visualizer to help diagnosing misconfigured components.
- added optimized mode to StartableFacility for Single-call-to-Install scenario that won't start anything before the end of Install (at which point the container is assumed to be completely configured and all components should be there) and it will throw if it can't resolve and start the component.
- added OptimizeDependencyResolution around calls to Install
- Component.IsInNamespace and its sister methods have now overload that let you include components from subnamespaces as well.
- added ability to load assemblies from designated directory (with fair bit of optional filtering using new AssemblyFilter class). It works in three places:
	- AllTypes.FromAssemblyInDirectory() - picks assemblies for registration
	- FromAssembly.InDirectory() - installs installers from assemblies in the directory
	- <install directory="" /> - installs installers from assemblies in directory via XML
- TypedFactoryFacility - added ability to configure factory inline: Component.For<IFooFactory>().AsFactory(f => f.SelectedWith("selectorKey")) 
- Changed IModelInterceptorSelector's signature and behavior (see breakingChanges.txt for details)
- removed IProxyHook interface (see breakingchanges.txt)
- added support for specifying typed factory component selectors on a per-factory basis
- added support for using services as mixins
- added autogenerated delegate-based factories. Taking dependency on Func<IFoo> and calling the delegate will return IFoo from the container
- implemented IOC-ISSUE-203 - Add to fluent API scanning assemblies for IWindsorInstallers and installing them
- added fluent API for EventWiringFacility
- added ability to specify assemblies that will be scanned for types when shorthened type name is using via XML using the following syntax:
	<using assembly="Assembly name or path to file.dll" />
- added ability to specify installers (IWindsorInstaller) via XML using either of the following:
  <installers>
	<install type="Castle.Windsor.Tests.Installers.CustomerInstaller"/>
	<install assembly="Castle.Windsor.Tests"/>
  </installers>
  installers must be public and have default constructor.
- Xml config does not require assembly qualified type name any more - specifying just type name, or typename+namespace should be enough. Works only for types in already loaded assemblies.
- ResolveAll will now resolve components that are not in Valid state when inline/dynamic arguments are provided
- TypedFactoryFacility: TypedFactoryComponent will now fallback to resolving by type if no component with designated name can be found
- fixed issue with per-web-request components not being released properly in some cases
- fixed IOC-ISSUE-199 - NamingPartsSubSystem broken when RegisterHandlerForwarding is used
- TypedFactoryFacility: added ability to resolve multiple components
- TypedFactoryFacility: added ability to put custom resolving logic
- fixed another case of IoC-168 where a component with two constructors of different parameter length couldn't be resolved when the fewer parameter constructor was not satisfied
- If and Unless functions on fluent registration API are now cumulative - it is legal to call them multiple times and all conditions will be checked. This is a minor breaking change from previous behavior where last call would win.
- added typed arguments (specified by type rather than by name).
	It works for:
	- call site (Resolve with Dictionary, specifying System.Type as value of key. A helper class 'Arguments' should be used for this.)
	- DynamicParameters - there's an extension method Insert that should make using it nicer
	- fluent Api (DependsOn(Property.ForKey<string>().Eq("typed"))
- added 'Insert' extension method on IDictionary, specifically so simplify usage in DynamicParameters method and similar situations. It behaves like IDictionary's indexer setter
- added 'Arguments' class to carry inline arguments (typed or named) for components. It is recommended to use this class rather than Hashtable or Dictionary<>
- added strongly typed overloads for StartUsingMethod and StopUsingMethod from startable facility's registration API. It is now possible to call .StartUsingMethod(x => x.Start).StopUsingMethod(x => x.Stop)
- moved StartUsingMethod/StopUsingMethod to extension methods in StartableFacility's namespace. Startable() method was removed as superfluous.
- changed the UsingFactoryMethod (and UsingFactory) methods in fluent registration API to not rely on FactorySupportFacility. They now work even if facility is not used.
- fixed IOC-ISSUE-190 - "Resolve with argumentsAsAnonymousType overload is now case sensitive".
	This fixed a regression bug introduced in v2.1, and brings the behavior back to what it was in v2.0.
- added support for specifying interceptorsSelector, proxyHook and mixins from config (see new tests for example). This also means some small impact breaking changes:
	- DefaultProxyFactory.CreateProxyGenerationOptionsFrom protected method has changed signature - it now also takes IKernel and CreationContext, to be used by IReferences to do resolve (see below)
	- DefaultProxyFactory.CustomizeProxy protected virtual method has changed signature, for the same reason as above
	- ProxyOption's properties changed types: 
		Selector, from IInterceptorSelector to IReference<IInterceptorSelector>
		Hook from IProxyHook to IReference<IProxyHook>
		MixIns from object[] to IEnumerable<IReference<object>>
	IReference abstraction allows to use components resolved from the container, similar to InterceptorReferences.
- Moved several types from Core:
	ComponentActivatorAttribute
	ComponentProxyBehaviorAttribute
	CustomLifestyleAttribute
	DoNotWireAttribute      
	InterceptorAttribute    
	LifestyleAttribute
	PooledAttribute
	TransientAttribute
	GraphNode
	IVertex
	IRecyclable
	IStartable
	ComponentModel
	ConstructorCandidate
	ConstructorCandidateCollection
	DependencyModel
	DependencyModelCollection
	InterceptorReference
	InterceptorReferenceCollection
	LifecycleStepCollection
	MethodMetaModel
	MethodMetaModelCollection
	ParameterModel
	ParameterModelCollection
	PropertySet
	PropertySetCollection
	TopologicalSortAlgo
	IOnBehalfAware
	GraphSets
	GraphTestCase

## 2.1.1 (2010-01-13)

- Reverted factory support facility changes in r6595, r6596 and r6653 which fixed IOC-ISSUE-153, however caused other bugs
  reported on the mailing list (http://groups.google.com/group/castle-project-users/browse_thread/thread/3f2b602e738a08c6?hl=en)

## 2.1.0 (2010-01-12)

- Moved the logging facility project into the Windsor project:
  - Applied Tom Allard's patch fixing FACILITIES-93: "Extra constructors on LoggingFacility"
  - Added test case supplied by chris ortman
  - Register base logger and factory when using extended logger.
  - Fixed FACILITIES-77 - ILoggerFactory instance creation requires constructor with one argument
- simplified API for attaching interceptors.
- added support for forwarded types in XML config
- added WithParameters method to fluent registration that enables inspecting and modifying arguments passed to Resolve method.
- BREAKING CHANGE - AbstractHandler.Resolve method is no longer abstract and instead a ResolveCore protected abstract method was added. To fix this, implementers should override ResolveCore instead of Resolve.
- added OnCreate method (refactored from OnCreateFacility created by Tehlike) which allows to specify actions to be invoked on the component right after it is created, and before it's returned from the container

## 2.0

- Updated FactorySupportFacility and fluent registration to allow propagation of CreationContext to factory methods
- Fixed Burden release issue in which children were being released if the component was not destroyed
- Automatically configure proxy to omit target if no implementation
- Fluent interface for factory support
- Fixed an issue with of not considering unregistered service dependencies in arrays
- Will not try to convert values that are already a match to the parameter type
- XmlProcessor now properly dispose of the stream reader
- The kernel will now check if trying to register null types

## RC 4

- Update FromInterface Registration policy to only consider toplevel interfaces and allow multiple services.
- Fixed bug in AllComponentsReleasePolicy in which burden not properly handled on dispose.
- Applied patch from Joao Braganca to allow abstract types in DefaultComponentActivator if proxied.
- Added additional AddFacility overrides to improve fluent configuration of facilities.
- Moved DefaultComponentActivator check for abstract so it can be better overriden.
- Added Attribute to Component Registration fluent interface.
- Add ability to use Configure components based on implementation type when using AllTypesOf.
- Do not return forward handlers in ResolveAll since you will get duplicate services.
- Applied patch (with mods) from Martin Nllsson to select registration interface from containing interface.
- Added shortcut to AllTypes to accept a where.
- Added ability to include non-public types in registration.
- Updated registration to support providing multiple service types.
- Add registration support for mixins.
- Do not allow registering components with the same name in fluent interface.
- Applied Ayendes patch to introduce component service type forwarding to
  support multiple service interfaces for a component.
  Extended the Component Registration interface to support service forwarding.
- Avoid to register abstract component via IKernel.AddComponent, now throws when trying to add instead of while resolving component
- Removed sealed qualifier from CreationContext and made ISubDependencyResolver methods virtual so they can be overriden.
- Made IKernel.AddFacility fluent.
- Added StartMethod/StartMethod to ComponentRegistration.
- Add if/unless support for ComponentRegistration.
- Applied Daniel Jins patch to not proxy internal interfaces.
- Fixed IOC-126: "PoolableLifestyleManager creates pool in constructor" 
- Fixed IOC-125: "DefaultGenericHandler does not properly handle proxied generic components"
- Updated AllTypes strategy to support types based on generic type definitions.
- Updated AllTypes strategy to support multiple registrations from a single set of types.
- Collection handlers from parent container for GetAssignableHandlers.
- Added ability to change invocation target to DefaultProxyFactory.
- Fixed bug with ComponentRegistration.Instance in which the instance type was not assigned as the ComponentModel implementation.
- Replaced AllTypesOf<T> syntax with AllTypes.Of<T> so a non-generic version can be consistently provided.  
- Added generic AddFacility methods to kernel.
- Added generalized configuration support to ComponentRegistration.
- Added IWindsorInstaller interface to enhance Windsor component installation.
- Added AllTypesOf registration stratgey to simplify custom registration scenarios.
- Added IRegistration interface to allow alternate registration mechanisms.
- Fixed CORE-16 (Should be Facilities): "The FactorySupportFacility does not create proxies if interceptors are present"
- Added support for list service overrides using the fluent registration interface.
  Added support for specifying configuration parameters using the fluent interface to allow any complex registration scenarios.
- Restructured the registration fluent interface to be a little more readable,
  better support component registrations and prevent errors resulting from  forgetting to call ComponentRegistration.Register
- Fixed Facilities-97: "EventWiring Facility fails to create some components"
- Added support for non-generic usage of fluent-interface.  Needed for dynamic registrations scenarios (Binsor)
  Automatically register the component between consecutive AddComponentEx (Saves a few strokes).
- Initial version of MicroKernel/Windsor fluent interface IOC-99
- Applied patch from Jacob Lewallen improving the locking performance in the DefaultNamingSubsystem under high load.
- Applied Philippe Tremblay's patch fixing IOC-94: "Copy LifeStyle from generic interface"
- Added support for copying interceptors defined on the geneirc interface handler.
- Fixed IOC-80
  "StartableFacility erroneously tries to start a component before
  RegisterCustomDependency can be called"
- Added ComponentModelConverter to utilize System.ComponentModel TypeConverters
  Very useful for converting things like Fonts and Colors
- Updated DefaultComplexConverter to support interfaces and derived types
- Fixed IOC-96: "FactorySupport fails to create components if the factory instance is a proxy"
- Fixed IOC-93: "GenericListConverter does not handle service overrides properly" 
- Fixed IOC-91: "ContextBoundObject's context is not bound when object is created by MicroKernel"
- Fixed build from IContainerAccessor change
- Applied Ron Grabowski's patch fixing IOC-89: "Make DefaultKernel implement IServiceProvider"
- Check for required Properties before determining the Handlers initial state
- Fixed IoC-87: "DefaultComplextConverter does not properly handle nested components"
- Applied Lee Henson's patch fixing IOC-86: "Additional generic AddComponent overloads"
- Applied Ido Samuelson patch fixing IOC-85: "IKernel to support generics to add/resolve components."
- Refactored proxy options support. Now you can use the attribute 'marshalByRefProxy' on the external configuration, or the ComponentProxyBehaviorAttribute
- Fixed IOC-79: "Kernel.GetHandlers(Type) does not consider generic handlers when satisfying the type"
- Updated StartableFacilityTestCase to correctly demonstrate the facility and added a unit test to demonstrate IOC-80
- Applied Alex Henderson's patch that makes the ComponentModel available to the ILifestyleManager
- Applied Adam Mills's patch fixing IOC-74: "BinaryComponentName VisitNode null check"
- Fixed IOC-67: "RemoveComponent needs to unwire handlers and remove them"
- Fixed IOC-59: "Child component unable to correctly resolve parent service added after the component"
- Fixed IOC-47: "Components created by FactoryActivator have their dependencies checked"
- Applied Marcus Widerberg's patch fixing FACILITIES-84: "FactorySupport - Allow parameters to factory method to be set at resolvetime"
- Applied Marcus Widerberg's patch fixing FACILITIES-82: "Programmatic configuration for FactorySupport"
- Reverted by Henry -> Apply patch from Sam Camp that fixes problems with Remoting Facility Sample and RecoverableComponent. 
- Updated TypedFactoryFacility to not require a target instance when proxying.
- Added Windsor proxy support to create proxies without targets.
- Removed relationship between ProxyOptions and ProxyGeneration options
  and moved ProxyOptions into the MicroKernel.  ProxyGeneration options
  are created from the ProxyOptions and will probably need to be updated
  as facilities demand more proxy generation customizations.
- Added ProxyOptions to allow facilities to easily add proxy interfaces
  without having to create custom proxy factories.  The ProxyOptions
  are obtained via the ProxyUtil.
- Fixed IOC-65
  "DictionaryConverter should use the alternate overload of the 
   PerformConversion method in order to support dictionaries that contain 
   custom types"
- Moved ProxyComponentInspector from Castle.MicroKernel to here and added
  support to supply ProxyGenerationOptions on a ComponentModel basis.  This
  provides the needed ability to provide proxy options in facilities.
- Fixed IOC-69 - DefaultDependencyResolver issue with Service Overrides.
- Added ComponentProxyBehaviorAttribute and ComponentProxyInspector 
  to control the creation of component proxies.
- Added eval support to configuration. Currently it only supports 
  BaseDirectory as a content to evaluate
  <?eval $BaseDirectory ?>
- Added IEnvironmentInfo in an attempt to solve complex 
  configuration/environment issues.
- Fixing IOC-63 - source order of constructors should not matter
- Fixed IOC-62: "Interceptors don't work properly on generic components"
- Applied Norbert Wagner's patch fixing IOC-55: "Generic Type Converters: Set default entry types to generic arguments of property type"
- Applied Jeff Brown's patch fixing IOC-54: "Empty component parameter values cause runtime exception during component resolution."
- Applied patch by Bill Pierce that
  - Introduces the WebUserControlComponentActivator
  - Introduces the KeySearchNamingSubSystem
  - Allows you to associate a custom component activator using
    1. componentActivatorType on component node
    2. ComponentActivatorAttribute
  - Allows you to create and configure child containers through the configuration, using
  <configuration>
	<containers>
		<container name="child1">
		  <configuration>
				<facilities>
					...
				</facilities>
				<components>
					...
				</components>
			</configuration>
		</container>
	</containers>
  </configuration>
- Applied AndyD's patch fixing IOC-52: "Remote access to generic components"
- Fixed IOC-45: "Proxying a component that has an interface that is extended from another interface throws an exception"
- Applied patch by Ernst Naezer fixing IOC-37: "Resolving with arguments in Windsor"
- Fixed IOC-43: "Creation of an Attribute in the Kernel that allows one property to be ignored by the dependency builder"
  Introduced DoNotWireAttribute that marks a property and prevents it
  from being considered by the container
- Changed Windsor to use DynamicProxy 2
- Applied patch by Adam Mills fixing IOC-42: "ResolveServices", new method added to IKernel
- Applied patch by Adam Mills fixing IOC-41: "Bug Fix BinaryTreeComponentName - Assumed Lesser nodes went to left"
- Applied patch by Adam Mills fixing IOC-40: "Provided an Implementation for BinaryTreeComponentName.Remove"
- Applied patch by Adam Mills fixing IOC-39: "Fix for Null Reference when accessing empty BinaryTreeComponentName"
- Fixed IOC-35: "Add bootstrap section to configuration file"
- Fixed issue where KeyAlreadyAdded exception would be throw for components accepting two parameters of the same type, without overrides
- Fixed IOC-36
  - "Transient components with multliple constructors throw unresolved dependency exceptions."
  - Removed best candidate reference, as the kernel is dynamic it should not cache best constructors as components can be added or removed at any time
  - Removed Points from candidates, as in a multithreaded scenario this would lead to failures
- Fixed IOC-34: "Cannot use types having their own base type as constructor argument". See revision r2787
- IOC-32, Support generic collections.
  Supported collections are: ICollection<T>, IList<T>, List<T>, IDictionary<K,V>, Dictionary<K,V>, IEnumerable<T> 

## RC 3

- Applied patch by William C. Pierce <wcpierce@gmail.com> adding PerWebRequestAttribute

- Added setter to ReleasePolicy property

- Applied Curtis Schlak's patch fixing IOC-30
  "Add overload to Windsor AddComponent to specify the Lifestyle"

- Refactored AbstractHandler to use IDependencyResolver

- Dependencies can be resolved now in three levels:

  * CreationContext (which now implements ISubDependencyResolver)
  * IHandler (which now implements ISubDependencyResolver)
  * IKernel which is the normal flow

- Implemented IoC-29 using a different approach

- Renamed IKernel.AddComponentWithProperties to AddComponentExtendedProperties.
  The old method name misled the programmer about its purpose.

- Added a PerWebRequestLifestyleManager which creates at most one instance of
  an object per web request.  To use it you must add the following http module

  <httpModules>
	  ...
	  <add name="PerWebRequest" type="Castle.MicroKernel.Lifestyle.PerWebRequestLifestyleManager , Castle.MicroKernel,Version=0.0.1.7, Culture=neutral, PublicKeyToken=407dd0808d44fbdc"/>

  <httpModules>

  The strong name could be omitted if not in the GAC

- Added checks to handle cycles in dependencies graphs and avoid deadly Stack Overflow Exceptions.

- Fixed IOC-24: "Allow user to provide an attribute which will customize how to inspect properties (PropertiesDependenciesModelInspector)"
  Now users can add an 'inspectionBehavior' attribute to the component node that defines
  the inspection strategy. Possible values are
  
  - None: No properties inspection should happen
  - All: All properties will be inspected and collected (on the class and on the superclasses). 
		 This is the default behavior
  - DeclaredOnly: Only properties specified on type are checked (superclasses will be ignored) 

- Added overload to ITypeConvertor that accept the current node configuration as well as the type in CanHandleType()

- Change: Better error message when there's an exception 
  setting up properties

- Fixed IOC-25: Overrides on the configuration should be considered a non-optional dependency

  This fix changes a little the MicroKernel behavior. Now if you specify an service override
  through external configuration, it will be considered a _non-optional_ dependency

- Uri usage replaced by CustomUri which, differently than MS' Uri class, has the same
  behavior on net-1.1, net-2.0 and mono

- EventWiring Facility: now when a publisher is requested, the subscribers
  are automatically started. 
  
  The side effects are: 
  
  - when a subscriber is requested it won't be wired automatically. 
  - There no much sense in having a subscriber with a lifestyle other than singleton
  
  I'm still evaluating this idea. Sometimes wiring only when the subscriber is requested
  might make sense, but supporting both approaches is kinda hard.
  

- Applied patch by Alex Henderson <webmaster@bittercoder.com> adding
	IWindsorContainer.RemoveChildContainer(IWindsorContainer childContainer)
	and IKernel.RemoveChildKernel(IKernel kernel)

- Applied fix by Ahmed. Now defines can be used on properties nodes like

  <properties>
   <?if DEBUG?>
	<item>x</item>
   <?end?>
  </properties>

- Now with DictionaryConverter you can specify the keyType and valueType on each entry (kudos to Ahmed)

- xmlinterpreter will throw an exception if a property is not defined but referenced
using #{propertyName} syntax.(Patch from Ahmed)

- XmlProcessor refactored from XmlInterpreter (kudos to Ahmed)
  Now PI are also supported (don't forget to document this on the wiki)

- Support for nested nodes on the properties. (kudos to Ahmed)
  Example:

  <configuration>
	<properties>
	   <MyComponentParams>
		 <user>Joe</user>
		 <pwd>Doe</pwd>
	   </MyComponentParams>
	 </properties>
	 <components id=??Component
	   <parameters>#{ MyComponentParams }</parameters>
	 </components>
  </configuration>
  
  Will result in 

	 <components id=??Component
	   <parameters>
		 <user>Joe</user>
		 <pwd>Doe</pwd>
	   </parameters>
	 </components>

- Type converter for kernel components. This allows a usage like this:

  <component id="mycomp">
  
	<parameters>
	  <servicelist>
		<list type="IMyService, MyAssembly">
		  <item>${keytocomponent1}</item>
		  <item>${keytocomponent2}</item>
		</list>
	  </servicelist>
	</parameters>

- Removed support for MethodMeta on ComponentModel. The design decision here 
  is to make the facilities interested on it to extend MethodMetaInspector
  reading from a specific node.

## RC 2

- AsyncInitializationContainer introduced. Special container flavor that installs the
  facilities and components using a background thread.

- Support for evaluation of expressions within the xml configuration (kudos to Ahmed)
  The following "statements" are supported:
  
	<define flag="DEBUG" />
	<undef flag="DEBUG"/>
	
	<if defined="DEBUG">
		component/facility nodes
	</if>
	
	<choose>
		<when defined="DEBUG">
			<component id="debug"/>
		</when>
		<when defined="Qa">
			<component id="qa"/>
		</when>
		<when defined="Prod">
			<component id="prod"/>
		</when>
		<otherwise>
			<component id="default"/>
		</otherwise>
	</choose>

- Startable facility: support to specify the attribute startable=true on the configuration

- Better error messages: now the components waiting for dependencies will recursively 
  report what they are waiting for.

- Support for custom lifestyle through configuration (kudos to Bawer Dagdeviren):

  <component id="my.component"
				   type="MyLib.MyComponent, MyLib"
				   lifestyle="custom"
				   customLifestyleType="MyLib.MyCustomLifestyle, MyLib" />

- Added Type converter for enums

- Support to associate configuration nodes to methods. Usage:

	<component>
		<methods>
			<save />
			<save signature="System.String, mscorlib" />
			<save signature="System.String, mscorlib;System.Int32, mscorlib" />
		</methods>
	</component>

  Which is equivalent to

	<component>
		<methods>
			<method name="save" />
			<method name="save" signature="System.String, mscorlib" />
			<method name="save" signature="System.String, mscorlib;System.Int32, mscorlib" />
		</methods>
	</component> 

- IResource introduced (FileResource, AssemblyResource, ConfigResource and UncResource)
  which are accessible through Uris:

  - FileResource:  
	file://pathtofile 
	(For example: file://c:\mydir\file.txt)

  - AssemblyResource:  
	assembly://AssemblyName/ExtendingNamespace/filename 
	(For example: assembly://Castle.Windsor.Tests/Configuration2/include1.xml)

  - ConfigResource:  
	config://sectioname 
	(For example: config://castle will fetch the 
	<configuration><castle> entry in the configuration)

  - UncResource:  
	\\server\file 
	(For example: \\mysharedplace\myconfig.xml)

- IResource, IResourceFactory and IResourceSubSystem introduced

- Ability to use <properties> in configuration files. Usage

	<properties>
		<prop1>prop1 value</prop1>
		<prop2>prop2 value</prop2>
	</properties>
	<facilities>
		<facility id="testidengine" >
			<item>#{prop1}</item>
		</facility>
		<facility id="testidengine2" >
			<item value="#{prop2}"/>
		</facility>
	</facilities>

- Ability to use <include> in configuration files. Usage

  Main file:

	<configuration>
		<include uri="file://include1.xml"/>
	</configuration>

  include1.xml:

	<configuration>
		<components>
			<component id="testidcomponent1">
			</component>
			<component id="testidcomponent2">
			</component>
		</components>
	</configuration>

## Beta 3

- Bug in dependency resolution (when chained) fixed
- Better message description on exceptions related to unresolved dependencies.
- Fixed bug in AddComponentWithProperties

## Beta 2 (2005-04-10)

- Bug fixes
- Configuration object model separated into interpreters and sources
- AbstractFacility added

## Beta 1 (2005-01-21)

- Changed: from #{} to ${} - way of referencing to another component on the configuration.
- Added: support for dictionaries, lists and arrays on the configuration file.
  <component>
	<parameters>
	  <properties>
		<dictionary>
		  <item key="mykey">value</item>
		</dictionary>
	  </properties>
	</parameters>
  </component>
- Added: Component Graph (used by the Remove method and to dispose the components)
- Fixed: Remove method
- Fixed: Windsor: Proxy for components with (service != impl)
