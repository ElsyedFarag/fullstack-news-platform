using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News_Data.Dbintializer
{
    public interface IDbintializer
    {
        Task Initialize();
    }
}
