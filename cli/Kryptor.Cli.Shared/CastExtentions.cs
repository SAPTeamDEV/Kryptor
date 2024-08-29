using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace SAPTeam.Kryptor.Cli
{
    public static class CastExtensions
    {
        private const string ImplicitCastMethodName = "op_Implicit";

        private const string ExplicitCastMethodName = "op_Explicit";

#if NET8_0_OR_GREATER
        public static bool CanCast<T>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] this Type baseType)
#else
        public static bool CanCast<T>(this Type baseType)
#endif
        {
            if (!baseType.CanImplicitCast<T>())
            {
                return baseType.CanExplicitCast<T>();
            }

            return true;
        }

        public static bool CanCast<T>(this object obj)
        {
            return obj.GetType().CanCast<T>();
        }

        public static T Cast<T>(this object obj)
        {
            try
            {
                return (T)obj;
            }
            catch (InvalidCastException)
            {
                if (obj.CanImplicitCast<T>())
                {
                    return obj.ImplicitCast<T>();
                }

                if (obj.CanExplicitCast<T>())
                {
                    return obj.ExplicitCast<T>();
                }

                throw;
            }
        }

#if NET8_0_OR_GREATER
        private static bool CanImplicitCast<T>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] this Type baseType)
#else
        private static bool CanImplicitCast<T>(this Type baseType)
#endif
        {
            return baseType.CanCast<T>("op_Implicit");
        }

        private static bool CanImplicitCast<T>(this object obj)
        {
            return obj.GetType().CanImplicitCast<T>();
        }

#if NET8_0_OR_GREATER
        private static bool CanExplicitCast<T>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] this Type baseType)
#else
        private static bool CanExplicitCast<T>(this Type baseType)
#endif
        {
            return baseType.CanCast<T>("op_Explicit");
        }

        private static bool CanExplicitCast<T>(this object obj)
        {
            return obj.GetType().CanExplicitCast<T>();
        }

#if NET8_0_OR_GREATER
        private static bool CanCast<T>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]this Type baseType, string castMethodName)
#else
        private static bool CanCast<T>(this Type baseType, string castMethodName)
#endif
        {
            Type targetType = typeof(T);
            return (from mi in baseType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    where mi.Name == castMethodName && mi.ReturnType == targetType
                    select mi).Any(delegate (MethodInfo mi)
                    {
                        ParameterInfo parameterInfo = mi.GetParameters().FirstOrDefault();
                        return parameterInfo != null && parameterInfo.ParameterType == baseType;
                    });
        }

        private static T ImplicitCast<T>(this object obj)
        {
            return obj.Cast<T>("op_Implicit");
        }

        private static T ExplicitCast<T>(this object obj)
        {
            return obj.Cast<T>("op_Explicit");
        }

        private static T Cast<T>(this object obj, string castMethodName)
        {
            Type objType = obj.GetType();
            MethodInfo methodInfo = (from mi in objType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                     where mi.Name == castMethodName && mi.ReturnType == typeof(T)
                                     select mi).SingleOrDefault(delegate (MethodInfo mi)
                                     {
                                         ParameterInfo parameterInfo = mi.GetParameters().FirstOrDefault();
                                         return parameterInfo != null && parameterInfo.ParameterType == objType;
                                     });
            if (methodInfo != null)
            {
                return (T)methodInfo.Invoke(null, new object[1] { obj });
            }

            throw new InvalidCastException("No method to cast " + objType.FullName + " to " + typeof(T).FullName);
        }
    }
}