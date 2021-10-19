using System;
using System.Threading.Tasks;

namespace Reviewer.Core
{
    public interface IMicrosoftAuthService
    {
        void Initialize();
        Task<User> OnSignInAsync();
        Task OnSignOutAsync();
    }
}
