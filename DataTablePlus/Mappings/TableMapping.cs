using DataTablePlus.Common;
using System.Collections.Generic;

namespace DataTablePlus.Mappings
{
	public class TableMapping
	{
		public string Schema { get; set; } = Constants.DefaultSchema;
		public string TableName { get; set; }
		public IList<string> PrimaryKeyNames { get; set; } = new List<string>();
		public IList<ColumnMapping> ColumnMappings { get; set; } = new List<ColumnMapping>();
	}
}
