using System.Collections.Generic;

namespace Visual
{
    public interface ICommentService
    {
        List<Domain.Comment> GetList();
        List<Domain.Comment> GetList(CommentListParameters requestParameters);
    }
}