#if ENABLE_MONO && (DEVELOPMENT_BUILD || UNITY_EDITOR)
using SingularityGroup.HotReload.DTO;
using System;

namespace SingularityGroup.HotReload
{
    internal class SymbolResolvingFailedException : Exception
    {
        public SymbolResolvingFailedException(SMethod m, Exception inner)
            : base($"Unable to resolve method {m.displayName} in assembly {m.assemblyName}", inner) { }

        public SymbolResolvingFailedException(SType t)
            : base($"Unable to resolve type with name: {t.typeName} in assembly {t.assemblyName}") { }
    }
}
#endif
