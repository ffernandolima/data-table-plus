using DataTablePlus.Common;
using DataTablePlus.Extensions;
using System;

namespace DataTablePlus.Mappings
{
	public class ColumnMapping
	{
		private string _name;
		private Type _type;
		private int _order;
		private object _value;
		private object _defaultValue;

		public ColumnMapping() => this.EnsureDefaultValue();

		public string Name
		{
			get => this._name;
			set
			{
				this.ValidateName(value);
				this._name = value;
			}
		}

		public Type Type
		{
			get => this._type;
			set
			{
				this.ValidateType(value);
				this._type = value;
			}
		}

		public int Order
		{
			get => this._order;
			set
			{
				this.ValidateOrder(value);
				this._order = value;
			}
		}

		public bool IsPrimaryKey { get; set; }
		public bool AllowNull { get; set; }

		public object Value
		{
			get => this._value;
			set
			{
				this.ValidateDataType(value);
				this._value = value;
			}
		}

		public object DefaultValue
		{
			get => this._defaultValue;
			set
			{
				this.ValidateDataType(value);
				this._defaultValue = value;
			}
		}

		public void Validate()
		{
			this.ValidateName();
			this.ValidateType();
			this.ValidateOrder();

			this.EnsureDefaultValue();
			this.ValidateAllowNull();

			this.ValidateDataType();
		}

		private void EnsureDefaultValue() => this.DefaultValue = this.DefaultValue ?? this.Type?.GetDefaultValue();

		private void ValidateName() => this.ValidateName(this.Name);

		private void ValidateName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException($"{nameof(name)} {CommonResources.CannotBeNullOrWhiteSpace}", nameof(name));
			}
		}

		private void ValidateType() => this.ValidateType(this.Type);

		private void ValidateType(Type type)
		{
			if (type == null)
			{
				throw new ArgumentException($"{nameof(type)} {CommonResources.CannotBeNull}", nameof(type));
			}
		}

		private void ValidateOrder() => this.ValidateOrder(this.Order);

		private void ValidateOrder(int order)
		{
			if (order < 0)
			{
				throw new ArgumentException($"{nameof(order)} {CommonResources.CannotBeLessThanZero}", nameof(order));
			}
		}

		private void ValidateAllowNull()
		{
			if (!this.AllowNull && (this.Value == null && this.DefaultValue == null))
			{
				throw new ArgumentException(CommonResources.NullValueIsNotAllowed);
			}
		}

		private void ValidateDataType()
		{
			this.ValidateDataType(this.Value);
			this.ValidateDataType(this.DefaultValue);
		}

		private void ValidateDataType(object value)
		{
			if (value != null && this.Type != null && value.GetType() != this.Type)
			{
				throw new ArgumentException(CommonResources.DataTypesDoNotMatch);
			}
		}
	}
}

