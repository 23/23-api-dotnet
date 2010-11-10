using System.Collections.Generic;

namespace Visual
{
    public interface ITagService
    {
        List<Domain.Tag> GetList();
        List<Domain.Tag> GetList(TagListParameters requestParameters);
        List<Domain.Tag> GetRelatedList(string name);
    }
}