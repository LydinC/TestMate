using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMate.Common.DataTransferObjects.Developers
{
    public class DeveloperLoginResultDTO
    {
        public string Token { get; set; }
        public bool Success { get; set; }
        public string Message {get; set; } 
    }
}
