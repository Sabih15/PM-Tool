using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMTool.Models.DTOs
{
    public class UsersListDto
    {
        public List<UsersDto> resources { get; set; }
        public int count { get; set; }
    }
}
