﻿using System;
using System.Linq.Expressions;
using Raven.Client.Linq;

namespace Raven.Client.NodaTime
{
	internal static class CustomQueryTranslators
	{
		public static LinqPathProvider.Result OffsetDateTimeToInstantTranslator(LinqPathProvider provider, Expression expression)
		{
			// Just send the parent path back since we are storing it as a DateTimeOffset

			var exp = (MethodCallExpression) expression;
			var parent = provider.GetPath(exp.Object);

			return new LinqPathProvider.Result
			       {
				       MemberType = exp.Method.ReturnType,
				       IsNestedPath = false,
				       Path = parent.Path
			       };
		}

		public static LinqPathProvider.Result OffsetDateTimeLocalDateTimeTranslator(LinqPathProvider provider, Expression expression)
		{
			// Change the OffsetDateTime.LocalDateTime property to the DateTimeOffset.DateTime property

			var exp = (MemberExpression)expression;
			var parent = provider.GetPath(exp.Expression);

			return new LinqPathProvider.Result
			       {
				       MemberType = typeof(DateTime),
				       IsNestedPath = false,
				       Path = parent.Path + ".DateTime"
			       };
		}

		public static LinqPathProvider.Result ZonedDateTimeTimeToInstantTranslator(LinqPathProvider provider, Expression expression)
		{
			// Just send the parent path back since we are storing it as a DateTimeOffset

			var exp = (MethodCallExpression)expression;
			var parent = provider.GetPath(exp.Object);

			return new LinqPathProvider.Result
			       {
				       MemberType = exp.Method.ReturnType,
				       IsNestedPath = false,
				       Path = parent.Path
			       };
		}
	}
}
