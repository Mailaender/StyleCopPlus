using System;
using System.Collections.Generic;
using StyleCop;
using StyleCop.CSharp;

namespace StyleCopPlus.Plugin.AdvancedNaming
{
	/// <summary>
	/// Advanced naming rules.
	/// </summary>
	public class AdvancedNamingRules
	{
		private readonly StyleCopPlusRules m_parent;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public AdvancedNamingRules(StyleCopPlusRules parent)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			m_parent = parent;
		}

		#region Analyzing document

		/// <summary>
		/// Analyzes source document.
		/// </summary>
		public void AnalyzeDocument(CodeDocument document)
		{
			CurrentNamingSettings settings = new CurrentNamingSettings();
			settings.Initialize(m_parent, document);

			CsDocument doc = (CsDocument)document;
			AnalyzeElements(doc.RootElement.ChildElements, settings);
		}

		/// <summary>
		/// Analyzes a collection of elements.
		/// </summary>
		public void AnalyzeElements(IEnumerable<CsElement> elements, CurrentNamingSettings settings)
		{
			foreach (CsElement element in elements)
			{
				AnalyzeElement(element, settings);
				AnalyzeElements(element.ChildElements, settings);
			}
		}

		/// <summary>
		/// Analyzes specified element.
		/// </summary>
		public void AnalyzeElement(CsElement element, CurrentNamingSettings settings)
		{
			switch (element.ElementType)
			{
				case ElementType.Class:
					AnalyzeClass(element, settings);
					break;
				case ElementType.Constructor:
					AnalyzeConstructor(element, settings);
					break;
				case ElementType.Delegate:
					AnalyzeDelegate(element, settings);
					break;
				case ElementType.Destructor:
					AnalyzeDestructor(element, settings);
					break;
				case ElementType.Enum:
					AnalyzeEnum(element, settings);
					break;
				case ElementType.EnumItem:
					AnalyzeEnumItem(element, settings);
					break;
				case ElementType.Event:
					AnalyzeEvent(element, settings);
					break;
				case ElementType.Field:
					AnalyzeField(element, settings);
					break;
				case ElementType.Indexer:
					AnalyzeIndexer(element, settings);
					break;
				case ElementType.Interface:
					AnalyzeInterface(element, settings);
					break;
				case ElementType.Method:
					AnalyzeMethod(element, settings);
					break;
				case ElementType.Namespace:
					AnalyzeNamespace(element, settings);
					break;
				case ElementType.Property:
					AnalyzeProperty(element, settings);
					break;
				case ElementType.Struct:
					AnalyzeStruct(element, settings);
					break;
			}
		}

		/// <summary>
		/// Analyzes class element.
		/// </summary>
		private void AnalyzeClass(CsElement element, CurrentNamingSettings settings)
		{
			CheckDerivings(element, settings);

			if (CodeHelper.IsInternal(element))
			{
				CheckDeclaration(element, settings, NamingSettings.ClassInternal);
			}
			else
			{
				CheckDeclaration(element, settings, NamingSettings.ClassNotInternal);
			}

			CheckTypeParameters(element, settings);
		}

		/// <summary>
		/// Analyzes constructor element.
		/// </summary>
		private void AnalyzeConstructor(CsElement element, CurrentNamingSettings settings)
		{
			CheckParameters(element, settings);
			CheckLocalDeclarations(element, settings);
			CheckLabels(element, settings);
		}

		/// <summary>
		/// Analyzes delegate element.
		/// </summary>
		private void AnalyzeDelegate(CsElement element, CurrentNamingSettings settings)
		{
			CheckDeclaration(element, settings, NamingSettings.Delegate);
			CheckTypeParameters(element, settings);
			CheckParameters(element, settings);
		}

		/// <summary>
		/// Analyzes destructor element.
		/// </summary>
		private void AnalyzeDestructor(CsElement element, CurrentNamingSettings settings)
		{
			CheckParameters(element, settings);
			CheckLocalDeclarations(element, settings);
			CheckLabels(element, settings);
		}

