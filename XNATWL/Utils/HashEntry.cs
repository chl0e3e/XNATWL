using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Utils
{
    public class HashEntry<K, T> where T : HashEntry<K, T>
    {
        public K key;
        int hash;
        public T nextEntry;

        public HashEntry(K key)
        {
            this.key = key;
            this.hash = key.GetHashCode();
        }

        public T next()
        {
            return nextEntry;
        }

        public void setNext(T n)
        {
            this.nextEntry = n;
        }

        public static T get<K, T>(T[] table, Object key) where T : HashEntry<K, T>
        {
            int hash = key.GetHashCode();
            T e = table[hash & (table.Length - 1)];
            Object k;
            while (e != null && (e.hash != hash || (((k = e.key) != key) && !key.Equals(k))))
            {
                e = e.nextEntry;
            }
            return e;
        }

        public static void insertEntry<K, T>(T[] table, T newEntry) where T : HashEntry<K, T>
        {
            int idx = newEntry.hash & (table.Length - 1);
            newEntry.nextEntry = table[idx];
            table[idx] = newEntry;
        }

        public static T remove<K, T>(T[] table, Object key) where T : HashEntry<K, T>
        {
            int hash = key.GetHashCode();
            int idx = hash & (table.Length - 1);
            T e = table[idx];
            T p = null;
            Object k;
            while (e != null && (e.hash != hash || (((k = e.key) != key) && !key.Equals(k))))
            {
                p = e;
                e = e.nextEntry;
            }
            if (e != null)
            {
                if (p != null)
                {
                    p.nextEntry = e.nextEntry;
                }
                else
                {
                    table[idx] = e.nextEntry;
                }
            }
            return e;
        }

        public static void remove<K, T>(T[] table, T entry) where T : HashEntry<K, T>
        {
            int idx = entry.hash & (table.Length - 1);
            T e = table[idx];
            if (e == entry)
            {
                table[idx] = e.nextEntry;
            }
            else
            {
                T p;
                do
                {
                    p = e;
                    e = e.nextEntry;
                } while (e != entry);
                p.nextEntry = e.nextEntry;
            }
        }

        public static T[] maybeResizeTable<K, T>(T[] table, int usedCount) where T : HashEntry<K, T>
        {
            if (usedCount * 4 > table.Length * 3)
            {
                table = resizeTable<K, T>(table, table.Length * 2);
            }
            return table;
        }

        private static T[] resizeTable<K, T>(T[] table, int newSize) where T : HashEntry<K, T>
        {
            if (newSize < 4 || (newSize & (newSize - 1)) != 0)
            {
                throw new ArgumentOutOfRangeException("newSize");
            }

            T[] newTable = (T[])Array.CreateInstance(table.GetType().GetElementType(), newSize);
            for (int i = 0, n = table.Length; i < n; i++)
            {
                for (T e = table[i]; e != null;)
                {
                    T ne = e.nextEntry;
                    int ni = e.hash & (newSize - 1);
                    e.nextEntry = newTable[ni];
                    newTable[ni] = e;
                    e = ne;
                }
            }
            return newTable;
        }
    }
}
