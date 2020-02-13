using System;
using System.Collections.Generic;
using System.Text;

namespace DBRepository.Interfaces
{
    public interface IIdentityRepository
    {
        string GetUserName();
        int CheckUserAuthorization();
    }
}