		/// <summary>
		/// Analyzes enum element.
		/// </summary>
		private void AnalyzeEnum(CsElement element, CurrentNamingSettings settings)
		{
			CheckDeclaration(element, settings, NamingSettings.Enum);
		}

		/// <summary>
		/// Analyzes enum item element.
		/// </summary>
		private void AnalyzeEnumItem(CsElement element, CurrentNamingSettings settings)
		{
			CheckDeclaration(element, settings, NamingSettings.EnumItem);
		}

		/// <summary>
		/// Analyzes event element.
		/// </summary>
		private void AnalyzeEvent(CsElement element, CurrentNamingSettings settings)
		{
			CheckDeclaration(element, settings, NamingSettings.Event);
		}

		/// <summary>
		/// Analyzes field element.
		/// </summary>
		private void AnalyzeField(CsElement element, CurrentNamingSettings settings)
		{
			Field field = (Field)element;
			if (field.Const)
			{
				CheckDeclarationAccess(
					field,
					settings,
					NamingSettings.PublicConst,
					NamingSettings.ProtectedConst,
					NamingSettings.PrivateConst,
					NamingSettings.InternalConst);
			}
			else if (CodeHelper.IsStatic(field))
			{
				CheckDeclarationAccess(
					field,
					settings,
					NamingSettings.PublicStaticField,
					NamingSettings.ProtectedStaticField,
					NamingSettings.PrivateStaticField,
					NamingSettings.InternalStaticField);
			}
			else
			{
				CheckDeclarationAccess(
					field,
					settings,
					NamingSettings.PublicInstanceField,
					NamingSettings.ProtectedInstanceField,
					NamingSettings.PrivateInstanceField,
					NamingSettings.InternalInstanceField);
			}
		}

		/// <summary>
		/// Analyzes indexer element.
		/// </summary>
		private void AnalyzeIndexer(CsElement element, CurrentNamingSettings settings)
		{
			CheckParameters(element, settings);
			CheckLocalDeclarations(element, settings);
			CheckLabels(element, settings);
		}

		/// <summary>
		/// Analyzes interface element.
		/// </summary>
		private void AnalyzeInterface(CsElement element, CurrentNamingSettings settings)
		{
			CheckDeclaration(element, settings, NamingSettings.Interface);
		}

		/// <summary>
		/// Analyzes method element.
		/// </summary>
		private void AnalyzeMethod(CsElement element, CurrentNamingSettings settings)
		{
			if (!CodeHelper.IsOperator(element))
			{
				if (CodeHelper.IsPrivateEventHandler(element))
				{
					CheckDeclaration(element, settings, NamingSettings.MethodPrivateEventHandler);
				}
				else if (CodeHelper.IsProtectedEventHandler(element))
				{
					CheckDeclaration(element, settings, NamingSettings.MethodProtectedEventHandler);
				}
				else if (CodeHelper.IsTestMethod(element))
				{
					CheckDeclaration(element, settings, NamingSettings.MethodTest);
				}
				else
				{
					CheckDeclaration(element, settings, NamingSettings.MethodGeneral);
				}
			}

			CheckTypeParameters(element, settings);
			CheckParameters(element, settings);
			CheckLocalDeclarations(element, settings);
			CheckLabels(element, settings);
		}

		/// <summary>
		/// Analyzes namespace element.
		/// </summary>
		private void AnalyzeNamespace(CsElement element, CurrentNamingSettings settings)
		{
			CheckDeclaration(element, settings, NamingSettings.Namespace);
		}

		/// <summary>
		/// Analyzes property element.
		/// </summary>
		private void AnalyzeProperty(CsElement element, CurrentNamingSettings settings)
		{
			CheckDeclaration(element, settings, NamingSettings.Property);
			CheckParameters(element, settings);
			CheckLocalDeclarations(element, settings);
			CheckLabels(element, settings);
		}

		/// <summary>
		/// Analyzes struct element.
		/// </summary>
		private void AnalyzeStruct(CsElement element, CurrentNamingSettings settings)
		{
			if (CodeHelper.IsInternal(element))
			{
				CheckDeclaration(element, settings, NamingSettings.StructInternal);
			}
			else
			{
				CheckDeclaration(element, settings, NamingSettings.StructNotInternal);
			}
		}

