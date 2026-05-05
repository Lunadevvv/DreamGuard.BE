using DreamGuard.BE.BLL.Services.Interfaces;
using Hangfire;
using System.Linq.Expressions;

namespace DreamGuard.BE.API.Implements
{
    public class HangFireService : IHangFireService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        public HangFireService(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
        {
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
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

        public void AddOrUpdateRecurringJob<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression)
        {
            try
            {
                _recurringJobManager.AddOrUpdate<T>(recurringJobId, methodCall, cronExpression);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
