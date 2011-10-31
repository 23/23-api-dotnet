# Introduction and further documentation

The .NET implementation of the 23 API has been written to allow you to easily deploy applications, that communicate with 23 solutions. The implementation is sparsly documented with method summaries, but for a concise explanation of specific methods and parameters, refer to the [23 developer documentation](http://www.23developer.com/).

The implementation relies fully on [DotNetOpenAuth](http://www.dotnetopenauth.net/) for OAuth 1.0a communication with the video sites. The commits are currently compiled and distributed with DotNetOpenAuth 3.4.5. Please refer to the official site for documentation and license specifications.

# Interfaces and objects

The .NET API has been subdivided into two external layers all contained in the `Visual` namespace; a service layer and a domain layer. The service layer provides service interfaces for retrieving and pushing data to the specific site through a series of methods. The following service layer interfaces exist:

* IAlbumService (default implementation: Visual.AlbumService)
* ICommentService (default implementation: Visual.CommentService)
* IPhotoService (default implementation: Visual.PhotoService)
* ISessionService (default implementation: Visual.SessionService)
* ISiteService (default implementation: Visual.SiteService)
* ITagService (default implementation: Visual.TagService)
* IUserService (default implementation: Visual.UserService)

The constructors of all service interfaces take a single parameter, a reference to an IAPIProvider interface implementation. The IAPIProvider interface describes the communications between the 23 API and the service layer. Therefore you have to instantiate an implemenetation of the interface (default implementation is APIProvider) before being able to do any requests to your services.

Furthermore most methods for listing a series of objects can be overloaded with a ListParameter object, which must be instantied and changed in order to do a certain request. Please refer to "Sample: Get a list of photos and videos" for an explanation.

The domain layer contains domain objects, ie. pure content holders without any functionality, describing different aspects of the method return data. The following domain objects exist, which are returned by some service methods:

* Album (Visual.Domain.Album)
* Comment (Visual.Domain.Comment)
* Photo (Visual.Domain.Photo)
* Session (Visual.Domain.Session)
* Site (Visual.Domain.Site)
* Tag (Visual.Domain.Tag)
* User (Visual.Domain.User)

If a method returns a domain object or a list of these, a failed operation will return a `null`.

# Setting up the API

The .NET API can be set up in two different ways; with fully configured authentication and with runtime authentication. The fully configured authentication requires you to set up a "privileged API account" on the target 23 site, after which the API provider can be instantiated as follows using the authentication credentials and domain name of your 23 site:

    IAPIProvider ServiceProvider = new APIProvider("yourdomain.23video.com", "consumer key", "consumer secret", "access token", "access token secret");

Runtime authentication asks for authentication approval when a request is sent. If you use this implementation, you should recompile the .NET API library with your own implementation of the InMemoryTokenManager, which is derived directly from the DotNetOpenAuth library, in order to handle saving tokens, so your users don't have to accept your authentication over and over again:

    IAPIProvider ServiceProvider = new APIProvider("yourdomain.23video.com", "consumer key", "consumer secret");

For most uses however, the fully configured authentication for a specific site is the way to go, as this allows you to control every aspect of the API, whereas runtime authenticated API requests are subject to access restrictions as per the 23 API documentation.

# Sample: get a list of photos and videos

To get a list of photos by the default API parameters, simple make sure you have your API provider instantiated, and execute the following code:

    IPhotoService _PhotoService = new PhotoService(ServiceProvider);
    List<Domain.Photo> Photos = _PhotoService.GetList();

    if (Photos == null) return; // Handle error

    foreach (Domain.Photo _Photo in Photos)
    {
        // Execute whatever code here...
    }

Where this may be sufficient for some cases, most of the time you're going to need to specify some request parameters, which is done by a ListParameters object. For photos this object is called `PhotoListParameters`, the properties of which can be found by viewing the library source. All parameters are at creation set to the request default, so you only need to change the necessary parameters. If we want to request video objects only, the following request is needed:

    IPhotoService _PhotoService = new PhotoService(ServiceProvider);
    PhotoListParameters ListParameters = new PhotoListParameters()
    {
        Video = true
    };
    List<Domain.Photo> Photos = _PhotoService.GetList(ListParameters);

    if (Photos == null) return; // Handle error

    foreach (Domain.Photo _Photo in Photos)
    {
        // Execute whatever code here...
    }

The methodology can be replicated on to any service, that allows you to get an object list.

# Sample: create a user

To create a user, you simply need to instantiate an `IUserService` interface and use the `Create` method with the overload that describes the parameters, you have available. In this sample, it is expected that all parameters are available, in any other case you must supply a null value. The method returns an integer value representing the user id of the created user, or null if it fails.

    IPhotoService _UserService = new UserService(ServiceProvider);
    int? NewUserId = _UserService.Create(
        "email@address.tld", // The e-mail address of the user
        "username", // The username of the user. Must be at least 4 characters
        "secret", // The password of the user
        "Ernest Johanson", // The full name of the user
        Timezone.CET, // The timezone of the user. Refer to the library implementation of the Timezone enumerator for possible values
        false // We don't want this user to be a site administrator
    );

    if (NewUserId == null)
    {
        // We failed somehow...
    }
    else
    {
        // Do whatever is necessary to NewUserId
    }