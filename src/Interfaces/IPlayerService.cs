using System.Collections.Generic;

namespace Visual
{
    public interface IPlayerService
    {
        List<Domain.Player> GetList();
    }
}