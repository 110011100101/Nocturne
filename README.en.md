# Project Divided into Three Components

---

## Table of Contents

Basic Structure

- [Framework](#framework)
- [Container](#container)
- [Unit](#unit)

---

## Framework

### Framework Rules

The framework utilizes Dependency Injection (DI) and Inversion of Control (IoC) to load containers. The framework and container are tightly coupled, with one framework corresponding exclusively to one container. This design means you don't need to concern yourself with framework implementation. We recommend treating the framework as an encapsulated program entry point. For control flow implementation, use the [Container](#container).

To bypass type erasure with generics, the interface inherits from a non-generic interface, as does the `IUnit` interface.

```csharp
internal interface IFrame
{
    Variant ExecuteDynamic(Variant input);
}

internal interface IFrame<[MustBeVariant] in TInput, [MustBeVariant] out KOutput> : IFrame
{
    KOutput Main(TInput input);

    Variant IFrame.ExecuteDynamic(Variant input)
    {
        return Variant.From(Main(input.As<TInput>()));
    }
}
```

The `IFrame` interface employs covariance and contravariance to precisely control input and output types. When needed, use `GetType().GetGenericArguments()` to obtain a generic type argument array. We recommend checking the first and last items using `First()` and `Last()` rather than direct indexing.

```csharp
class Framework<[MustBeVariant] TInput, [MustBeVariant] KOutput>(IContainer<TInput, KOutput> container) : IFrame<TInput, KOutput>
{
    public List<IFrame> InFrame { get; } = new() {}; // Specifies frameworks linked as inputs
    public List<IFrame> OutFrame { get; } = new() {}; // Specifies frameworks linked as outputs
    private readonly IContainer<TInput, KOutput> _container = container;

    public KOutput Main(TInput input)
    {
        return _container.WorkFlow(input);
    }
}
```

### Framework Methods

#### KOutput Main(TInput input)

Executes the workflow of the injected container.

---

## Container

### Container Rules

The container serves as the execution unit for control flow within the framework, also implementing input/output control. It stores units in a collection and executes them sequentially by index during workflow.

#### Therefore, the following rules are enforced

- The first unit's input type must match the framework's input type
- During workflow execution, the last unit's output must match the framework's output type
- Units follow a chain where each unit's output becomes the next unit's input

```csharp
internal interface IContainer<in TInput, out KOutput>
{
    void Add(IUnit unit); // Adds content
    void Clear(IUnit unit); // Removes content
    KOutput WorkFlow(TInput input);
}
```

The interface defines `Add()` and `Clear()` methods for unit management. Both methods accept units as parameters of `Variant` type, which prevents generic dependency injection through interface implementation. However, thanks to covariance/contravariance, you can still pass generics.

```csharp
class Container<[MustBeVariant] TInput, [MustBeVariant] KOutput> : IContainer<TInput, KOutput>
{
    private readonly List<IUnit> units = new List<IUnit>();

    public void Add(IUnit unit)
    {
        if (units.Count == 0)
        {
            Type[] genericArguments = unit.GetType().BaseType.GetGenericArguments();

            if (genericArguments.First() != typeof(TInput))
            {
                throw new ArgumentException("The TInput of the first unit must match the TInput of the container.");
            }
        }
        else
        {
            // Verify subsequent unit's TInput matches previous unit's KOutput
            IUnit lastUnit = units.Last();

            Type[] lastGenericArguments = lastUnit.GetType().BaseType.GetGenericArguments();
            Type[] unitGenericArguments = unit.GetType().BaseType.GetGenericArguments();

            if (lastGenericArguments.Last() != unitGenericArguments.First())
            {
                throw new ArgumentException("The TInput of the unit must match the KOutput of the previous unit.");
            }
        }

        units.Add(unit);
    }

    public void Clear(IUnit unit)
    {
        units.Clear();
    }

    public KOutput WorkFlow(TInput input)
    {
        if (units.Count == 0)
        {
            throw new Exception("There is no unit in the container.");
        }
        
        // Enforces strict output type compliance with framework
        if (units.Last().GetType().BaseType.GetGenericArguments().Last() != typeof(KOutput))
        {
            throw new Exception("Ouput type is not match.");
        }

        Variant result = Variant.From(input);

        // Sequential execution
        foreach (IUnit unit in units)
        {
            result = unit.ExecuteDynamic(result);
        }

        return result.As<KOutput>();
    }
}
```

### Container Methods

#### void Add(IUnit<Variant,Variant> unit)

Adds a unit to the workflow, strictly validating input/output types against the [rules](#therefore-the-following-rules-are-enforced). Throws an exception and rejects invalid units.

#### void Clear(IUnit<Variant,Variant> unit)

Clears all units from the workflow. This is the only method to remove units, ensuring every addition undergoes `Add()` validation for program safety.

#### KOutput WorkFlow(TInput input)

Executes the workflow sequentially, passing each unit's output as the next unit's input. Verifies the final unit's output type before execution to ensure correct return types.

---

## Unit

Units contain concrete implementation code. Each unit should typically perform a single task. In Nocturne's architecture, you can think of a unit as a method. The `Execute()` method is the only truly essential component - all other designs exist to support this method. For example, the `Unit` class exists purely to enable dependency injection while maintaining type control for inputs/outputs.

```csharp
internal interface IUnit
{
    Variant ExecuteDynamic(Variant input);
}

internal interface IUnit<[MustBeVariant] in TInput, [MustBeVariant] out KOutput> : IUnit
{
    KOutput Execute(TInput input);

    Variant IUnit.ExecuteDynamic(Variant input)
    {
        return Variant.From(Execute(input.As<TInput>()));
    }
}
```

```csharp
// This class defines the content
public abstract class Unit<[MustBeVariant] TInput, [MustBeVariant] KOutput>: IUnit<TInput, KOutput>
{
    abstract public KOutput Execute(TInput input);
}
```

---

## How to Use

Prepare a container, populate it with content, then inject the container when calling the framework's `Main()` method.
