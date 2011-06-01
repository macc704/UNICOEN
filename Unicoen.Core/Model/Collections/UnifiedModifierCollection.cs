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

using System.Collections.Generic;
using Unicoen.Core.Processor;

namespace Unicoen.Core.Model {
	/// <summary>
	///   修飾子の集合を表します。
	///   e.g. Javaにおける<code>public static void method(){...}</code>の<code>public static</code>
	/// </summary>
	public class UnifiedModifierCollection
			: UnifiedElementCollection<UnifiedModifier, UnifiedModifierCollection> {
		private UnifiedModifierCollection() {}

		private UnifiedModifierCollection(IEnumerable<UnifiedModifier> elements)
				: base(elements) {}

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

		public static UnifiedModifierCollection Create() {
			return new UnifiedModifierCollection();
		}

		public static UnifiedModifierCollection Create(
				params UnifiedModifier[] modifiers) {
			return new UnifiedModifierCollection(modifiers);
		}

		public static UnifiedModifierCollection Create(
				IEnumerable<UnifiedModifier> modifiers) {
			return new UnifiedModifierCollection(modifiers);
		}
			}
}