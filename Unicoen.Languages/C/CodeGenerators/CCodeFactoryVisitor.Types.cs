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

using Unicoen.Model;
using Unicoen.Processor;

namespace Unicoen.Languages.C.CodeGenerators {
    // CCodeFactoryVisitorのうち、型に関する処理を行います
    public partial class CCodeFactoryVisitor {
        // 型(UnifiedBasicType)
        public override bool Visit(
                UnifiedBasicType element, VisitorArgument arg) {
            element.BasicTypeName.TryAccept(this, arg);
            return false;
        }
    }
}