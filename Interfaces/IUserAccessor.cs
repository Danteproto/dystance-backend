﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Interfaces
{
    public interface IUserAccessor
    {
        string GetCurrentUserId();
    }
}