		#endregion

		#region Checking entity names

		/// <summary>
		/// Checks declaration naming for common access modifiers.
		/// </summary>
		private void CheckDeclarationAccess(
			CsElement element,
			CurrentNamingSettings settings,
			string publicSettingName,
			string protectedSettingName,
			string privateSettingName,
			string internalSettingName)
		{
			if (CodeHelper.IsPublic(element))
			{
				CheckDeclaration(element, settings, publicSettingName);
			}
			else if (CodeHelper.IsProtected(element))
			{
				CheckDeclaration(element, settings, protectedSettingName);
			}
			else if (CodeHelper.IsPrivate(element))
			{
				CheckDeclaration(element, settings, privateSettingName);
			}
			else if (CodeHelper.IsInternal(element))
			{
				CheckDeclaration(element, settings, internalSettingName);
			}
		}

		/// <summary>
		/// Checks declaration naming.
		/// </summary>
		private void CheckDeclaration(CsElement element, CurrentNamingSettings settings, string settingName)
		{
			CheckName(
				element,
				null,
				settings,
				settingName,
				element.Declaration.Name,
				element.Declaration.Tokens);
		}

		/// <summary>
		/// Checks parameters naming.
		/// </summary>
		private void CheckParameters(CsElement element, CurrentNamingSettings settings)
		{
			foreach (ParameterItem parameter in CodeHelper.GetParameters(element))
			{
				CheckName(
					element,
					parameter.LineNumber,
					settings,
					NamingSettings.Parameter,
					parameter.Name,
					parameter.Tokens);
			}
		}

		/// <summary>
		/// Checks type parameters naming.
		/// </summary>
		private void CheckTypeParameters(CsElement element, CurrentNamingSettings settings)
		{
			foreach (TypeParameterItem parameter in CodeHelper.GetTypeParameters(element))
			{
				CheckName(
					element,
					parameter.LineNumber,
					settings,
					NamingSettings.TypeParameter,
					parameter.Name,
					parameter.Tokens);
			}
		}

		/// <summary>
		/// Checks local declarations naming.
		/// </summary>
		private void CheckLocalDeclarations(CsElement element, CurrentNamingSettings settings)
		{
			foreach (LocalDeclarationItem declaration in CodeHelper.GetLocalDeclarations(element))
			{
				if (declaration.IsConstant)
				{
					CheckName(
						element,
						declaration.LineNumber,
						settings,
						NamingSettings.LocalConstant,
						declaration.Name,
						declaration.Tokens);
				}
				else
				{
					CheckName(
						element,
						declaration.LineNumber,
						settings,
						NamingSettings.LocalVariable,
						declaration.Name,
						declaration.Tokens);
				}
			}
		}

		/// <summary>
		/// Checks labels naming.
		/// </summary>
		private void CheckLabels(CsElement element, CurrentNamingSettings settings)
		{
			foreach (LabelItem label in CodeHelper.GetLabels(element))
			{
				CheckName(
					element,
					label.LineNumber,
					settings,
					NamingSettings.Label,
					label.Name,
					label.Tokens);
			}
		}

		/// <summary>
		/// Checks specified name.
		/// </summary>
		private void CheckName(
			CsElement element,
			int? lineNumber,
			CurrentNamingSettings settings,
			string settingName,
			string fullName,
			IEnumerable<CsToken> fullTokens)
		{
			if (settings.IsEnabledBlockAt(settingName))
			{
				string fullNameWithAt = CodeHelper.GetNameWithAt(fullTokens, fullName);
				foreach (string nameToCheck in CodeHelper.ExtractNamesToCheck(fullNameWithAt, settingName))
				{
					string pureName = CodeHelper.ExtractPureName(nameToCheck, false);

					CheckBlockAt(element, lineNumber, settings, settingName, pureName);
				}
			}

			foreach (string nameToCheck in CodeHelper.ExtractNamesToCheck(fullName, settingName))
			{
				string pureName = CodeHelper.ExtractPureName(nameToCheck, true);

				CheckNamingRules(element, lineNumber, settings, settingName, pureName);
				CheckEnglishOnly(element, lineNumber, settings, settingName, pureName);
				CheckNameLength(element, lineNumber, settings, settingName, pureName);
			}
		}

