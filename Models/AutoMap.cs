using System;
using System.Linq;
using System.Reflection;

public static class AutoMap
{
   public static TDestination Map<TSource, TDestination>(TSource source)
       where TSource : class
       where TDestination : class, new()
   {
      if (source == null) throw new ArgumentNullException(nameof(source));

      var destination = new TDestination();

      var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
      var destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance);

      foreach (var sourceProperty in sourceProperties)
      {
         var destinationProperty = destinationProperties.FirstOrDefault(p =>
             p.Name == sourceProperty.Name && p.PropertyType == sourceProperty.PropertyType);

         if (destinationProperty != null && destinationProperty.CanWrite)
         {
            var value = sourceProperty.GetValue(source);
            destinationProperty.SetValue(destination, value);
         }
      }

      return destination;
   }

   public static void Map<TSource, TDestination>(TSource source, TDestination destination)
       where TSource : class
       where TDestination : class
   {
      if (source == null) throw new ArgumentNullException(nameof(source));
      if (destination == null) throw new ArgumentNullException(nameof(destination));

      var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
      var destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance);

      foreach (var sourceProperty in sourceProperties)
      {
         var destinationProperty = destinationProperties.FirstOrDefault(p =>
             p.Name == sourceProperty.Name && p.PropertyType == sourceProperty.PropertyType);

         if (destinationProperty != null && destinationProperty.CanWrite)
         {
            var value = sourceProperty.GetValue(source);
            destinationProperty.SetValue(destination, value);
         }
      }
   }
}
