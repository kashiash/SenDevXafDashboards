﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.DashboardCommon;
using DevExpress.DashboardCommon.Native;
using SenDev.Xaf.Dashboards.Scripting;
using Xunit;

namespace UnitTests
{
	public class DataReaderListTests
	{
		[Fact]
		public void BasicReaderTest()
		{
			DataTable table = new DataTable();
			table.Columns.Add("IntColumn", typeof(int));
			table.Columns.Add("StringColumn", typeof(string));
			table.Rows.Add(1, "Row 1");
			table.Rows.Add(2, "Row 2");
			table.Rows.Add(3, "Row 3");
			var reader = table.CreateDataReader();

			DataReaderList list = new DataReaderList(reader);
			var properties = list.GetItemProperties(null);
			Assert.Equal(2, properties.Count);
			Assert.Equal("IntColumn", properties[0].Name);
			Assert.Equal(typeof(int), properties[0].PropertyType);

			Assert.Equal(1, properties[0].GetValue(list[0]));
			Assert.Equal(2, properties[0].GetValue(list[1]));
			Assert.Equal(3, properties[0].GetValue(list[2]));

		}

		[Fact]
		public void ForEachTest()
		{
			DataTable table = new DataTable();
			table.Columns.Add("IntColumn", typeof(int));
			table.Columns.Add("StringColumn", typeof(string));
			table.Rows.Add(1, "Row 1");
			table.Rows.Add(2, "Row 2");
			table.Rows.Add(3, "Row 3");
			var reader = table.CreateDataReader();

			DataReaderList dynamicList = new DataReaderList(reader);
			var properties = dynamicList.GetItemProperties(null);
			List<object> list = new List<object>();
			foreach (var item in dynamicList)
			{
				list.Add(item);
			}
			Assert.Equal(3, list.Count);

		}

		[Fact]
		public void DataExtractCreationTest()
		{
			DataTable table = new DataTable();
			table.Columns.Add("IntColumn", typeof(int));
			table.Columns.Add("StringColumn", typeof(string));
			table.Rows.Add(1, "Row 1");
			table.Rows.Add(2, "Row 2");
			table.Rows.Add(3, "Row 3");
			var reader = table.CreateDataReader();

			using (DashboardObjectDataSource ods = new DashboardObjectDataSource())
			{
				ods.DataSource = new DataReaderList(reader);
				using (DashboardExtractDataSource extractDataSource = new DashboardExtractDataSource())
				{
					extractDataSource.ExtractSourceOptions.DataSource = ods;
					extractDataSource.FileName = Path.GetTempFileName();
					extractDataSource.UpdateExtractFile();
					IDashboardDataSourceInternal dsi = extractDataSource;
					var storage = dsi.GetStorage(null);
					Assert.Equal(3, storage.RowCount);
				}
			}
		}
	}
}
