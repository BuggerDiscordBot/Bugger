using System.Collections.Generic;

namespace Bugger.Configuration
{
    public class InMemoryDataStorage : IDataStorage
    {
        private readonly Dictionary<string, object> storage;

        public InMemoryDataStorage()
        {
            storage = new Dictionary<string, object>();
        }

        public bool KeyExists(string key)
        {
            return storage.ContainsKey(key);
        }

        public T RestoreObject<T>(string key)
        {
            // NOTE(Peter): This is not handled as an
            // exception is assumed to be thrown when
            // trying to get a non existent type.
            return (T)storage[key];
        }

        public void StoreObject(object obj, string key)
        {
            if(storage.ContainsKey(key)) 
            { 
                storage[key] = obj; 
            }
            else
            {
                storage.Add(key, obj);
            }
        }
    }
}
