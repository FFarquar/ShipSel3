using Microsoft.Extensions.Logging;
using SharedLibrary.Models;
using SharedLibrary.Services.UploadDownloadService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Services.TestService
{
    public class TestInterface : ITestInterface
    {

        //private readonly HttpClient _http;
        private readonly ILogger<TestInterface> _logger;
        

        //public TestInterface()
        //{
            
        //}
        //public TestInterface(HttpClient http, ILogger<TestInterface> logger)
        public TestInterface(ILogger<TestInterface> logger)
        
        {
            //_http = http;
            _logger = logger;
        }

        public async Task<SharedLibrary.Models.ServiceResponse<string>> ReturnTestMEssageFromInterface(string messageToDisplay)
        {
            _logger.LogInformation("Call made to ReturnTestMessageFromInterface. Caller message " + messageToDisplay);

            return new SharedLibrary.Models.ServiceResponse<string>
            {
                Data = "ReturnTestMEssageFromInterface. " + messageToDisplay,
                Success = true,
                Message = "This worked"
            };
        }
    }
}
