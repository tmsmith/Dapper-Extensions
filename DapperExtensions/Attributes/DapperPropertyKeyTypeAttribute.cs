using DapperExtensions.Mapper;

namespace DapperExtensions.Attributes
{
    public class DapperPropertyKeyTypeAttribute : DapperPropertyAttribute
    {
        private readonly KeyType keyType;

        public DapperPropertyKeyTypeAttribute(KeyType keyType)
        {
            this.keyType = keyType;
        }

        public KeyType KeyType
        {
            get { return keyType; }
        }
    }
}
