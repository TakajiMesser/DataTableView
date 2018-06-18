using System;

namespace Sample.DataAccessLayer
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IdentifierAttribute : Attribute
    {
        public IdentifierAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ForeignKeyAttribute : Attribute
    {
        private Type _type;

        public Type Type => _type;

        public ForeignKeyAttribute(Type type) => _type = type;
    }
}