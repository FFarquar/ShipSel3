using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//This is a test interface to see if the WPF and Blazor apps can access it
namespace SharedLibrary.Services.TestService
{
    public interface ITestInterface
    {
        Task<SharedLibrary.Models.ServiceResponse<String>> ReturnTestMEssageFromInterface(string messageToDisplay);
    }
}
