using System;
using System.Collections.Generic;
using System.Text;

namespace Bugger.Configuration
{
    public interface IDataStorage
    {
        void StoreObject(object obj, string key);
        T RestoreObject<T>(string key);
        bool KeyExists(string key);
    }
}
