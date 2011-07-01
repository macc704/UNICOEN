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

using System.Diagnostics.Contracts;
using System.Linq;

namespace Unicoen.Core.Model {
	public abstract class UnifiedType : UnifiedElement, IUnifiedExpression {
		public abstract IUnifiedExpression NameExpression { get; set; }

		public static UnifiedType Create(string name) {
			// new[] の場合，NameExpressionがnullなUnifiedSimpleTypeを生成する．
			return new UnifiedSimpleType {
					NameExpression = name != null
					                 		? UnifiedVariableIdentifier.Create(name)
					                 		: null,
			};
		}

		public static UnifiedType Create(
				IUnifiedExpression nameExpression = null) {
			return new UnifiedSimpleType {
					NameExpression = nameExpression,
			};
		}

		public UnifiedType WrapArrayRepeatedly(int count) {
			Contract.Requires(count >= 0);
			var type = this;
			for (int i = 0; i < count; i++) {
				type = type.WrapArray();
			}
			return type;
		}

		public UnifiedType WrapArray(UnifiedArgument argument = null) {
			return new UnifiedArrayType {
					Type = this,
					Arguments = argument.ToCollection(),
			};
		}

		public UnifiedType WrapRectangleArray(int dimension) {
			Contract.Requires(dimension >= 1);
			return new UnifiedArrayType {
					Type = this,
					Arguments =
							Enumerable.Repeat<UnifiedArgument>(null, dimension).ToCollection(),
			};
		}

		public UnifiedType WrapRectangleArray(
				UnifiedArgumentCollection arguments = null) {
			return new UnifiedArrayType {
					Type = this,
					Arguments = arguments,
			};
		}

		public UnifiedType WrapGeneric(
				UnifiedGenericArgumentCollection arguments = null) {
			return new UnifiedGenericType {
					Type = this,
					Arguments = arguments,
			};
		}

		public UnifiedType WrapPointer() {
			return new UnifiedPointerType {
					Type = this,
			};
		}

		public UnifiedType WrapReference() {
			return new UnifiedReferenceType {
					Type = this,
			};
		}

		public UnifiedType WrapConst() {
			return new UnifiedConstType {
					Type = this,
			};
		}

		public UnifiedType WrapVolatile() {
			return new UnifiedVolatileType {
					Type = this,
			};
		}

		public UnifiedType WrapUnion() {
			return new UnifiedUnionType {
					Type = this,
			};
		}

		public UnifiedType WrapStruct() {
			return new UnifiedStructType {
					Type = this,
			};
		}
	}
}