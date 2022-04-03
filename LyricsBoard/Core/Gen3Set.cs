using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsBoard.Core
{
    /// <summary>
    /// The generic container for any kind of values/instances of three generations.
    /// </summary>
    internal struct Gen3Set<T>
    {
        public Gen3Set(T standby, T current, T retiring)
        {
            Standby = standby;
            Current = current;
            Retiring = retiring;
        }

        public T Standby { get; set; }
        public T Current { get; set; }
        public T Retiring { get; set; }
    }
}
