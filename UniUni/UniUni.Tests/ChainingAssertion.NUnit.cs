﻿#region License

// Copyright (C) 2011 The Unicoen Project
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;

namespace UniUni.Tests {

	#region Extensions

	public static class AssertEx {
		/// <summary>
		///   Assert.AreEqual, if T is IEnumerable then CollectionAssert.AreEqual
		/// </summary>
		public static void Is<T>(this T actual, T expected, string message = "") {
			if (typeof(T) != typeof(string)
			    && typeof(IEnumerable).IsAssignableFrom(typeof(T))) {
				((IEnumerable)actual).Cast<object>().Is(
						((IEnumerable)expected).Cast<object>(), message);
				return;
			}

			Assert.AreEqual(expected, actual, message);
		}

		/// <summary>
		///   Assert.IsTrue(predicate(value))
		/// </summary>
		public static void Is<T>(
				this T value, Expression<Func<T, bool>> predicate, string message = "") {
			var paramName = predicate.Parameters.First().Name;
			var msg = string.Format(
					"{0} = {1}, {2}{3}",
					paramName, value, predicate,
					string.IsNullOrEmpty(message) ? "" : ", " + message);

			Assert.IsTrue(predicate.Compile().Invoke(value), msg);
		}

		/// <summary>
		///   CollectionAssert.AreEqual
		/// </summary>
		public static void Is<T>(this IEnumerable<T> actual, params T[] expected) {
			Is(actual, expected.AsEnumerable());
		}

