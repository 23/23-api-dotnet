using System.Collections.Generic;

namespace Visual
{
    public interface IPhotoService
    {
        Domain.Photo Get(int photoId);
        Domain.Photo Get(int photoId, bool includeUnpublished);
        List<Domain.Photo> GetList();
        List<Domain.Photo> GetList(PhotoListParameters requestParameters);
        int? Upload(string filename, string fileContentType, System.IO.Stream filestream);
        int? Upload(string filename, string fileContentType, System.IO.Stream filestream, int? userId);
        int? Upload(string filename, string fileContentType, System.IO.Stream filestream, int? userId, int? albumId);
        int? Upload(string filename, string fileContentType, System.IO.Stream filestream, int? userId, int? albumId, string title);
        int? Upload(string filename, string fileContentType, System.IO.Stream filestream, int? userId, int? albumId, string title, string description);
        int? Upload(string filename, string fileContentType, System.IO.Stream filestream, int? userId, int? albumId, string title, string description, string tags);
        int? Upload(string filename, string fileContentType, System.IO.Stream filestream, int? userId, int? albumId, string title, string description, string tags, bool? publish);
        bool Delete(int photoId);
        bool Replace(int photoId, string filename, string fileContentType, System.IO.Stream filestream);
        bool Update(int photoId, int? albumId);
        bool Update(int photoId, int? albumId, string title);
        bool Update(int photoId, int? albumId, string title, string description);
        bool Update(int photoId, int? albumId, string title, string description, string tags);
        bool Update(int photoId, int? albumId, string title, string description, string tags, bool? published);
        Domain.PhotoUploadToken GetUploadToken(string returnUrl, bool? backgroundReturn, int? userId, int? albumId, string title, string description, string tags, bool? publish, int? validMinutes, int? maxUploads);
        bool RedeemUploadToken(string filename, string fileContentType, System.IO.Stream filestream, string uploadToken);
    }
}