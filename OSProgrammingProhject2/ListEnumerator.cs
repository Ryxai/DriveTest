using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSProgrammingProject2
{
    public class ListEnumerator<T>:List<T>
    {
        private int cursor;

        public T Current
        {
            get
            {
                if (cursor <= -1 || cursor > this.Count - 1) return default(T);
                else return this[cursor];
            }
        }

        public bool MoveNext()
        {
            cursor++;
            return cursor < this.Count;
        }

        public void Reset()
        {
            cursor = -1;
        }

        public ListEnumerator(IEnumerable<T> enumerable)
            : base(enumerable)
        {
            cursor = -1;
        }  
    }
}
