using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public interface CacheContext : Resource
    {
        bool Valid
        {
            get;
        }
    }
}