		/// <summary>
		///   CollectionAssert.AreEqual
		/// </summary>
		public static void Is<T>(
				this IEnumerable<T> actual, IEnumerable<T> expected, string message = "") {
			CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), message);
		}

		/// <summary>
		///   CollectionAssert.AreEqual
		/// </summary>
		public static void Is<T>(
				this IEnumerable<T> actual, IEnumerable<T> expected,
				IEqualityComparer<T> comparer, string message = "") {
			Is(actual, expected, comparer.Equals, message);
		}

		/// <summary>
		///   CollectionAssert.AreEqual
		/// </summary>
		public static void Is<T>(
				this IEnumerable<T> actual, IEnumerable<T> expected,
				Func<T, T, bool> equalityComparison, string message = "") {
			CollectionAssert.AreEqual(
					expected.ToArray(), actual.ToArray(),
					new ComparisonComparer<T>(equalityComparison), message);
		}

		/// <summary>
		///   Assert.AreNotEqual, if T is IEnumerable then CollectionAssert.AreNotEqual
		/// </summary>
		public static void IsNot<T>(this T actual, T notExpected, string message = "") {
			if (typeof(T) != typeof(string)
			    && typeof(IEnumerable).IsAssignableFrom(typeof(T))) {
				((IEnumerable)actual).Cast<object>().IsNot(
						((IEnumerable)notExpected).Cast<object>(), message);
				return;
			}

			Assert.AreNotEqual(notExpected, actual, message);
		}

		/// <summary>
		///   CollectionAssert.AreNotEqual
		/// </summary>
		public static void IsNot<T>(
				this IEnumerable<T> actual, params T[] notExpected) {
			IsNot(actual, notExpected.AsEnumerable());
		}

		/// <summary>
		///   CollectionAssert.AreNotEqual
		/// </summary>
		public static void IsNot<T>(
				this IEnumerable<T> actual, IEnumerable<T> notExpected, string message = "") {
			CollectionAssert.AreNotEqual(
					notExpected.ToArray(), actual.ToArray(), message);
		}

		/// <summary>
		///   CollectionAssert.AreNotEqual
		/// </summary>
		public static void IsNot<T>(
				this IEnumerable<T> actual, IEnumerable<T> notExpected,
				IEqualityComparer<T> comparer, string message = "") {
			IsNot(actual, notExpected, comparer.Equals, message);
		}

		/// <summary>
		///   CollectionAssert.AreNotEqual
		/// </summary>
		public static void IsNot<T>(
				this IEnumerable<T> actual, IEnumerable<T> notExpected,
				Func<T, T, bool> equalityComparison, string message = "") {
			CollectionAssert.AreNotEqual(
					notExpected.ToArray(), actual.ToArray(),
					new ComparisonComparer<T>(equalityComparison), message);
		}

		/// <summary>
		///   Assert.IsNull
		/// </summary>
		public static void IsNull<T>(this T value) {
			Assert.IsNull(value);
		}

		/// <summary>
		///   Assert.IsNotNull
		/// </summary>
		public static void IsNotNull<T>(this T value) {
			Assert.IsNotNull(value);
		}

		/// <summary>
		///   Assert.AreSame
		/// </summary>
		public static void IsSameReferenceAs<T>(
				this T actual, T expected, string message = "") {
			Assert.AreSame(expected, actual, message);
		}

		/// <summary>
		///   Assert.AreNotSame
		/// </summary>
		public static void IsNotSameReferenceAs<T>(
				this T actual, T notExpected, string message = "") {
			Assert.AreNotSame(notExpected, actual, message);
		}

		/// <summary>
		///   Assert.IsInstanceOf
		/// </summary>
		public static void IsInstanceOf<TExpected>(
				this object value, string message = "") {
			Assert.IsInstanceOf<TExpected>(value, message);
		}

		/// <summary>
		///   Assert.IsNotInstanceOf
		/// </summary>
		public static void IsNotInstanceOf<TWrong>(
				this object value, string message = "") {
			Assert.IsNotInstanceOf<TWrong>(value, message);
		}

		/// <summary>
		///   EqualityComparison to IComparer Converter for CollectionAssert
		/// </summary>
		private class ComparisonComparer<T> : IComparer {
			private readonly Func<T, T, bool> comparison;

			public ComparisonComparer(Func<T, T, bool> comparison) {
				this.comparison = comparison;
			}

			public int Compare(object x, object y) {
				return (comparison != null)
				       		? comparison((T)x, (T)y) ? 0 : -1
				       		: Equals(x, y) ? 0 : -1;
			}
		}

		#region DynamicAccessor

		/// <summary>
		///   to DynamicAccessor that can call private method/field/property/indexer.
		/// </summary>
		public static dynamic AsDynamic<T>(this T target) {
			return new DynamicAccessor<T>(target);
		}

		private class DynamicAccessor<T> : DynamicObject {
			private readonly T target;

			private static readonly BindingFlags TransparentFlags = BindingFlags.Instance
			                                                        | BindingFlags.Public
			                                                        |
			                                                        BindingFlags.
			                                                        		NonPublic;

			public DynamicAccessor(T target) {
				this.target = target;
			}

			public override bool TrySetIndex(
					SetIndexBinder binder, object[] indexes, object value) {
				try {
					typeof(T).InvokeMember(
							"Item", TransparentFlags | BindingFlags.SetProperty, null, target,
							indexes.Concat(new[] { value }).ToArray());
					return true;
				} catch (MissingMethodException) {
					throw new ArgumentException(
							string.Format("indexer not found : Type <{0}>", typeof(T).Name));
				}
				;
			}

			public override bool TryGetIndex(
					GetIndexBinder binder, object[] indexes, out object result) {
				try {
					result = typeof(T).InvokeMember(
							"Item", TransparentFlags | BindingFlags.GetProperty, null, target,
							indexes);
					return true;
				} catch (MissingMethodException) {
					throw new ArgumentException(
							string.Format("indexer not found : Type <{0}>", typeof(T).Name));
				}
				;
			}

			public override bool TrySetMember(SetMemberBinder binder, object value) {
				var accessor = new ReflectAccessor(target, binder.Name);
				accessor.SetValue(value);
				return true;
			}

			public override bool TryGetMember(GetMemberBinder binder, out object result) {
				var accessor = new ReflectAccessor(target, binder.Name);
				result = accessor.GetValue();
				return true;
			}

			public override bool TryInvokeMember(
					InvokeMemberBinder binder, object[] args, out object result) {
				var csharpBinder =
						binder.GetType().GetInterface(
								"Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");
				if (csharpBinder == null)
					throw new ArgumentException("is not generic csharp code");

				var typeArgs =
						(csharpBinder.GetProperty("TypeArguments").GetValue(binder, null) as
						 IList<Type>).ToArray();
				var method = MatchMethod(binder.Name, args, typeArgs);
				result = method.Invoke(target, args);

				return true;
			}

			private Type AssignableBoundType(Type left, Type right) {
				return (left == null || right == null) ? null
				       		: left.IsAssignableFrom(right) ? left
				       		  		: right.IsAssignableFrom(left) ? right
				       		  		  		: null;
			}

			private MethodInfo MatchMethod(
					string methodName, object[] args, Type[] typeArgs) {
				// name match
				var nameMatched = typeof(T).GetMethods(TransparentFlags)
						.Where(mi => mi.Name == methodName)
						.ToArray();
				if (!nameMatched.Any())
					throw new ArgumentException(
							string.Format(
									"\"{0}\" not found : Type <{1}>", methodName, typeof(T).Name));

				// type inference
				var typedMethods = nameMatched
						.Select(
								mi => {
									var genericArguments = mi.GetGenericArguments();

									if (!typeArgs.Any() && !genericArguments.Any()) // non generic method
									{
										return new {
												MethodInfo = mi,
												TypeParameters = default(Dictionary<Type, Type>)
										};
									} else if (!typeArgs.Any()) {
										var parameterGenericTypes = mi.GetParameters()
												.Select(pi => pi.ParameterType)
												.Zip(args.Select(o => o.GetType()), Tuple.Create)
												.GroupBy(a => a.Item1, a => a.Item2)
												.Where(g => g.Key.IsGenericParameter)
												.Select(g => new { g.Key, Type = g.Aggregate(AssignableBoundType) })
												.Where(a => a.Type != null);

										var typeParams = genericArguments
												.GroupJoin(
														parameterGenericTypes, x => x, x => x.Key, (_, Args) => Args)
												.ToArray();
										if (!typeParams.All(xs => xs.Any())) return null; // types short

										return new {
												MethodInfo = mi,
												TypeParameters = typeParams
														.Select(xs => xs.First())
														.ToDictionary(a => a.Key, a => a.Type)
										};
									} else {
										if (genericArguments.Length != typeArgs.Length) return null;

										return new {
												MethodInfo = mi,
												TypeParameters = genericArguments
														.Zip(typeArgs, Tuple.Create)
														.ToDictionary(t => t.Item1, t => t.Item2)
										};
									}
								})
						.Where(a => a != null)
						.Where(
								a => a.MethodInfo
								     		.GetParameters()
								     		.Select(pi => pi.ParameterType)
								     		.SequenceEqual(
								     				args.Select(o => o.GetType()),
								     				new EqualsComparer<Type>(
								     						(x, y) =>
								     						(x.IsGenericParameter)
								     								? a.TypeParameters[x].IsAssignableFrom(y)
								     								: x.Equals(y)))
						)
						.ToArray();

				if (!typedMethods.Any())
					throw new ArgumentException(
							string.Format(
									"\"{0}\" not match arguments : Type <{1}>", methodName, typeof(T).Name));

				// nongeneric
				var nongeneric = typedMethods.Where(a => a.TypeParameters == null).ToArray();
				if (nongeneric.Length == 1) return nongeneric[0].MethodInfo;

				// generic--
				var lessGeneric = typedMethods
						.Where(
								a =>
								!a.MethodInfo.GetParameters().All(
										pi => pi.ParameterType.IsGenericParameter))
						.ToArray();

				// generic
				var generic = (typedMethods.Length == 1)
				              		? typedMethods[0]
				              		: (lessGeneric.Length == 1 ? lessGeneric[0] : null);

				if (generic != null)
					return
							generic.MethodInfo.MakeGenericMethod(
									generic.TypeParameters.Select(kvp => kvp.Value).ToArray());

				// ambiguous
				throw new ArgumentException(
						string.Format(
								"\"{0}\" ambiguous arguments : Type <{1}>", methodName, typeof(T).Name));
			}

			private class EqualsComparer<TX> : IEqualityComparer<TX> {
				private readonly Func<TX, TX, bool> equals;

				public EqualsComparer(Func<TX, TX, bool> equals) {
					this.equals = equals;
				}

				public bool Equals(TX x, TX y) {
					return equals(x, y);
				}

				public int GetHashCode(TX obj) {
					return 0;
				}
			}

			private class ReflectAccessor {
				public Func<object> GetValue { get; private set; }
				public Action<object> SetValue { get; private set; }

				public ReflectAccessor(T target, string name) {
					var field = typeof(T).GetField(name, TransparentFlags);
					if (field != null) {
						GetValue = () => field.GetValue(target);
						SetValue = value => field.SetValue(target, value);
						return;
					}

					var prop = typeof(T).GetProperty(name, TransparentFlags);
					if (prop != null) {
						GetValue = () => prop.GetValue(target, null);
						SetValue = value => prop.SetValue(target, value, null);
						return;
					}

					throw new ArgumentException(
							string.Format("\"{0}\" not found : Type <{1}>", name, typeof(T).Name));
				}
			}
		}

		#endregion
	}

	#endregion
}