#IronVelocity

[![Build status](https://ci.appveyor.com/api/projects/status/4w106h0epo497h6d/branch/master?svg=true)](https://ci.appveyor.com/project/afscrome/ironvelocity/branch/master)

IronVelocity is an implementation of the Velocity Templating Language (VTL) for .Net.

You can find out more details on Velocity (including the syntax) at https://velocity.apache.org/engine/releases/velocity-1.5/user-guide.html


##Isn't that like Razor?

Razor is a very good markup language, with very good tooling in Visual Studio.  This makes it a very good language for developers, however it can be a bit complicated if you want templates to be modifyable by non developers.  A good example of where Velocity would be more suitable than Razor is a multi-tenant web application, in which you allow applicaiton owners to modify html fragments.  Your users may have some technical expertiese without being developers, in which case the simpler Velocity syntax is easier to understand, and less error prone.  Additional Velocity only allows code to be executed on the objects passed in to the template, whereas Razor would allow any arbitary .net code to be executed.  

##What about NVelocity

NVelocity is another implementation of Velocity for .Net.  IronVelocity stil uses NVelocity for the parsing of the template, but differs in how it executes templates.  The major differences are as follows:

* [+] IronVelocity compiles templates (as opposed to NVelocity which interprets the templates) which results in faster template execution (although at the cost of a higher startup time for the first execution)
* [+] IronVelocity uses the DLR for method and property invocation which is faster, but also means you can use dynamic objects in velocity templates
* [+] IronVelocity uses generic collections for lists and dictionaries which can result in nicer code when you pass velocity objects through to .net code.
* [-] IronVelocity doesn't currently support the #Parse or #Include directives. It also only has limited support for velocimacros (i.e. only enough to get other regression tests to pass).
