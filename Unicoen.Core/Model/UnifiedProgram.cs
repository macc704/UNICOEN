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
	///   ブログラム全体を表します。
	/// </summary>
	public class UnifiedProgram
			: UnifiedElementCollection<IUnifiedExpression, UnifiedProgram> {
		public override UnifiedProgram CreateSelf() {
			return new UnifiedProgram();
		}

		private UnifiedComment _comments;

		/// <summary>
		///   ソースコードの先頭に表記されたマジックコメントを表します．
		///   e.g. Pythonにおける<c># -*- coding: utf-8 -*-</c>
		/// </summary>
		public UnifiedComment Comments {
			get { return _comments; }
			set { _comments = SetChild(value, _comments); }
		}

		protected  UnifiedProgram() {}

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
			}
}