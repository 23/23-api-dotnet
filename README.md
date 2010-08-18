# Introduction and further documentation

The .NET implementation of the 23 API has been written to allow you to easily deploy applications, that communicate with 23 solutions. The implementation is sparsly documented with method summaries, but for a concise explanation of specific methods and parameters, refer to the [23 developer documentation](http://www.23developer.com/).

The implementation relies fully on [DotNetOpenAuth](http://www.dotnetopenauth.net/) for OAuth 1.0a communication with the video sites. The commits are currently compiled and distributed with DotNetOpenAuth 3.4.5. Please refer to the official site for documentation and license specifications.

# Interfaces and objects

The .NET API has been subdivided into two external layers all contained in the `Twentythree' namespace; a service layer and a domain layer. The service layer provides service interfaces for retrieving and pushing data to the specific site through a series of methods. The following service layer interfaces exist:

* IAlbumService (default implementation: Twentythree.AlbumService)
* ICommentService (default implementation: Twentythree.CommentService)
* IPhotoService (default implementation: Twentythree.PhotoService)
* ISessionService (default implementation: Twentythree.SessionService)
* ISiteService (default implementation: Twentythree.SiteService)
* ITagService (default implementation: Twentythree.TagService)
* IUserService (default implementation: Twentythree.UserService)

The constructors of all service interfaces take a single parameter, a reference to an IAPIProvider interface implementation. The IAPIProvider interface describes the communications between the 23 API and the service layer. Therefore you have to instantiate an implemenetation of the interface (default implementation is APIProvider) before being able to do any requests to your services.

Furthermore most methods for listing a series of objects can be overloaded with a ListParameter object, which must be instantied and changed in order to do a certain request. Please refer to "Sample: Get a list of videos" for an explanation.

The domain layer contains domain objects, ie. pure content holders without any functionality, describing different aspects of the method return data. The following domain objects exist, which are returned by some service methods:

* Album (Twentythree.Domain.Album)
* Comment (Twentythree.Domain.Comment)
* Photo (Twentythree.Domain.Photo)
* Session (Twentythree.Domain.Session)
* Site (Twentythree.Domain.Site)
* Tag (Twentythree.Domain.Tag)
* User (Twentythree.Domain.User)

# Setting up the API

The .NET API can be set up in two different ways; with fully configured authentication and with runtime authentication. The fully configured authentication requires you to set up a "privileged API account" on the target 23 site, after which the API provider can be instantiated as follows using the authentication credentials and domain name of your 23 site:

    IAPIProvider ServiceProvider = new APIProvider("yourdomain.23video.com", "consumer key", "consumer secret", "access token", "access token secret");

Runtime authentication asks for authentication approval when a request is sent. If you use this implementation, you should recompile the .NET API library with your own implementation of the InMemoryTokenManager, which is derived directly from the DotNetOpenAuth library, in order to handle saving tokens, so your users don't have to accept your authentication over and over again:

    IAPIProvider ServiceProvider = new APIProvider("yourdomain.23video.com", "consumer key", "consumer secret");

For most uses however, the fully configured authentication for a specific site is the way to go, as this allows you to control every aspect of the API, whereas runtime authenticated API requests are subject to access restrictions as per the 23 API documentation.

