# Introduction and further documentation

The .NET implementation of the 23 API has been written to allow you to easily deploy applications, that communicate with 23 solutions. The implementation is sparsly documented with method summaries, but for a concise explanation of specific methods and parameters, refer to the [23 developer documentation](http://www.23developer.com/).

The implementation relies fully on [DotNetOpenAuth](http://www.dotnetopenauth.net/) for OAuth 1.0a communication with the video sites. The commits are currently compiled and distributed with DotNetOpenAuth 3.4.5. Please refer to the official site for documentation and license specifications.

# Interfaces and objects

The .NET API has been subdivided into two external layers; a service layer and a domain layer. The service layer provides a service interface for retrieving and 

# Setting up the API

The .NET API can be set up in two different ways; with fully configured authentication and with runtime authentication. The fully configured authentication requires you to set up a "privileged API account" on the target 23 site, after which the API provider can be instantiated as follows using the authentication credentials and domain name of your 23 site:

    IAPIProvider ServiceProvider = new APIProvider("yourdomain.23video.com", "consumer key", "consumer secret", "access token", "access token secret");

Runtime authentication asks for authentication approval when a request is sent. If you use this implementation, you should recompile the .NET API library with your own implementation of the InMemoryTokenManager, which is derived directly from the DotNetOpenAuth library, in order to handle saving tokens, so your users don't have to accept your authentication over and over again.

For most uses however, the fully configured authentication for a specific site is the way to go, as this allows you to control every aspect of the API, whereas runtime authenticated API requests are subject to access restrictions as per the 23 API documentation.

