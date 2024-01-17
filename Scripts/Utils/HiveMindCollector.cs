using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HiveMind;

public class HiveMindCollector
{
    /// <summary>
    /// Collects the names and types of all available HiveMinds in the project.
    /// </summary>
    /// <returns>A list of tuples containing the name and type of each HiveMind.</returns>
    public static List<(string name,Type hiveMindType)> CollectHiveMindAndNames()
    {
        
        Assembly assembly = Assembly.GetExecutingAssembly();
        IEnumerable<Type> types = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IHiveMind)) && !t.IsAbstract);

        List<(string name,Type hiveMindType)> hiveMinds = new List<(string name,Type hiveMindType)>();
        foreach (var type in types)
        {
            GD.Print("Found HiveMind: " + type.Name);
            hiveMinds.Add((name: type.Name, hiveMindType: type));
        }
        return hiveMinds;
    }


    /// <summary>
    /// Instantiate an implementation of IHiveMind from a given HiveMind type.
    /// </summary>
    public static IHiveMind InstantiateHiveMind(Type hiveMindType) => (IHiveMind)Activator.CreateInstance(hiveMindType);

}
