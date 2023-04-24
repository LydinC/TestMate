using MongoDB.Driver;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using TestMate.Common.Enums;
using TestMate.Common.Models.Devices;
using TestMate.Common.Models.TestRuns;
using TestMate.Common.Utils;

namespace TestMate.Runner.BackgroundServices
{
    public class QueuingService : BackgroundService
    {
        private readonly ILogger<QueuingService> _logger;
        private readonly IMongoCollection<TestRun> _testRunsCollection;
        private readonly IModel _channel;

        private readonly int _batchInterval = 60; //seconds
        private readonly int _batchSize = 10;
        QueuePrioritisationStrategy priorityStrategy = QueuePrioritisationStrategy.BalancedDevelopers;

        string testRunExchange = "TestRunExchange";
        string testRunQueue = "test-run-queue";
        string testRunRoutingKey = "testRunRoutingKey";

        public QueuingService(ILogger<QueuingService> logger, IMongoDatabase database, IModel channel) 
        {
            _logger = logger;
            _testRunsCollection = database.GetCollection<TestRun>("TestRuns");
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await PerformQueuingJob(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error encountered during queuing service routine.");
                }

                await Task.Delay(TimeSpan.FromSeconds(_batchInterval), cancellationToken);
            }
        }

        private async Task PerformQueuingJob(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                try
                {
                    _logger.LogDebug("====== Queuing Service Started ======");
                    var pendingTestRuns = await _testRunsCollection.Find(tr => tr.Status == TestRunStatus.New && tr.NextAvailableProcessingTime <= DateTime.UtcNow).ToListAsync();
                    List<TestRun> prioritisedTestRuns = new List<TestRun>();

                    if (pendingTestRuns.Count > 0)
                    {
                        switch (priorityStrategy)
                        {
                            case QueuePrioritisationStrategy.FIFO:
                                //default retrieval order, no need to order
                                prioritisedTestRuns = pendingTestRuns.Take(_batchSize).ToList();

                                break;

                            case QueuePrioritisationStrategy.BalancedDevelopers:

                                var groupedTestRuns = pendingTestRuns.GroupBy(r => r.Requestor);

                                // Sort test runs within each group by priority level in descending order
                                foreach (var group in groupedTestRuns)
                                {
                                    group.OrderBy(r => r.PriorityLevel)
                                          .ThenBy(r => r.NextAvailableProcessingTime)
                                          .ThenByDescending(r => r.RetryCount);
                                }

                                //using round robin technique to prioritise test runs across different developers
                                while (prioritisedTestRuns.Count < _batchSize && prioritisedTestRuns.Count < pendingTestRuns.Count)
                                {
                                    foreach (var group in groupedTestRuns)
                                    {
                                        TestRun? prioritisedTestRun = group.FirstOrDefault(r => prioritisedTestRuns.Contains(r) == false);
                                        if (prioritisedTestRun != null)
                                        {
                                            prioritisedTestRuns.Add(prioritisedTestRun);
                                        }
                                    }
                                }

                               break;

                            default:
                                throw new ArgumentException("Invalid prioritisation strategy");

                        }

                        int runsToBeQueued = prioritisedTestRuns.Count();


                        //publish messages to Exchange
                        foreach (TestRun testRun in prioritisedTestRuns)
                        {
                            try
                            {
                                //Update status to InQueue
                                await updateTestRunStatus(testRun, TestRunStatus.InQueue);

                                // Publish deserialised test run as message to the queue
                                string message = JsonConvert.SerializeObject(testRun);
                                _logger.LogDebug($"Publishing message: {message} to exchange '{testRunExchange}'");
                                Byte[] body = Encoding.UTF8.GetBytes(message);
                                _channel.BasicPublish(
                                    exchange: testRunExchange,
                                    routingKey: testRunRoutingKey,
                                    basicProperties: null,
                                    body: body
                                    );
                                _logger.LogDebug("Published successfully!");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Failed to publish Test Run document {testRun.Id} in queue!");
                            }
                        }
                        _logger.LogDebug($"Queued Test Runs: {runsToBeQueued}");
                    }
                    else {
                        _logger.LogDebug($"No pending test runs to be queued identified.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process Queueing mechanism!");
                }
                finally
                {
                    _logger.LogDebug("====== Queuing Service Ended ======");
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }, cancellationToken);

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queueing Service is stopping");
            await base.StopAsync(cancellationToken);
        }


        public async Task updateTestRunStatus(TestRun testRun, TestRunStatus status)
        {
            var testRunFilter = Builders<TestRun>.Filter.Where(x => x.Id == testRun.Id);
            var testRunUpdate = Builders<TestRun>.Update.Set(x => x.Status, status);
            await _testRunsCollection.UpdateOneAsync(testRunFilter, testRunUpdate);
        }
    }
}
