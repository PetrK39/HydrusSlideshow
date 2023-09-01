using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydrus_Slideshow.Utils
{
    public class CircularEnumerator<T>
    {
        private IEnumerable<T> collection;
        private int index;
        public CircularEnumerator(IEnumerable<T> collection)
        {
            this.collection = collection;
            index = 0;
        }
        public T GetCurrent()
        {
            return collection.ElementAt(index);
        }
        public T GetNext()
        {
            index++;

            if (index > collection.Count() - 1) index = 0;

            return collection.ElementAt(index);
        }
        public T GetPrev()
        {
            index--;

            if (index < 0) index = collection.Count() - 1;

            return collection.ElementAt(index);
        }
    }
}
