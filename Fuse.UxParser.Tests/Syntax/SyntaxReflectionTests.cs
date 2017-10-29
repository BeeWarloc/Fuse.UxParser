using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using Fuse.UxParser.Syntax;
using NUnit.Framework;

namespace Fuse.UxParser.Tests.Syntax
{
	[TestFixture]
	public class SyntaxReflectionTests
	{
		static bool IsValidNodeChild(PropertyInfo prop)
		{
			return typeof(SyntaxBase).IsAssignableFrom(prop.PropertyType) ||
				typeof(SyntaxToken).IsAssignableFrom(prop.PropertyType) ||
				typeof(IEnumerable<SyntaxBase>).IsAssignableFrom(prop.PropertyType) ||
				typeof(IEnumerable<SyntaxToken>).IsAssignableFrom(prop.PropertyType);
		}

		static string FirstToUpper(string str)
		{
			return str.Substring(0, 1).ToUpperInvariant() + str.Substring(1);
		}

		[Test]
		public void All_public_properties_have_ChildOrder_syntax_matching_constructor_arguments()
		{
			var types =
				typeof(SyntaxBase)
					.Assembly
					.ExportedTypes
					.Where(type => typeof(SyntaxBase).IsAssignableFrom(type) && !type.IsAbstract);

			Assert.Multiple(
				() =>
				{
					foreach (var type in types)
					{
						var ctor = type.GetConstructors().OrderByDescending(x => x.GetParameters().Length).Single();

						var paramIndex = 0;
						foreach (var param in ctor.GetParameters())
						{
							var prop = type.GetProperty(FirstToUpper(param.Name));
							if (prop == null)
							{
								Assert.Fail("No property matching {0} ctor param {1}", type.Name, param.Name);
								continue;
							}
							var nodeChildAttribute = prop.GetCustomAttribute<NodeChildAttribute>();
							if (nodeChildAttribute == null)
							{
								Assert.Fail("Property {0}.{1} missing attribute", type.Name, prop.Name);
								continue;
							}
							Assert.That(
								IsValidNodeChild(prop),
								"Property {0}.{1} is marked with [NodeChild] but is neither a {2} nor a {3}",
								type.Name,
								prop.Name,
								typeof(SyntaxBase),
								typeof(SyntaxToken));

							Assert.That(
								nodeChildAttribute.OrderIndex,
								Is.EqualTo(paramIndex),
								"Index {0} defined in attribute on {1}.{2} don't match parameter {3} index {4}",
								nodeChildAttribute.OrderIndex,
								type.Name,
								prop.Name,
								param.Name,
								paramIndex);
							paramIndex++;
						}
					}
				});
		}

		[Explicit("This is not really a test, its actually for generating the visit methods for syntax trees")]
		[Test]
		public void Generate_visitor()
		{
			var sb = new StringBuilder();
			var types =
				typeof(SyntaxBase)
					.Assembly
					.ExportedTypes
					.Where(
						type => (typeof(SyntaxBase).IsAssignableFrom(type) || typeof(SyntaxToken).IsAssignableFrom(type)) &&
							!type.IsAbstract);

			sb.AppendLine("public class SyntaxVisitor\r\n{\r\n");

			var visitSwitch = string.Empty;

			foreach (var type in types)
			{
				string expectedSuffix;
				Type baseType;
				if (typeof(SyntaxToken).IsAssignableFrom(type))
				{
					baseType = typeof(SyntaxToken);
					expectedSuffix = "Token";
				}
				else if (typeof(SyntaxBase).IsAssignableFrom(type))
				{
					baseType = typeof(SyntaxBase);
					expectedSuffix = "Syntax";
				}
				else
				{
					continue;
				}

				var paramName = expectedSuffix.ToLower();
				Assert.That(type.Name.EndsWith(expectedSuffix), "{0} don't end with {1}", type.Name, expectedSuffix);
				var nameWithoutSuffix = type.Name.Substring(0, type.Name.Length - expectedSuffix.Length);
				sb.Append($"    protected virtual {baseType.Name} Visit{nameWithoutSuffix}({type.Name} {paramName})\r\n    {{\r\n");
				var invoke = "            return new " + type.Name + "(";
				var ctor = type.GetConstructors().OrderByDescending(x => x.GetParameters().Length).Single();

				var isFirstParam = true;
				var hasVisitedChild = false;
				var equalCheck = string.Empty;

				foreach (var prop in type.GetProperties().Where(x => x.GetCustomAttribute<NodeChildAttribute>() != null)
					.OrderBy(x => x.GetCustomAttribute<NodeChildAttribute>().OrderIndex))
				{
					var argName = ctor.GetParameters()
						.Single(x => string.Equals(prop.Name, x.Name, StringComparison.OrdinalIgnoreCase)).Name;

					if (!isFirstParam)
						invoke += ", ";
					isFirstParam = false;

					var propType = prop.PropertyType;
					if (typeof(SyntaxBase).IsAssignableFrom(propType) ||
						typeof(SyntaxToken).IsAssignableFrom(propType) ||
						propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(IImmutableList<>))
					{
						sb.AppendFormat("        var {0} = VisitAndConvert({1}.{2});\r\n", argName, paramName, prop.Name);
						invoke += argName;
						if (hasVisitedChild)
							equalCheck += " &&\r\n            ";
						equalCheck += $"Equals({paramName}.{prop.Name}, {argName})";
						hasVisitedChild = true;
					}
					else
					{
						invoke += $"{paramName}.{prop.Name}";
					}
				}
				invoke += ");";
				if (hasVisitedChild)
					sb.Append(
						$"        if ({equalCheck})\r\n        {{\r\n            return {paramName};\r\n        }}\r\n        else\r\n        {{\r\n{invoke}\r\n        }}\r\n");
				else
					sb.AppendLine($"        return {paramName};");
				sb.AppendLine("    }");

				visitSwitch += string.Format(
					"        case {0} s:\r\n            return Visit{1}(s);\r\n",
					type.Name,
					nameWithoutSuffix);
			}

			sb.AppendFormat(
				"    public virtual ISyntax Visit(ISyntax syntax)\r\n    {{\r\n        switch(syntax)\r\n        {{\r\n{0}            default:\r\n\tthrow new InvalidOperationException(\"syntax not recognized\");\r\n        }}\r\n    }}\r\n",
				visitSwitch);

			sb.AppendLine("}");

			Console.WriteLine(sb.ToString());
		}
	}
}