		/// <summary>
		/// Checks whether specified name is correct.
		/// </summary>
		private void CheckNamingRules(
			CsElement element,
			int? lineNumber,
			CurrentNamingSettings settings,
			string settingName,
			string nameToCheck)
		{
			if (settings.CheckNamingRule(settingName, nameToCheck))
				return;

			AddViolation(
				element,
				lineNumber,
				settings,
				settingName,
				nameToCheck);
		}

		/// <summary>
		/// Checks whether name with @ character is correct.
		/// </summary>
		private void CheckBlockAt(
			CsElement element,
			int? lineNumber,
			CurrentNamingSettings settings,
			string settingName,
			string nameToCheck)
		{
			if (settings.CheckBlockAt(settingName, nameToCheck))
				return;

			string friendlyName = settings.GetFriendlyName(settingName);
			string example = EntitySettingResources.BlockAtExample;

			AddViolation(
				element,
				lineNumber,
				friendlyName,
				nameToCheck,
				example);
		}

		/// <summary>
		/// Checks whether name with non-English characters is correct.
		/// </summary>
		private void CheckEnglishOnly(
			CsElement element,
			int? lineNumber,
			CurrentNamingSettings settings,
			string settingName,
			string nameToCheck)
		{
			if (settings.CheckEnglishOnly(settingName, nameToCheck))
				return;

			string friendlyName = settings.GetFriendlyName(settingName);
			string example = EntitySettingResources.EnglishOnlyExample;

			AddViolation(
				element,
				lineNumber,
				friendlyName,
				nameToCheck,
				example);
		}

		/// <summary>
		/// Checks whether name follows length agreement.
		/// </summary>
		private void CheckNameLength(
			CsElement element,
			int? lineNumber,
			CurrentNamingSettings settings,
			string settingName,
			string nameToCheck)
		{
			if (settings.CheckNameLength(settingName, nameToCheck))
				return;

			string friendlyName = settings.GetFriendlyName(settingName);
			string example = EntitySettingResources.CheckLengthExample;

			AddViolation(
				element,
				lineNumber,
				friendlyName,
				nameToCheck,
				example);
		}

		/// <summary>
		/// Checks derivings condition.
		/// </summary>
		private void CheckDerivings(CsElement element, CurrentNamingSettings settings)
		{
			Class @class = (Class)element;
			if (String.IsNullOrEmpty(@class.BaseClass))
				return;

			string name = CodeHelper.ExtractPureName(@class.Declaration.Name, true);
			string baseName = CodeHelper.ExtractPureName(@class.BaseClass, true);

			string deriving;
			if (settings.CheckDerivedName(baseName, name, out deriving))
				return;

			string friendlyName = SpecialSettingResources.DerivingsFriendlyName;
			string example = String.Format(SpecialSettingResources.DerivingsExample, deriving);

			AddViolation(
				element,
				null,
				friendlyName,
				name,
				example);
		}

		#endregion

		#region Firing violations

		/// <summary>
		/// Fires violation.
		/// </summary>
		private void AddViolation(
			CsElement element,
			int? lineNumber,
			CurrentNamingSettings settings,
			string settingName,
			string currentName)
		{
			AddViolation(
				element,
				lineNumber,
				settings.GetFriendlyName(settingName),
				currentName,
				settings.GetExample(settingName));
		}

		/// <summary>
		/// Fires low-level violation.
		/// </summary>
		private void AddViolation(
			CsElement element,
			int? lineNumber,
			string friendlyName,
			string currentName,
			string example)
		{
			example = example.TrimExample();

			if (lineNumber.HasValue)
			{
				m_parent.AddViolation(
					element,
					lineNumber.Value,
					Rules.AdvancedNamingRules,
					friendlyName,
					currentName,
					example);
			}
			else
			{
				m_parent.AddViolation(
					element,
					Rules.AdvancedNamingRules,
					friendlyName,
					currentName,
					example);
			}
		}

		#endregion
	}
}
