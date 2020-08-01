using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS.Services
{
    public class SharedService : ISharedService
    {
       public string SendSuccessRespond(int code, string message) 
       {

            return "dsds";
       }

       public string SendErrorRespond(int code, string message, string error) 
        {
            return "dsds";

        }
    }
}
