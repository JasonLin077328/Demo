using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 設定ViewModel要對應的Model。
/// 這個用預設的Convention來對應
/// </summary>
/// <typeparam name="T">要被對應到的Type</typeparam>
public interface IMapFrom<T>
{
}
/// <summary>
/// 註冊有設定AutoMapper的viewmodel
/// </summary>
public class AutoMapperConfig
{
    /// <summary>
    /// 要執行的邏輯
    /// </summary>
    public static void Execute()
    {
        var typeOfIMapFrom = typeof(IMapFrom<>);

        // 這個predicate 的條件和下面個別mapping的第一個條件是一致的。
        Func<Type, bool> predicate = (t => t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeOfIMapFrom).Any()); // 找到符合IMapFrom<>
        
        var types = System.Reflection.Assembly.GetCallingAssembly().GetExportedTypes()
               .Where(predicate).ToList();


       

        LoadStandardMappings(types);
    }

    /// <summary>
    /// Loads the standard mappings.
    /// </summary>
    /// <param name="types">The types.</param>
    private static void LoadStandardMappings(IEnumerable<Type> types)
    {
        var maps = (from t in types
                    from i in t.GetInterfaces()
                    where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>) &&
                           !t.IsAbstract &&
                           !t.IsInterface
                    select new
                    {
                        Source = i.GetGenericArguments()[0],
                        Destination = t
                    }).ToArray();
        
        Mapper.Initialize(cfg =>
        {            
            
            foreach (var map in maps)
            {
                cfg.CreateMap(map.Source, map.Destination).IgnoreAllPropertiesWithAnInaccessibleSetter();
                cfg.CreateMap(map.Destination, map.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
            }
        });

    }
}

