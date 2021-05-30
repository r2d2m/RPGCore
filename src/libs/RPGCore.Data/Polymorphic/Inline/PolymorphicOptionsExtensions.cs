﻿using RPGCore.Data.Polymorphic.Naming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RPGCore.Data.Polymorphic.Inline
{
	public static class PolymorphicOptionsExtensions
	{
		public static PolymorphicOptions UseInline(this PolymorphicOptions options)
		{
			var assemblies = GetDependentAssemblies(AppDomain.CurrentDomain, typeof(SerializeTypeAttribute).Assembly).ToList();

			foreach (var assembly in assemblies)
			{
				var types = assembly.GetTypes();
				foreach (var type in types)
				{
					var serialiseTypeAttributes = type.GetCustomAttributes<SerializeTypeAttribute>(false);
					var serialiseThistypeAttributes = type.GetCustomAttributes<SerializeThisTypeAttribute>(false);

					if (serialiseTypeAttributes.Any())
					{
						options.UseKnownBaseType(type, baseTypeOptions =>
						{
							foreach (var attribute in serialiseTypeAttributes)
							{
								if (attribute.Type != null)
								{
									baseTypeOptions.UseSubType(attribute.Type, subTypeOptions =>
									{
										subTypeOptions.Descriminator = GetDescriminatorForType(options, type, attribute.TypeName, attribute.NamingConvention);
										AddAliases(options, subTypeOptions.Aliases, attribute.TypeAliases, type, attribute.AliasConventions);
									});
								}
								else
								{
									baseTypeOptions.ResolveSubTypesAutomatically(resolveOptions =>
									{
										var namingConvention = GetNamingConvention(attribute.NamingConvention);
										if (namingConvention == null)
										{
											namingConvention = options.DefaultNamingConvention;
										}
										resolveOptions.TypeNaming = namingConvention;

										if (attribute.AliasConventions == TypeName.None)
										{
											resolveOptions.TypeAliasing.AddRange(options.DefaultAliasConventions);
										}
										else
										{
											resolveOptions.TypeAliasing.AddRange(GetNamingConventions(attribute.AliasConventions));
										}
									});
								}
							}
						});
					}

					if (serialiseThistypeAttributes.Any())
					{
						options.UseKnownSubType(type, subTypeOptions =>
						{
							foreach (var attribute in serialiseThistypeAttributes)
							{
								if (attribute.ExplicitBaseType != null)
								{
									subTypeOptions.UseBaseType(attribute.ExplicitBaseType, baseTypeOptions =>
									{
										baseTypeOptions.Descriminator = GetDescriminatorForType(options, type, attribute.TypeName, attribute.NamingConvention);
										AddAliases(options, baseTypeOptions.Aliases, attribute.TypeAliases, type, attribute.AliasConventions);
									});
								}
								else
								{
									subTypeOptions.ResolveBaseTypesAutomatically();
								}
							}
						});
					}
				}
			}
			return options;
		}

		private static string GetDescriminatorForType(PolymorphicOptions options, Type type, string? attributeTypeName, TypeName attributeTypeNameConvention)
		{
			string? descriminator = attributeTypeName;
			if (descriminator == null)
			{
				var namingConvention = GetNamingConvention(attributeTypeNameConvention);
				if (namingConvention == null)
				{
					namingConvention = options.DefaultNamingConvention;
				}
				descriminator = namingConvention.GetNameForType(type);
			}
			return descriminator;
		}

		private static void AddAliases(PolymorphicOptions options, List<string> destination, string[]? explicitAliases, Type type, TypeName aliasNames)
		{
			if (explicitAliases == null)
			{
				var aliasConventions = aliasNames != TypeName.None
					? GetNamingConventions(aliasNames)
					: options.DefaultAliasConventions;

				if (aliasConventions != null)
				{
					foreach (var convention in aliasConventions)
					{
						destination.Add(convention.GetNameForType(type));
					}
				}
			}
			else
			{
				foreach (string alias in explicitAliases)
				{
					destination.Add(alias);
				}
			}
		}

		private static IEnumerable<Assembly> GetDependentAssemblies(AppDomain appDomain, Assembly sourceAssembly)
		{
			bool Predicate(Assembly assembly)
			{
				return IsDependentAssemblies(assembly, sourceAssembly);
			}

			return appDomain.GetAssemblies().Where(Predicate);
		}

		private static bool IsDependentAssemblies(Assembly otherAssembly, Assembly sourceAssembly)
		{
			return otherAssembly == sourceAssembly
				|| otherAssembly.GetReferencedAssemblies()
					.Select(assemblyName => assemblyName.FullName)
					.Contains(sourceAssembly.FullName);
		}

		private static IEnumerable<ITypeNamingConvention> GetNamingConventions(TypeName namingConvention)
		{
			if (namingConvention.HasFlag(TypeName.FullName))
			{
				yield return TypeFullnameNamingConvention.Instance;
			}
			if (namingConvention.HasFlag(TypeName.Name))
			{
				yield return TypeNameNamingConvention.Instance;
			}
			if (namingConvention.HasFlag(TypeName.AssemblyQualifiedName))
			{
				yield return TypeAssemblyQualifiedNameNamingConvention.Instance;
			}
			if (namingConvention.HasFlag(TypeName.GUID))
			{
				yield return TypeGuidNamingConvention.Instance;
			}
		}

		private static ITypeNamingConvention? GetNamingConvention(TypeName namingConvention)
		{
			foreach (var naming in GetNamingConventions(namingConvention))
			{
				return naming;
			}
			return null;
		}
	}
}
