using System;
using System.Collections.Generic;

namespace Bugger.Entities
{
    public interface IGlobalAccount : IEquatable<IGlobalAccount>
    {
        ulong Id { get; }
        Dictionary<string, string> Tags { get; }
    }
}
