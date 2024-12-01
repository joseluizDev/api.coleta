public class AutoMapper
{
   public dynamic Map<TSource, TDestination>(object sourceObjects)
       where TSource : class
       where TDestination : class, new()
   {
      if (sourceObjects is IEnumerable<TSource> sourceList)
      {
         var destinationList = new List<TDestination>();
         foreach (var source in sourceList)
         {
            var destination = MapSingle<TSource, TDestination>(source);
            destinationList.Add(destination);
         }
         return destinationList;
      }
      else if (sourceObjects is TSource source)
      {
         return MapSingle<TSource, TDestination>(source);
      }
      else
      {
         throw new ArgumentException("O parâmetro não é uma lista nem um único objeto do tipo esperado.");
      }
   }

   private static TDestination MapSingle<TSource, TDestination>(TSource source)
       where TSource : class
       where TDestination : class, new()
   {
      if (source == null) return new TDestination();

      var destination = new TDestination();
      var sourceProperties = source.GetType().GetProperties();
      var destinationProperties = destination.GetType().GetProperties();

      foreach (var sourceProperty in sourceProperties)
      {
         var destinationProperty = destinationProperties.FirstOrDefault(p => p.Name == sourceProperty.Name);
         if (destinationProperty != null)
         {
            destinationProperty.SetValue(destination, sourceProperty.GetValue(source));
         }
      }

      return destination;
   }
}
