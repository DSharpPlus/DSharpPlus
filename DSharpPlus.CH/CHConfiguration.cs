using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DSharpPlus.CH.Message.Conditions;

namespace DSharpPlus.CH;

public class CHConfiguration
{
    public required Assembly Assembly { get; set; }
    public ServiceCollection? Services { get; set; }
    internal List<Func<IServiceProvider, IMessageCondition>> ConditionBuilders { get; set; } = new();
    public string? Prefix { get; set; }

    public CHConfiguration UseMessageCondition<T>() where T : IMessageCondition
    {
        Type type = typeof(T);
        List<Expression> expressions = new();

        ParameterExpression serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        expressions.Add(serviceProviderParam);
        ConstructorInfo info = type.GetConstructors()[0];
        if (info.GetParameters().Length == 0)
        {
            expressions.Add(Expression.New(info));
        }
        else
        {
            MethodInfo method = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService),
                BindingFlags.Instance | BindingFlags.Public)!;

            List<Expression> parameters = new();
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                parameters.Add(Expression.Call(method, Expression.Constant(typeof(Type),
                    parameter.ParameterType)));
            }

            expressions.Add(Expression.New(info, parameters));
        }

        Func<IServiceProvider, IMessageCondition> func =
            Expression.Lambda<Func<IServiceProvider, IMessageCondition>>(Expression.Block(expressions), false,
                serviceProviderParam).Compile();
        ConditionBuilders.Add(func);

        return this;
    }
}
