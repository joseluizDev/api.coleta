using AutoMapper;

public class AutoMapperService
{
    private readonly IMapper _mapper;

    public AutoMapperService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public List<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> sourceObjects)
    {
        return _mapper.Map<List<TDestination>>(sourceObjects);
    }

    public TDestination MapSingle<TSource, TDestination>(TSource sourceObject)
    {
        return _mapper.Map<TDestination>(sourceObject);
    }
}
