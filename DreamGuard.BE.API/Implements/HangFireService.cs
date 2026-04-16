using DreamGuard.BE.BLL.Services.Interfaces;
using Hangfire;
using System.Linq.Expressions;

namespace DreamGuard.BE.API.Implements
{
    public class HangFireService : IHangFireService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        public HangFireService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }
        public void Enqueue<T>(Expression<Func<T, Task>> methodCall)
        {
            try
            {
                _backgroundJobClient.Enqueue<T>(methodCall);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
