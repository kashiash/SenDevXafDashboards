﻿using CSScriptLibrary;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace SenDev.Xaf.Dashboards.Scripting
{
	public class ScriptDataSource
	{
		public ScriptDataSource(string script)
		{
			Script = script;
		}

		public XafApplication Application
		{
			get; set;
		}

		public string Script
		{
			get;
		}

		public object GetData(IDictionary<string, object> parameters)
		{
			var data = GetDataCore(parameters, out var objectSpace);
			return new ScriptResultList(data, objectSpace.TypesInfo);

		}

		private IEnumerable<Type> GetTypesHierarchy(Type type)
		{
			for (var currentType = type; currentType != null; currentType = currentType.BaseType)
			{
				yield return currentType;
			}

			foreach (var itf in type.GetInterfaces().SelectMany(GetTypesHierarchy))
			{
				yield return itf;
			}
		}

		private string[] GetReferencedAssemblies()
		{
			var assembly = GetType().Assembly;
			var assemmblyNames = 
				Application.TypesInfo.PersistentTypes.SelectMany(ti => GetTypesHierarchy(ti.Type)).Select(t => t.Assembly.GetName())
				.Where(a => a.Name != "mscorlib")
				.Select(a => a.Name);

			var assemblies = new HashSet<string>(assemmblyNames, StringComparer.OrdinalIgnoreCase);

			var module = Application.Modules.FindModule<SenDevDashboardsModule>();
			if (module != null)
			{
				if (module.ScriptReferenceAssemblies != null)
				{
					foreach (var ra in module.ScriptReferenceAssemblies)
						assemblies.Add(ra);
				}

				if (module.DefaultBusinessObjectType != null)
				{
					assemblies.Add(module.DefaultBusinessObjectType.Assembly.GetName().Name);
				}
			}

			return assemblies.ToArray();
		}

		private object GetDataCore(IDictionary<string, object> parameters, out XPObjectSpace objectSpace)
		{
			objectSpace = (XPObjectSpace)Application.CreateObjectSpace();
			var context = new ScriptContext(objectSpace, parameters);
			dynamic scriptObject = CSScript.LoadCode(Script, GetReferencedAssemblies()).CreateObject("*");
			return scriptObject.GetData(context);
		}

		public object GetDataForDataExtract()
		{
			return new DashboardDataList(() =>
				{
					var queryable = (IQueryable)GetDataCore(new Dictionary<string, object>(), out var objectSpace);
					return (queryable, objectSpace);
				}, 1);
		}

	}


}

