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
using System.Linq;
using System.Xml.Linq;
using Mocomoco.Xml.Linq;
using Paraiba.Linq;
using Unicoen.Core.Model;

namespace Unicoen.Languages.C.ModelFactories {
	public static partial class CModelFactoryHelper {
		public static UnifiedProgram CreateTranslationUnit(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "translation_unit");
			/*
			 * translation_unit: external_declaration+ ;
			 */
			UnifiedProgram program = UnifiedProgram.Create();
			IUnifiedElementCollection<IUnifiedExpression> expressions = program;
			foreach (XElement e in node.Elements("external_declaration")) {
				expressions.Add(CreateExternalDeclaration(e));
			}

			return program;
		}

		public static IUnifiedExpression CreateExternalDeclaration(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "external_declaration");
			/*
			 * external_declaration 
			 * options {k=1;}
			 * : ( declaration_specifiers? declarator declaration* '{' )=> function_definition
			 * | declaration
			 * ;
			 */
			XElement first = node.FirstElement();
			if (first.Name() == "function_definition") {
				return CreateFunctionDefinition(first);
			}
			if (first.Name() == "declaration") {
				return CreateDeclaration(first);
			}
			throw new InvalidOperationException();
		}

		public static UnifiedFunctionDefinition CreateFunctionDefinition(
				XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "function_definition");
			/* function_definition
			 * :	declaration_specifiers? declarator
			 * (	declaration+ compound_statement	// K&R style
			 *   |	compound_statement				// ANSI style
			 * )
			 */

			UnifiedModifierCollection modifiers = null;
			UnifiedType type = null;
			UnifiedTypeParameterCollection typeParameters = null;
			UnifiedIdentifier name = null;
			UnifiedParameterCollection parameters = null;
			UnifiedTypeCollection throws = null;
			UnifiedBlock body = null;

			XElement first = node.FirstElement();
			if (first.Name() == "declaration_specifiers") {
				CreateDeclarationSpecifiers(first, out modifiers, out type);
			}

			CreateDeclarator(
					node.Element("declarator"),
					out name, out parameters);

			if (!node.Elements("declaration").IsEmpty()) {
				throw new NotImplementedException(); //TODO: implement
			}

			body = CreateCompoundStatement(node.Element("compound_statement"));

			return UnifiedFunctionDefinition.CreateFunction(
					modifiers, type, typeParameters, name, parameters, throws, body);
		}

		public static UnifiedFunctionDefinition CreateDeclaration(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "declaration");
			/*
			declaration
			:	'typedef' declaration_specifiers? {$declaration::isTypedef=true;}
				init_declarator_list ';' // special case, looking for typedef	
			| declaration_specifiers init_declarator_list? ';'
			 */
			// declaration_specifiers init_declarator_list? ';' において init_declarator がない時だけ
			// struct, と union を UnifiedClassDefenition とする。その他は UnifiedType でラップする
			throw new NotImplementedException(); //TODO: implement
		}

		public static void CreateDeclarationSpecifiers(
				XElement node,
				out UnifiedModifierCollection modifiers,
				out UnifiedType type) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "declaration_specifiers");
			/*	declaration_specifiers
			 * :   ( storage_class_specifier
			 *       |   type_specifier
			 *       |   type_qualifier      )+
			 */
			UnifiedModifierCollection ms = UnifiedModifierCollection.Create();
			IList<UnifiedType> types = new List<UnifiedType>();
			foreach (XElement e in node.Elements()) {
				switch (e.Name()) {
				case "storage_class_specifier":
					ms.Add(CreateStorageClassSpecifier(e));
					break;
				case "type_specifier":
					types.Add(CreateTypeSpecifier(e));
					break;
				case "type_qualifier":
					ms.Add(CreateTypeQualifier(e));
					break;
				default:
					throw new InvalidOperationException();
				}
			}
			modifiers = ms.IsEmpty() ? null : ms;

			String s = "";
			String prefix = "";
			foreach (UnifiedType t in types) {
				s += prefix + t.Name;
				prefix = " ";
			}
			type =
					UnifiedType.Create(UnifiedIdentifier.CreateType(s));
		}

		public static IUnifiedElement CreateInitDeclaratorList(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "init_declarator_list");
			/*
			init_declarator_list
			: init_declarator (',' init_declarator)*
			*/

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateInitDeclarator(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "init_declarator");
			/*
			init_declarator
			: declarator ('=' initializer)?
			*/

			throw new NotImplementedException(); //TODO: implement
		}

		public static UnifiedModifier CreateStorageClassSpecifier(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "storage_class_specifier");
			/*
			 * storage_class_specifier
			 * : 'extern'
			 * | 'static'
			 * | 'auto'
			 * | 'register'
			 */
			return UnifiedModifier.Create(node.FirstElement().Value);
		}

		public static UnifiedType CreateTypeSpecifier(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "type_specifier");
			/*	type_specifier
			 * : 'void'
			 * | 'char'
			 * | 'short'
			 * | 'int'
			 * | 'long'
			 * | 'float'
			 * | 'double'
			 * | 'signed'
			 * | 'unsigned'
			 * | struct_or_union_specifier
			 * | enum_specifier
			 * | type_id
			 */
			XElement first = node.FirstElement();
			switch (first.Name()) {
			case "struct_or_union_specifier":
				return CreateStructOrUnionSpecifier(first);
			case "enum_specifier":
				return CreateEnumSpecifier(first);
			case "type_id":
				return CreateTypeId(first);
			default:
				var ui = UnifiedIdentifier.CreateType(first.Name());
				return UnifiedType.Create(ui);
			}
		}

		public static UnifiedType CreateTypeId(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "type_id");
			/*
			type_id
			: {isTypeName(input.LT(1).getText())}? IDENTIFIER
			*/

			throw new NotImplementedException(); //TODO: implement
		}

		public static UnifiedType CreateStructOrUnionSpecifier(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "struct_or_union_specifier");
			/*	struct_or_union_specifier
			 * : struct_or_union IDENTIFIER? '{' struct_declaration_list '}'
			 * | struct_or_union IDENTIFIER
			 */
			// 構造体の定義と宣言の両方をこのメソッドで作成
			// 常に UnifiedTyep を返すが、
			// 構造体定義をしている場合だけ関数の呼び出し元で UnifiedType の中身をとりだす

			UnifiedIdentifier name = null;
			var kind = node.FirstElement().Name() == "struct"
			                        		? UnifiedClassKind.Struct : UnifiedClassKind.Union;
			if (node.Element("IDENTIFIER") != null) {
				name = UnifiedIdentifier.CreateType(node.Element("IDENTIFIER").Value);
			}

			if (node.Elements().Count() == 2) {
				var modifiers =
						UnifiedModifierCollection.Create(
								UnifiedModifier.Create(node.FirstElement().Name()));
				return UnifiedType.Create(modifiers, name, null, null);
			}

			var body =
					CreateStructDeclarationList(node.Element("struct_declaration_list"));
			var structOrUnion = UnifiedClassDefinition.Create(kind, null, name, null, null, body);

			return UnifiedType.Create(null, structOrUnion, null, null);
		}

		public static IUnifiedElement CreateStructOrUnion(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "struct_or_union");
			/* struct_or_union
			 * : 'struct'
			 * | 'union'
			 */
			// CreateStructOrUnionSpecifier からしか呼び出されていないので使わない
			throw new InvalidOperationException("this method isn't supported.");
		}

		public static UnifiedBlock CreateStructDeclarationList(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "struct_declaration_list");
			/* struct_declaration_list
			 * : struct_declaration+
			 */
			return
					UnifiedBlock.Create(
							node.Elements("struct_declaration").Select(CreateStructDeclaration));
		}

		public static IUnifiedExpression CreateStructDeclaration(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "struct_declaration");
			/*
			 * struct_declaration
			 * : specifier_qualifier_list struct_declarator_list ';'
			 */
			UnifiedModifierCollection modifiers = null;
			UnifiedType type;

			CreateSpecifierQualifierList(
					node.Element("specifier_qualifier_list"),
					out modifiers, out type);

			return UnifiedVariableDefinition.Create(
				null,
					modifiers, type,
					CreateStructDeclaratorList(node.Element("struct_declarator_list")));
		}

		public static void CreateSpecifierQualifierList(
				XElement node,
				out UnifiedModifierCollection modifiers, out UnifiedType type) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "specifier_qualifier_list");
			/*
			 * specifier_qualifier_list
			 * : ( type_qualifier | type_specifier )+
			 */
			modifiers = UnifiedModifierCollection.Create();
			var types = UnifiedTypeCollection.Create();
			foreach (var e in node.Elements()) {
				switch (e.Name()) {
				case "type_qualifier":
					modifiers.Add(CreateTypeQualifier(e));
					break;
				case "type_specifier":
					types.Add(CreateTypeSpecifier(e));
					break;
				}
			}

			modifiers = modifiers.IsEmpty() ? null : modifiers;

			String s = "";
			String prefix = "";
			foreach (UnifiedType t in types) {
				s += prefix + t.Name;
				prefix = " ";
			}
			type = s.Equals("")
			       		? null : UnifiedType.Create(UnifiedIdentifier.CreateType(s));
		}

		public static UnifiedVariableDefinitionBodyCollection
				CreateStructDeclaratorList(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "struct_declarator_list");
			/*
			 * struct_declarator_list
			 * : struct_declarator (',' struct_declarator)*
			 * */

			var declarators = UnifiedVariableDefinitionBodyCollection.Create();
			foreach (var e in node.Elements("struct_declarator")) {
				declarators.Add(CreateStructDeclarator(e));
			}
			return declarators;
		}

		public static UnifiedVariableDefinitionBody CreateStructDeclarator(
				XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "struct_declarator");
			/*
			 * struct_declarator
			 * : declarator (':' constant_expression)?
			 * | ':' constant_expression
			 * */

			//UnifiedVariableDefinitionBody.Create();

			throw new NotImplementedException(); //TODO: implement
		}

		public static UnifiedType CreateEnumSpecifier(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "enum_specifier");
			/*
			enum_specifier
			: 'enum' '{' enumerator_list '}'
			| 'enum' IDENTIFIER '{' enumerator_list '}'
			| 'enum' IDENTIFIER
			*/
			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateEnumeratorList(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "enumerator_list");
			/*
			enumerator_list
			: enumerator (',' enumerator)*
			*/

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateEnumerator(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "enumerator");
			/*
			enumerator
			: IDENTIFIER ('=' constant_expression)?
			*/

			throw new NotImplementedException(); //TODO: implement
		}

		public static UnifiedModifier CreateTypeQualifier(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "type_qualifier");
			/* type_qualifier
			 * : 'const'
			 * | 'volatile'
			 */
			return UnifiedModifier.Create(node.FirstElement().Value);
		}

		public static void CreateDeclarator(
				XElement node,
				out UnifiedIdentifier name,
				out UnifiedParameterCollection parameters) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "declarator");
			/*	declarator
			 * : pointer? direct_declarator
			 * | pointer
			 */

			if (node.Element("direct_declarator") != null) {
				if (node.Element("pointer") != null) {
					throw new NotImplementedException(); //TODO: implement
				}

				CreateDirectDeclarator(node.Element("direct_declarator"),
				                 out name, out parameters);

			} else {
				throw new NotImplementedException(); //TODO: implement
			}
			throw new NotImplementedException(); //TODO: implement
		}

		public static void CreateDirectDeclarator(
				XElement node,
				out UnifiedIdentifier name,
				out UnifiedParameterCollection parameters) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "direct_declarator");
			/* direct_declarator
			 * :   (	IDENTIFIER  | '(' declarator ')' ) declarator_suffix*
			 */

			if (node.Element("IDENTIFIER") != null) {
				name = UnifiedIdentifier.CreateFunction(node.Element("IDENTIFIER").Value);
			} else if (node.Element("declarator") != null) {
				throw new NotImplementedException(); //TODO: implement
			} else {
				throw new InvalidOperationException();
			}

			if (node.Elements("declarator_suffix").Count() > 1) {
				throw new NotImplementedException(); //TODO: implement
			} else if (node.Elements("declarator_suffix").Count() == 1) {
				CreateDeclaratorSuffix(node.Element("declarator_suffix"),
				                       out parameters);
			}

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateDeclaratorSuffix(XElement node,
			out UnifiedParameterCollection parameters) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "declarator_suffix");
			/*
			declarator_suffix
			:   '[' constant_expression ']'
			|   '[' ']'
			|   '(' parameter_type_list ')'
			|   '(' identifier_list ')'
			|   '(' ')'
			;
			*/

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreatePointer(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "pointer");
			/*	pointer
			 * : '*' type_qualifier+ pointer?
			 * | '*' pointer
			 * | '*'
			 */

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateParameterTypeList(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "parameter_type_list");
			/*
			parameter_type_list
			: parameter_list (',' '...')?
			*/

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateParameterList(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "parameter_list");
			/*
			parameter_list
			: parameter_declaration (',' parameter_declaration)*
			*/

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateParameterDeclaration(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "parameter_declaration");
			/*
			parameter_declaration
			: declaration_specifiers (declarator|abstract_declarator)*
			 */

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateIdentifierList(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "identifier_list");
			/*
			identifier_list
			: IDENTIFIER (',' IDENTIFIER)*
			*/

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateTypeName(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "type_name");
			/*
			type_name
			: specifier_qualifier_list abstract_declarator?
			*/

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateAbstractDeclarator(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "abstract_declarator");
			/*
			abstract_declarator
			: pointer direct_abstract_declarator?
			| direct_abstract_declarator
			 */

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateDirectAbstractDeclarator(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "direct_abstract_declarator");
			/*
			direct_abstract_declarator
			:	( '(' abstract_declarator ')' | abstract_declarator_suffix ) abstract_declarator_suffix*
			 */

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateAbstractDeclaratorSuffix(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "abstract_declarator_suffix");
			/*
			abstract_declarator_suffix
			:	'[' ']'
			|	'[' constant_expression ']'
			|	'(' ')'
			|	'(' parameter_type_list ')'
			*/

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateInitializer(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "initializer");
			/*
			initializer
			: assignment_expression
			| '{' initializer_list ','? '}'
			 */

			throw new NotImplementedException(); //TODO: implement
		}

		public static IUnifiedElement CreateInitializerList(XElement node) {
			Contract.Requires(node != null);
			Contract.Requires(node.Name() == "initializer_list");
			/*
			initializer_list
			: initializer (',' initializer)*
			 */

			throw new NotImplementedException(); //TODO: implement
		}
	}
}