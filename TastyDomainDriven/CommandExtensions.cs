using System;
using System.Linq.Expressions;

namespace TastyDomainDriven
{
    public static class CommandExtensions
    {
        public static void GuardDatetime<TClass>(this TClass obj, Expression<Func<TClass, DateTime>> assert) where TClass : ICommand
        {
            if (assert.Compile()(obj) == DateTime.MinValue)
            {
                throw new ArgumentException(string.Format("Missing DateTime on {0}", GetMemberInfo(assert).Member.Name));
            }
        }

        public static void GuardTimestamp<TClass>(this TClass obj) where TClass : ICommand
        {
            if (obj.Timestamp == DateTime.MinValue)
            {
                throw new ArgumentException(string.Format("Missing Timestamp on {0}", obj));
            }
        }

        public static void GuardIdentity<TClass>(this TClass obj, Expression<Func<TClass, Guid>> assert, string msg = null) where TClass : ICommand
        {
            if (assert.Compile()(obj) == Guid.Empty)
            {
                throw new ArgumentException(string.Format("Missing Identity on {0}", GetMemberInfo(assert).Member.Name, msg ?? string.Empty));
            }
        }

        public static void GuardNotEmpty<TClass>(this TClass obj, Expression<Func<TClass, String>> assert, string msg = null) where TClass : ICommand
        {
            if (string.IsNullOrEmpty(assert.Compile()(obj)))
            {
                throw new ArgumentException(string.Format("Missing string on {0} {1}", GetMemberInfo(assert).Member.Name, msg ?? string.Empty));
            }
        }

        /// <summary>
        /// Guard agains null
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        /// <param name="obj"></param>
        /// <param name="assert"></param>
        /// <param name="msg"></param>
        public static void GuardNotNull<TClass>(this TClass obj, Expression<Func<TClass, object>> assert, string msg = null) where TClass : ICommand
        {
            if (assert.Compile()(obj) == null)
            {                
                throw new ArgumentException(string.Format("Null value on {0} {1}", GetMemberInfo(assert).Member.Name, msg ?? ""));
            }
        }

        /// <summary>
        /// Guard against agains default(T) and null
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="obj"></param>
        /// <param name="assert"></param>
        /// <param name="msg"></param>
        public static void GuardNotNullOrDefault<TClass, TValue>(this TClass obj, Expression<Func<TClass, TValue>> assert, string msg = null) where TClass : ICommand
        {
            TValue def = default(TValue);
            if (def.Equals(assert.Compile()(obj)))
            {
                throw new ArgumentException(string.Format("Null value on {0} {1}", GetMemberInfo(assert).Member.Name, msg ?? ""));
            }
        }

        private static MemberExpression GetMemberInfo(Expression method)
        {
            LambdaExpression lambda = method as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException("method");

            MemberExpression memberExpr = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null)
                throw new ArgumentException("method");

            return memberExpr;
        }
    }
}