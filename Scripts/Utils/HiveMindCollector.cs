using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HiveMind;

public class HiveMindCollector
{
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
}
