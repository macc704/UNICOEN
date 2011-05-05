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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unicoen.Core.Visitors;

namespace Unicoen.Core.Model {
	/// <summary>
	///   配列型やポインタ型などを表すために型に付加される修飾子を表します。
	///   e.g. <c>int* p;</c>の<c>*</c>部分
	///   e.g. <c>int[10] a;</c>の<c>[10]</c>部分
	///   e.g. <c>int[10, 10] a;</c>の<c>[10, 10]</c>部分
	/// </summary>
	public class UnifiedTypeSupplement : UnifiedElement {
		private UnifiedArgumentCollection _arguments;

		/// <summary>
		///   実引数の集合を表します
		///   e.g. Cにおける<c>int* a, b, c</c>の<c>a, b, c</c>
		/// </summary>
		public UnifiedArgumentCollection Arguments {
			get { return _arguments; }
			set { _arguments = SetParentOfChild(value, _arguments); }
		}

		/// <summary>
		///   UnifiedTypeSupplementの種類を表します
		/// </summary>
		public UnifiedTypeSupplementKind Kind { get; set; }

		private UnifiedTypeSupplement() {}

		public override void Accept(IUnifiedModelVisitor visitor) {
			visitor.Visit(this);
		}

		public override void Accept<TData>(
				IUnifiedModelVisitor<TData> visitor,
				TData state) {
			visitor.Visit(this, state);
		}

		public override TResult Accept<TData, TResult>(
				IUnifiedModelVisitor<TData, TResult> visitor, TData state) {
			return visitor.Visit(this, state);
		}

		public override IEnumerable<IUnifiedElement> GetElements() {
			yield return Arguments;
		}

		public override IEnumerable<ElementReference>
				GetElementReferences() {
			yield return ElementReference.Create
					(() => Arguments, v => Arguments = (UnifiedArgumentCollection)v);
		}

		public override IEnumerable<ElementReference>
				GetElementReferenecesOfPrivateFields() {
			yield return ElementReference.Create
					(() => _arguments, v => _arguments = (UnifiedArgumentCollection)v);
		}

		public static UnifiedTypeSupplement Create(
				UnifiedArgumentCollection arguments,
				UnifiedTypeSupplementKind kind) {
			// arguments.Countが2以上の場合はC#の長方形配列を指定してください。
			Contract.Requires(
					kind == UnifiedTypeSupplementKind.Array &&
					arguments.Count == 1);
			return new UnifiedTypeSupplement {
					Arguments = arguments,
					Kind = kind,
			};
		}

		/// <summary>
		///   実引数を取らない１次元配列を作成します。
		/// </summary>
		/// <returns></returns>
		public static UnifiedTypeSupplement CreateArray() {
			return CreateArray(UnifiedArgument.Create(null));
		}

		/// <summary>
		///   実引数を取る１次元配列を作成します。
		/// </summary>
		/// <returns></returns>
		public static UnifiedTypeSupplement CreateArray(UnifiedArgument argument) {
			return Create(argument.ToCollection(), UnifiedTypeSupplementKind.Array);
		}
	}
}