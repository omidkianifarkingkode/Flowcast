using System;
using UnityEngine;

namespace FlowPipeline
{
    [Serializable]
    public class TypeReference
    {
        [SerializeField] private string _typeName;
        private Type _cachedType;

        public Type Type
        {
            get
            {
                if (_cachedType == null && !string.IsNullOrEmpty(_typeName))
                    _cachedType = Type.GetType(_typeName);
                return _cachedType;
            }
        }

        public void Set(Type type)
        {
            _typeName = type?.AssemblyQualifiedName;
            _cachedType = type;
        }

        public override string ToString() => Type?.Name ?? "<None>";
    }

}
