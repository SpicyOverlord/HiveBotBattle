using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HiveMind;

public class HiveMindCollector
{
    public static List<(string name,IHiveMind hiveMind)> CollectHiveMindAndNames()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        IEnumerable<Type> types = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IHiveMind)) && !t.IsAbstract);

        List<(string name,IHiveMind hiveMind)> hiveMinds = new List<(string name,IHiveMind hiveMind)>();
        foreach (var type in types)
        {
            GD.Print("Found HiveMind: " + type.Name);
            IHiveMind instance = (IHiveMind)Activator.CreateInstance(type);
            hiveMinds.Add((name: type.Name, instance));
        }
        return hiveMinds;
    }
}
