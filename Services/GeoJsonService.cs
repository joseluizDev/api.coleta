using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;

namespace api.coleta.Services;

public class GeoJsonService : ServiceBase
{
    private readonly GeoJsonRepository _geoJsonRepository;
    public GeoJsonService(GeoJsonRepository geoJsonRepository, IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
        _geoJsonRepository = geoJsonRepository;
    }

    public Geojson? SalvarSafra(Geojson geojson)
    {
        Geojson geojsonRetorno = _geoJsonRepository.Adicionar(geojson);
        if (geojsonRetorno != null)
        {
            return geojsonRetorno;
        }

        return null;
    }
    
}
