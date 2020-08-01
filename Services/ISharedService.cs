using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS.Services
{
    public interface ISharedService
    {
        string SendSuccessRespond(int code, string message);

        string SendErrorRespond(int code, string message,string error);
    }
}
