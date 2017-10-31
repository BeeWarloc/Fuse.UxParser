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
		[Test]
		public void Public_properties_with_NodeChild_attribute_matches_constructor_params()
		{
			var types =
				typeof(SyntaxBase)
					.Assembly
					.ExportedTypes
					.Where(type => typeof(SyntaxBase).IsAssignableFrom(type) && !type.IsAbstract);


			// Check pattern consistency for all syntax types
			Assert.Multiple(
				() =>
				{
					foreach (var type in types)
					{
						var props = GetNodeChildProperties(type);

						foreach (var prop in props)
						{
							Assert.That(IsValidNodeChildProperty(prop), "[NodeChild] property is not of expected type");
						}

						foreach (var propGroup in props.GroupBy(x => x.GetCustomAttribute<NodeChildAttribute>().OrderIndex).Where(x => x.Count() > 1))
						{
							Assert.Fail($"Properties {string.Join(", ", propGroup.Select(x => x.Name))} of {type.Name} marked with [{nameof(NodeChildAttribute)}] have conflicting OrderIndex {propGroup.Key}");
						}

						var createMethod = GetSyntaxCreateMethod(type);

						if (createMethod == null)
						{
							Assert.Fail("No public static Create method found for " + type.FullName);
							continue;
						}

						foreach (var x in props.Zip(createMethod.GetParameters(), (prop, param) => new { prop, param }))
						{
							Assert.That(
								x.param.Name,
								Is.EqualTo(FirstToLower(x.prop.Name)),
								$"Parameter name {x.param.Name} of {type.Name}.Create does not match property name {x.prop.Name}");
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
				var invoke = $"            return {type.Name}.Create(";
				var createMethod = GetSyntaxCreateMethod(type);

				var isFirstParam = true;
				var hasVisitedChild = false;
				var equalCheck = string.Empty;

				foreach (var prop in type.GetProperties().Where(x => x.GetCustomAttribute<NodeChildAttribute>() != null)
					.OrderBy(x => x.GetCustomAttribute<NodeChildAttribute>().OrderIndex))
				{
					var argName = createMethod.GetParameters()
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

		static MethodInfo GetSyntaxCreateMethod(Type type)
		{
			return type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, GetNodeChildProperties(type).Select(x => x.PropertyType).ToArray(), null);
		}

		static List<PropertyInfo> GetNodeChildProperties(Type type)
		{
			return type.GetProperties()
				.Select(propInfo => new { info = propInfo, nodeChildAttr = propInfo.GetCustomAttribute<NodeChildAttribute>() })
				.Where(x => x.nodeChildAttr != null)
				.OrderBy(x => x.nodeChildAttr.OrderIndex)
				.Select(x => x.info)
				.ToList();
		}

		static bool IsValidNodeChildProperty(PropertyInfo prop)
		{
			return typeof(SyntaxBase).IsAssignableFrom(prop.PropertyType) ||
				typeof(SyntaxToken).IsAssignableFrom(prop.PropertyType) ||
				typeof(IEnumerable<SyntaxBase>).IsAssignableFrom(prop.PropertyType) ||
				typeof(IEnumerable<SyntaxToken>).IsAssignableFrom(prop.PropertyType);
		}

		static string FirstToLower(string str)
		{
			return str.Substring(0, 1).ToLowerInvariant() + str.Substring(1);
		}
	}
}