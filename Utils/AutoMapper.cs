
public class AutoMapper
{
   // Método para mapear uma lista de entidades para uma lista de DTOs de forma dinâmica
   public static List<TDto> MapEntitiesToDtoList<TEntity, TDto>(List<TEntity> entities)
       where TEntity : class
       where TDto : class, new()
   {
      var dtoList = new List<TDto>();

      foreach (var entity in entities)
      {
         var dto = MapEntityToDto<TEntity, TDto>(entity);
         dtoList.Add(dto);
      }

      return dtoList;
   }

   public static TDto MapEntityToDto<TEntity, TDto>(TEntity entity)
       where TEntity : class
       where TDto : class, new()
   {
      if (entity == null) return new TDto();

      var dto = new TDto();

      var entityProperties = entity.GetType().GetProperties();
      var dtoProperties = dto.GetType().GetProperties();

      foreach (var entityProperty in entityProperties)
      {
         var dtoProperty = dtoProperties.FirstOrDefault(p => p.Name == entityProperty.Name);
         if (dtoProperty != null)
         {
            dtoProperty.SetValue(dto, entityProperty.GetValue(entity));
         }
      }

      return dto;
   }
}