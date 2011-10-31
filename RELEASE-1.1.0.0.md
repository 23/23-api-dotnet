# Release notes for 23 API for .NET version 1.1.0.0

The following changes have been made to the API implementation for version 1.1.0.0:

* API refactored to target .NET 3.5 per default
* Namespace for API implementation changed from Twentythree to Visual for concurrency with general platform naming rules
* Interface and implementation IAPIProvider and APIProvider refactored to be lower camel case compliant; IApiProvider and ApiProvider respectively
* ISiteService.GetSite renamed to ISiteService.Get for concurrency
* Added a Get method to IUserService, IPhotoService and IAlbumService
* PhotoList.IncludeUnpublised remaned to correctly be named PhotoList.IncludeUnpublished
* Fixed a bug only allowing relative filenames for uploads to be passed. Absolute filenames are now allowed too
* General refactoring of internals