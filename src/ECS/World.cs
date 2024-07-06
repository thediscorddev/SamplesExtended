using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Riateu.ECS;

public record struct EntityID(uint ID);
public record struct ComponentID(uint ID);

public class World 
{
    internal static uint ComponentIDTotal;
    internal static uint EntityIDTotal;
    private Dictionary<EntityID, ComponentID[]> entities = new();
    private Dictionary<Type, ComponentID> typeToComponent = new();
    private List<ComponentStorage> storages = new();

    public unsafe void AddComponentToWorld<T>() 
    where T : unmanaged
    {
        var type = typeof(T);
        typeToComponent.Add(type, new ComponentID(ComponentIDTotal++));

        var size = sizeof(T);

        storages.Add(new ComponentStorage(size));
    }

    public EntityBuilder CreateEntity() 
    {
        return new EntityBuilder();
    }

    internal void AddToWorld(EntityID entityID, ComponentID[] ids) 
    {
        entities[entityID] = ids; 
    }
}

public struct EntityBuilder 
{
    private HashSet<ComponentID> addedComponents;
    public EntityBuilder() 
    {
        addedComponents = new();
    }

    public EntityBuilder Add<T>() 
    where T : struct
    {
        return this;
    }

    public EntityID Build(World world) 
    {
        var entity = new EntityID(World.EntityIDTotal++);
        world.AddToWorld(entity, addedComponents.ToArray());
        return entity;
    }
}

public class ComponentStorage 
{
    private ComponentArray components;

    public ComponentStorage(int elementSize) 
    {
        components = new ComponentArray(elementSize);
    }
}

public unsafe class ComponentArray : IDisposable
{
    private nint elements;
    private int capacity;
    private int elementSize;
    private int count;


    public int Size => elementSize;
    public int Count => count;



    public ComponentArray(int elementSize) 
    {
        capacity = 16;
        this.elementSize = elementSize;
        elements = (nint)NativeMemory.Alloc((nuint)(elementSize * capacity));
    }

    public void Add<T>(T component) 
    where T : unmanaged
    {
        if (count >= capacity) 
        {
            Resize();
        }

        ((T*)elements)[count] = component;
        count++;
    }

    public void Remove(int index) 
    {
        if (index != count - 1) 
        {
            NativeMemory.Copy(
                (void*)(elements + ((count - 1) * elementSize)),
                (void*)(elements + (index * elementSize)),
                (nuint)elementSize
            );
        }

        count--;
    }

    private void Resize() 
    {
        capacity *= 2;
        elements = (nint)NativeMemory.Realloc((void*)elements, (nuint)(elements * capacity));
    }

    public void Dispose()
    {
        NativeMemory.Free((void*)elements);
    }
}