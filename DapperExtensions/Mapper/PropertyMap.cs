using System;
using System.Reflection;

namespace DapperExtensions.Mapper
{
    /// <summary>
    /// Maps an entity property to its corresponding column in the database.
    /// </summary>
    public interface IPropertyMap
    {
        string Name { get; }
        string ColumnName { get; }
        bool Ignored { get; }
		bool SelectIgnored { get; }
		bool InsertIgnored { get; }
		bool UpdateIgnored { get; }
		bool IsReadOnly { get; }
        KeyType KeyType { get; }
        PropertyInfo PropertyInfo { get; }
    }

    /// <summary>
    /// Maps an entity property to its corresponding column in the database.
    /// </summary>
    public class PropertyMap : IPropertyMap
    {
		private SqlOperation _ignoreOperations = SqlOperation.None;

        public PropertyMap(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            ColumnName = PropertyInfo.Name;
        }

        /// <summary>
        /// Gets the name of the property by using the specified propertyInfo.
        /// </summary>
        public string Name
        {
            get { return PropertyInfo.Name; }
        }

        /// <summary>
        /// Gets the column name for the current property.
        /// </summary>
        public string ColumnName { get; private set; }

        /// <summary>
        /// Gets the key type for the current property.
        /// </summary>
        public KeyType KeyType { get; private set; }

		/// <summary>
		/// Gets the ignore status of the current property. If ignored, the current property will not be included in queries.
		/// </summary>
		public bool Ignored
		{
			get { return _ignoreOperations == SqlOperation.All; }
		}

		/// <summary>
		/// Gets the select ignore status of the current property. If ignored, the current property will not be included in SELECT queries.
		/// </summary>
		public bool SelectIgnored
		{
			get { return _ignoreOperations.HasFlag(SqlOperation.Select); }
		}

		/// <summary>
		/// Gets the insert ignore status of the current property. If ignored, the current property will not be included in INSERT queries.
		/// </summary>
		public bool InsertIgnored
		{
			get { return _ignoreOperations.HasFlag(SqlOperation.Insert); }
		}

		/// <summary>
		/// Gets the update ignore status of the current property. If ignored, the current property will not be included in UPDATE queries.
		/// </summary>
		public bool UpdateIgnored
		{
			get { return _ignoreOperations.HasFlag(SqlOperation.Update); }
		}

		/// <summary>
		/// Gets the read-only status of the current property. If read-only, the current property will not be included in INSERT and UPDATE queries.
		/// </summary>
		public bool IsReadOnly
		{
			get { return _ignoreOperations.HasFlag(SqlOperation.Insert | SqlOperation.Update); }
		}

        /// <summary>
        /// Gets the property info for the current property.
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Fluently sets the column name for the property.
        /// </summary>
        /// <param name="columnName">The column name as it exists in the database.</param>
        public PropertyMap Column(string columnName)
        {
            ColumnName = columnName;
            return this;
        }

        /// <summary>
        /// Fluently sets the key type of the property.
        /// </summary>
        /// <param name="columnName">The column name as it exists in the database.</param>
        public PropertyMap Key(KeyType keyType)
        {
            if (Ignored)
            {
                throw new ArgumentException(string.Format("'{0}' is ignored and cannot be made a key field. ", Name));
            }

            if (IsReadOnly)
            {
                throw new ArgumentException(string.Format("'{0}' is readonly and cannot be made a key field. ", Name));
            }

			if (_ignoreOperations != SqlOperation.None)
			{
				throw new ArgumentException(string.Format("'{0}' has ignored operations and cannot be made a key field. ", Name));
			}

            KeyType = keyType;
            return this;
        }

        /// <summary>
        /// Fluently sets the ignore status of the property.
        /// </summary>
        public PropertyMap Ignore(SqlOperation operations = SqlOperation.All)
        {
            if (KeyType != KeyType.NotAKey)
            {
                throw new ArgumentException(string.Format("'{0}' is a key field and cannot be ignored.", Name));
            }

            _ignoreOperations = operations;
            return this;
        }

        /// <summary>
        /// Fluently sets the read-only status of the property.
        /// </summary>
        public PropertyMap ReadOnly()
        {
            if (KeyType != KeyType.NotAKey)
            {
                throw new ArgumentException(string.Format("'{0}' is a key field and cannot be marked readonly.", Name));
            }

            _ignoreOperations = SqlOperation.Insert | SqlOperation.Update;
            return this;
        }
    }

    /// <summary>
    /// Used by ClassMapper to determine which entity property represents the key.
    /// </summary>
    public enum KeyType
    {
        /// <summary>
        /// The property is not a key and is not automatically managed.
        /// </summary>
        NotAKey,

        /// <summary>
        /// The property is an integery-based identity generated from the database.
        /// </summary>
        Identity,

        /// <summary>
        /// The property is a Guid identity which is automatically managed.
        /// </summary>
        Guid,

        /// <summary>
        /// The property is a key that is not automatically managed.
        /// </summary>
        Assigned,

        /// <summary>
        /// The property is a key that is generated by the database.
        /// </summary>
        Generated
    }

	/// <summary>
	/// Used by ClassMapper to determine whether an entity property should be included in a statement.
	/// </summary>
	[Flags]
	public enum SqlOperation
	{
		/// <summary>
		/// The property is included in all statements.
		/// </summary>
		None = 0,

		/// <summary>
		/// The property is ignored for select statements.
		/// </summary>
		Select = 1,

		/// <summary>
		/// The property is ignored for insert statements.
		/// </summary>
		Insert = 2,

		/// <summary>
		/// The property is ignored for update statements.
		/// </summary>
		Update = 4,

		/// <summary>
		/// The property is ignored for all statements.
		/// </summary>
		All = Select | Insert | Update
	}
}