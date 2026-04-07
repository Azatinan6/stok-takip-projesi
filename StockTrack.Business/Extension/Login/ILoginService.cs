using StockTrack.Dto.Login;
using StockTrack.Entity.Enitities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrack.Business.Extension.Login
{
    public interface ILoginService
    {
        Task<(bool Success, string ErrorMessage, AppUser appUser)> LoginAsync(LoginDto loginDto);
    }
}
