using Pillowfy.DTOs;
using Pillowfy.Models;

namespace Pillowfy.Factory
{
    public interface IChambreFactory
    {
        Chambre Create(ChambreCreateDto dto);
    }
}