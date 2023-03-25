using MongoDB.Driver;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using TestMate.Common.Enums;
using TestMate.Common.Models.TestRuns;

namespace TestMate.Runner.BackgroundServices
{
    public class QueuingService : IDisposable
    {
        private readonly ILogger<QueuingService> _logger;
        private readonly Timer _timer;
        private readonly IMongoCollection<TestRun> _testRunsCollection;
        private readonly IModel _channel;

        private readonly int _batchTimerInterval = 300000; //milliseconds
        private readonly int _batchSize = 10;

        string testRunExchange = "TestRunExchange";
        string testRunQueue = "test-run-queue";
        string testRunRoutingKey = "testRunRoutingKey";

        QueuePrioritisationStrategy priorityStrategy = QueuePrioritisationStrategy.FIFO;

        public QueuingService(ILogger<QueuingService> logger, IMongoDatabase database, IModel channel) 
        {
            _logger = logger;
            _timer = new Timer(async state => await OnTimerElapsed(), null, 0, _batchTimerInterval);
            _testRunsCollection = database.GetCollection<TestRun>("TestRuns");
            _channel = channel;
        }

        private async Task OnTimerElapsed()
        {
            try
            {
                IQueryable<TestRun> testRuns = _testRunsCollection.AsQueryable()
                    .Where(tr => tr.Status == TestRunStatus.New && tr.NextAvailableProcessingTime <= DateTime.UtcNow);

                switch (priorityStrategy)
                {
                    case QueuePrioritisationStrategy.FIFO:
                        //default retrieval order, no need to order
                        break;

                    case QueuePrioritisationStrategy.BalancedDevelopers:
                        testRuns = testRuns.OrderBy(tr => tr.Requestor)
                                         .ThenBy(tr => tr.PriorityLevel)
                                         .ThenBy(tr => tr.NextAvailableProcessingTime);
                                        //RETRY COUNT???
                        break;
                    
                    default:
                        throw new ArgumentException("Invalid prioritization type");

                }

                testRuns.Take(_batchSize);
                
                //publish messages to Exchange
                foreach(TestRun testRun in testRuns)
                {
                    try
                    {
                        string message = JsonConvert.SerializeObject(testRun);
                        _logger.LogInformation($"Publishing message: {message} to exchange '{testRunExchange}'");

                        // Publish the message to the queue
                        Byte[] body = Encoding.UTF8.GetBytes(message);
                        _channel.BasicPublish(
                            exchange: testRunExchange,
                            routingKey: testRunRoutingKey,
                            basicProperties: null,
                            body: body
                            );

                        _logger.LogInformation("Published successfully!");
                            
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to publish Test Run document {testRun.Id} in queue!");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Queueing mechanism!");
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
