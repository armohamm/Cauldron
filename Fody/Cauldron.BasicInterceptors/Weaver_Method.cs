﻿using Cauldron.Interception.Cecilator;
using Cauldron.Interception.Cecilator.Coders;
using Cauldron.Interception.Fody;
using Cauldron.Interception.Fody.HelperTypes;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Weaver_Method
{
    public static string Name = "Method Interceptors";
    public static int Priority = 0;
    private static IEnumerable<BuilderType> methodInterceptionAttributes;

    static Weaver_Method()
    {
        methodInterceptionAttributes =
            Builder.Current.FindAttributesByInterfaces("Cauldron.Interception.IMethodInterceptor")
            .Concat(Builder.Current.FindAttributesByInterfaces("Cauldron.Interception.ISimpleMethodInterceptor"));
    }

    [Display("Type-Wide Method Interception")]
    public static void ImplementTypeWideMethodInterception(Builder builder) => ImplementTypeWideMethodInterception(builder, methodInterceptionAttributes);

    [Display("Method Interception")]
    public static void InterceptMethods(Builder builder)
    {
        if (!methodInterceptionAttributes.Any())
            return;

        var asyncTaskMethodBuilder = new __AsyncTaskMethodBuilder();
        var asyncTaskMethodBuilderGeneric = new __AsyncTaskMethodBuilder_1();
        var syncRoot = new __ISyncRoot();
        var task = new __Task();
        var exception = new __Exception();

        var methods = builder
            .FindMethodsByAttributes(methodInterceptionAttributes)
            .Where(x => !x.Method.IsPropertyGetterSetter)
            .GroupBy(x => new MethodKey(x.Method, x.AsyncMethod))
            .Select(x => new MethodBuilderInfo<MethodBuilderInfoItem<__IMethodInterceptor, __ISimpleMethodInterceptor>>(x.Key,
                x.Select(y => new MethodBuilderInfoItem<__IMethodInterceptor, __ISimpleMethodInterceptor>(y, __IMethodInterceptor.Instance, __ISimpleMethodInterceptor.Instance))))
            .OrderBy(x => x.Key.Method.DeclaringType.Fullname)
            .ToArray();

        foreach (var method in methods)
        {
            if (method.Items == null || method.Items.Length == 0 || method.Key.Method.IsAbstract)
                continue;

            builder.Log(LogTypes.Info, $"Implementing method interceptors: {method.Key.Method.DeclaringType.Name.PadRight(40, ' ')} {method.Key.Method.Name}({string.Join(", ", method.Key.Method.Parameters.Select(x => x.Name))})");

            var targetedMethod = method.Key.AsyncMethod ?? method.Key.Method;
            var attributedMethod = method.Key.Method;
            var hasSimpleMethodInterceptors = method.Items.Any(x => x.HasInterfaceB);
            var hasFullMethodInterceptors = method.Items.Any(x => x.HasInterfaceA);

            if (method.RequiresSyncRootField)
            {
                if (method.SyncRoot.IsStatic)
                    method.Key.Method.DeclaringType.CreateStaticConstructor().NewCoder()
                        .SetValue(method.SyncRoot, x => x.NewObj(builder.GetType(typeof(object)).Import().ParameterlessContructor))
                        .Insert(InsertionPosition.Beginning);
                else
                    foreach (var ctors in method.Key.Method.DeclaringType.GetRelevantConstructors().Where(x => x.Name == ".ctor"))
                        ctors.NewCoder().SetValue(method.SyncRoot, x => x.NewObj(builder.GetType(typeof(object)).Import().ParameterlessContructor))
                            .Insert(InsertionPosition.Beginning);
            }

            var coder = targetedMethod
                .NewCoder()
                .Context(x =>
                {
                    for (int i = 0; i < method.Items.Length; i++)
                    {
                        var item = method.Items[i];
                        var alwaysCreateNewInstance = item.InterceptorInfo.AlwaysCreateNewInstance;
                        var methodCoder = method.Key.Method.IsAsync ? method.Key.Method.NewCoder() : x;

                        Coder codeInterceptorInstance(Coder interceptorInstanceCoder)
                        {
                            interceptorInstanceCoder.SetValue(item.Interceptor, z => z.NewObj(item.Attribute));
                            if (item.HasSyncRootInterface)
                                interceptorInstanceCoder.Load<ICasting>(item.Interceptor).As(__ISyncRoot.Type).To<ICallMethod<CallCoder>>().Call(syncRoot.SyncRoot, method.SyncRoot);

                            ModuleWeaver.ImplementAssignMethodAttribute(builder, method.Items[i].AssignMethodAttributeInfos, item.FieldOrVariable, item.Attribute.Attribute.Type, interceptorInstanceCoder);
                            return interceptorInstanceCoder;
                        }

                        if (alwaysCreateNewInstance)
                            codeInterceptorInstance(methodCoder);
                        else
                            methodCoder.If(y => y.Load<IRelationalOperators>(item.Interceptor).IsNull(), y => codeInterceptorInstance(y));

                        if (method.Key.Method.IsAsync) methodCoder.Insert(InsertionPosition.Beginning);
                        item.Attribute.Remove();
                    }

                    return x;
                });

            if (hasSimpleMethodInterceptors)
            {
                var simpleMethodInterceptors = method.Items.Where(x => x.HasInterfaceB).ToArray();
                coder.Context(x =>
                {
                    for (int i = 0; i < simpleMethodInterceptors.Length; i++)
                        x.Load<ICallMethod<CallCoder>>(simpleMethodInterceptors[i].FieldOrVariable).Call(simpleMethodInterceptors[i].InterfaceB.OnEnter, attributedMethod.OriginType, CodeBlocks.This, attributedMethod,
                            method.Key.Method.Parameters.Length > 0 ? x.GetParametersArray() : null);

                    if (!hasFullMethodInterceptors)
                        return x.OriginalBody();

                    return x;
                });
            }

            var fullMethodInterceptors = method.Items.Where(x => x.HasInterfaceA).ToArray();
            if (hasFullMethodInterceptors)
            {
                var tryCoder = coder.Try(x =>
                    {
                        for (int i = 0; i < fullMethodInterceptors.Length; i++)
                            x.Load<ICallMethod<CallCoder>>(fullMethodInterceptors[i].FieldOrVariable).Call(fullMethodInterceptors[i].InterfaceA.OnEnter, attributedMethod.OriginType, CodeBlocks.This, attributedMethod,
                                method.Key.Method.Parameters.Length > 0 ? x.GetParametersArray() : null);

                        return x.OriginalBody();
                    });

                if (method.Key.AsyncMethod == null)
                    tryCoder.Catch(__Exception.Type, (eCoder, e) => eCoder.If(x =>
                        {
                            var or = x.Load<ICallMethod<BooleanExpressionCallCoder>>(fullMethodInterceptors[0].FieldOrVariable).Call(fullMethodInterceptors[0].InterfaceA.OnException, e());
                            for (int i = 1; i < fullMethodInterceptors.Length; i++)
                                or.Or(y => y.Load<ICallMethod<CallCoder>>(fullMethodInterceptors[i].FieldOrVariable).Call(fullMethodInterceptors[i].InterfaceA.OnException, e()));

                            return or.Is(true);
                        }, then => eCoder.NewCoder().Rethrow())
                            .DefaultValue().Return());

                tryCoder.Finally(x =>
                {
                    for (int i = 0; i < fullMethodInterceptors.Length; i++)
                        x.Load<ICallMethod<CallCoder>>(fullMethodInterceptors[i].FieldOrVariable).Call(fullMethodInterceptors[i].InterfaceA.OnExit);

                    return x;
                })
                .EndTry();
            }

            coder.Return().Replace();

            if (method.Key.AsyncMethod != null && hasFullMethodInterceptors)
            {
                // Special case for async methods
                var exceptionBlock = method.Key.Method.AsyncMethodHelper.GetAsyncStateMachineExceptionBlock();
                targetedMethod
                    .NewCoder().Context(context =>
                    {
                        var exceptionVariable = method.Key.Method.AsyncMethodHelper.GetAsyncStateMachineExceptionVariable();

                        return context.If(x =>
                         {
                             var or = x.Load(fullMethodInterceptors[0].FieldOrVariable as Field).Call(fullMethodInterceptors[0].InterfaceA.OnException, exceptionVariable);

                             for (int i = 1; i < fullMethodInterceptors.Length; i++)
                                 or.Or(y => y.Load(fullMethodInterceptors[i].FieldOrVariable as Field).Call(fullMethodInterceptors[i].InterfaceA.OnException, exceptionVariable));

                             return or.Is(false);
                         }, x => x.Jump(exceptionBlock.Item1.End));
                    }).Insert(InsertionAction.After, exceptionBlock.Item1.Beginning);
            }
        };
    }

    internal static void ImplementTypeWideMethodInterception(Builder builder, IEnumerable<BuilderType> attributes)
    {
        if (!methodInterceptionAttributes.Any())
            return;

        var types = builder
            .FindTypesByAttributes(methodInterceptionAttributes)
            .GroupBy(x => x.Type)
            .Select(x => new
            {
                x.Key,
                Item = x.ToArray()
            })
            .ToArray();

        foreach (var type in types)
        {
            builder.Log(LogTypes.Info, $"Implementing interceptors in type {type.Key.Fullname}");

            foreach (var method in type.Key.Methods)
            {
                if (method.IsConstructor || method.IsPropertyGetterSetter)
                    continue;

                for (int i = 0; i < type.Item.Length; i++)
                    method.CustomAttributes.Copy(type.Item[i].Attribute);
            }

            for (int i = 0; i < type.Item.Length; i++)
                type.Item[i].Remove();
        }
    }
}