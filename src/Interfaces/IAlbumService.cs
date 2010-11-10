using System.Collections.Generic;

namespace Visual
{
    public interface IAlbumService
    {
        Domain.Album Get(int albumId);
        List<Domain.Album> GetList();
        List<Domain.Album> GetList(AlbumListParameters requestParameters);
        int? Create(string title);
        int? Create(string title, string description);
        int? Create(string title, string description, bool hide);
        int? Create(string title, string description, bool hide, int? userId);
        bool Update(int albumId, string title);
        bool Update(int albumId, string title, string description);
        bool Update(int albumId, string title, string description, bool hide);
        bool Delete(int albumId);
    }
}