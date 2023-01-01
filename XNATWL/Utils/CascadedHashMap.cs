using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.DialogLayout;

namespace XNATWL.Utils
{
    public class CascadedHashMap<K, Object> : Dictionary<K, object>
    {
        public CascadedHashMap<K, Object> fallback = null;

        public CascadedHashMap()
        {
        }

        public void CollapseAndSetFallback(CascadedHashMap<K, Object> map)
        {
            if (fallback != null)
            {
                this.CollapsePutAll(map);
                fallback = null;
            }

            fallback = map;
        }

        public void CollapsePutAll(CascadedHashMap<K, Object> map)
        {
            do
            {
                K[] mapKeys = map.Keys.ToArray();
                for (int i = 0, n = mapKeys.Count(); i < n; i++)
                {
                    object mapEntry = map[mapKeys[i]];
                    if (mapEntry != null)
                    {
                        if (!this.ContainsKey(mapKeys[i]))
                        {
                            this[mapKeys[i]] = mapEntry;
                        }
                    }
                }
                map = map.fallback;
            } while (map != null);
        }

        public object CascadingEntry(K key)
        {
            return getEntry(this, key);
        }

        public object PutCascadingEntry(K key, object value)
        {
            if (this.ContainsKey(key))
            {
                object oldValue = this[key];
                this[key] = value;
                return oldValue;
            }
            else
            {
                object cascadedEntry = CascadingEntry(key);
                this[key] = value;
                return cascadedEntry;
            }
        }

        protected static object getEntry<K>(CascadedHashMap<K, Object> map, K key)
        {
            do
            {
                if (map.ContainsKey(key) && map[key] != null)
                {
                    return map[key];
                }
                map = map.fallback;
            } while (map != null);

            return null;
        }
    }
}
