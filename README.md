# SiteAwake
Demonstration of software architecture concepts using a working .NET solution by Dave Naile.

SiteAwake is a web application / console application that solves the issue of hosted sites that allow the ASP.NET worker process to sleep. When this happens, it can take 15+ seconds to wake the app back up upon a page request. 

The web application is C# ASP.NET MVC that uses Microsoft Identity Provider, OWIN, Unity, bootstrap, AngularJS, Entity Framework, log4Net and more. It allows a user to sign up and register a website to be pinged every N minutes to ensure the site stays awake.

The console application (SiteAwake.TestHarness) calls SiteAwake.WakeUpCallProcessor, which pings all the active URLs retrieved from the MS SQL database. The next step would be to convert this console app into an Azure WebJob to be deployed with the web app to an Azure App Service.

There are many great examples of high level, best practice architecture throughout. Specifically, the use of Inversion of Control (IoC) in the Domain layer allows the Entity Framework Data Context to be injected into the application. Although not completed in the UI, a payment gateway exists and is also injected into the app. Additionally, there is no need to restore or publish a database from the database project. Microsoft Identity Proivder takes care of database creation upon the first data access request, such as signing up.
