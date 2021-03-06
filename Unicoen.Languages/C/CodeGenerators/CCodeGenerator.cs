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
using System.ComponentModel.Composition;
using System.IO;
using Unicoen.CodeGenerators;
using Unicoen.Model;
using Unicoen.Processor;

namespace Unicoen.Languages.C.CodeGenerators {
    [Export(typeof(UnifiedCodeGenerator))]
    public class CCodeGenerator : UnifiedCodeGenerator {
        public override string Extension {
            get { return ".c"; }
        }

        public override void Generate(
                UnifiedElement codeObject, TextWriter writer, string indentSign) {
            codeObject.Accept(
                    new CCodeFactoryVisitor(writer, indentSign),
                    new VisitorArgument());
        }

        public override void Generate(
                UnifiedElement codeObject, TextWriter writer) {
            Generate(codeObject, writer, "\t");
        }
    }
}