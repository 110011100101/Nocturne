# 项目分成三个部分

---

## 目录

基本构成

- [框架(FrameWork)](#框架framework)
- [容器(Container)](#容器container)
- [单元(Unit)](#单元unit)

---

## 框架(FrameWork)

### 框架规则

框架采用依赖注入(DI)以及控制反转(Ioc)实现加载容器,框架与容器之间是强耦合的,并且一个框架只能对应一个容器,这意味着你不需要关心框架的实现,我们建议你将框架作为一个封装好的程序入口,如果需要实现控制流请使用[容器(Container)](#容器container).

为了绕过泛型的类型擦除,所以让接口继承了非泛型接口,`IUnit`接口亦然.

``` Csharp
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

`IFrame`接口采用了协变与逆变来实现对输入与输出的具体控制,需要的时候通过`GetType().GetGenericArguments()`方法获取一个泛型参数数组.我们建议你通过`First()`和`Last()`检查数组的第一项和最后一项,而非直接使用索引.

``` Csharp
class Framework<[MustBeVariant] TInput, [MustBeVariant ]KOutput>(IContainer<TInput, KOutput> container) : IFrame<TInput, KOutput>
{
    public List<IFrame> InFrame { get; } = new() {}; // 规定了以进入形式链接的框架类型
    public List<IFrame> OutFrame { get; } = new() {}; // 规定了以输出形式链接的框架类型
    private readonly IContainer<TInput, KOutput> _container = container;

    public KOutput Main(TInput input)
    {
        return _container.WorkFlow(input);
    }
}
```

### 框架方法

#### KOutput Main(TInput input)

运行注入容器的工作流

---

## 容器(Container)

### 容器规则

容器在框架中作为控制流的执行单元,它同样实现了对输入与输出的控制.
容器通过一个集合来存储单元,在工作流中,他将会按照索引顺序依次执行.

#### 因此强制实现以下规则

- 首个单元必须与框架的输入类型一致
- 执行工作流时末位单元必须与框架的输出类型一致
- 单元间输入与输出遵循首尾相接的原则

``` Csharp
internal interface IContainer<in TInput, out KOutput>
{
    void Add(IUnit unit); // 添加内容
    void Clear(IUnit unit); // 移除内容
    KOutput WorkFlow(TInput input);
}
```

接口定义了`Add()`方法与`Clear()`方法来管理单元,两个方法的入参均为单元,参数为`Variant`类型,这意味着将无法传入接口实现泛型的依赖注入,但是得益于逆变与协变的实现,你依然可以传入一个泛型.

``` Csharp
class Container<[MustBeVariant] TInput, [MustBeVariant] KOutput> : IContainer<TInput, KOutput>
{
    private readonly List<IUnit> units = new List<IUnit>();

    public void Add(IUnit unit)
    {
        if (units.Count == 0)
        {
            Type[] genericArguments = unit.GetType().GetGenericArguments();

            if (genericArguments.First() != typeof(TInput))
            {
                throw new ArgumentException("The TInput of the first unit must match the TInput of the container.");
            }
        }
        else
        {
            // 检查后续unit的TInput是否与前一个unit的KOutput一致
            IUnit lastUnit = units.Last();

            Type[] lastGenericArguments = lastUnit.GetType().GetGenericArguments();
            Type[] unitGenericArguments = unit.GetType().GetGenericArguments();

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
        
        // 这里规定了输出类型必须严格遵循框架
        if (units.Last().GetType().GetGenericArguments().Last() != typeof(KOutput))
        {
            throw new Exception("Ouput type is not match.");
        }

        Variant result = Variant.From(input);

        // 需要轮流执行
        foreach (IUnit unit in units)
        {
            result = unit.ExecuteDynamic(result);
        }

        return result.As<KOutput>();
    }
}
```

### 容器方法

#### void Add(IUnit<Variant,Variant> unit)

该方法用于将单元加入工作流中,加入时会严格检查工作流的输入与输出类型是否符合[规则](#因此强制实现以下规则),不符合规则时将会抛出异常并且不会加入工作流中.

#### void Clear(IUnit<Variant,Variant> unit)

该方法用于清空工作流中的单元,并且此方法是唯一能够移除工作流中单元的方法,这意味着每一次输入都会严格经过`Add()`方法的检查以确保程序的安全性.

#### KOutput WorkFlow(TInput input)

此方法依次执行工作流,每次执行时,前一个单元返回的参数将会作为下一个单元的输入参数.此方法还会在执行前检查末位单元的输出类型来确保出参正确.

---

## 单元(Unit)

单元包含了代码的具体实现,一个单元通常应该执行一个任务,基于Nocturne框架的架构,你可以将单元理解为一个方法.对于单元,真正重要的只有`Execute()`方法,所有其他的设计都为这个方法服务,例如`Unit`类,纯粹只为了实现依赖注入而设计,同时还保证了出入参数的类型控制.

``` Csharp
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

``` Csharp
// 这个类定义了内容
public abstract class Unit<[MustBeVariant] TInput, [MustBeVariant] KOutput>: IUnit<TInput, KOutput>
{
    abstract public KOutput Execute(TInput input);
}
```

---

## 如何使用

准备好一个容器, 填入内容, 最后调用框架的`Main()`方法时注入容器即可.
