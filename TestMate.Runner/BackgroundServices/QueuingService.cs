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

        private readonly int _batchInterval = 60; //45 seconds
        private readonly int _batchSize = 1000;
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
                var watch = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    _logger.LogDebug("====== Queuing Service Started ======");

                    var pendingTestRuns = await _testRunsCollection.Find(tr => tr.Status == TestRunStatus.New && tr.NextAvailableProcessingTime <= DateTime.UtcNow).ToListAsync();

                    
                    _logger.LogInformation($"[PROBE_QS-1] [Task {Task.CurrentId}] Retrieved Pending Test Runs from DB in {watch.ElapsedMilliseconds} ms");

                    List<TestRun> prioritisedTestRuns = new List<TestRun>();

                    if (pendingTestRuns.Count > 0)
                    {
                        switch (priorityStrategy)
                        {
                            case QueuePrioritisationStrategy.FIFO:
                                
                                //default retrieval order, no need to order
                                prioritisedTestRuns = pendingTestRuns.Take(_batchSize).ToList(); //0(1)
                                
                                _logger.LogInformation($"[PROBE_QS-2] [Task {Task.CurrentId}] Time consumed to Retrieve and Prioritise using FIFO Strategy on a total of {pendingTestRuns.Count} Pending Test Runs is {watch.ElapsedMilliseconds} ms");

                                break;

                            case QueuePrioritisationStrategy.BalancedDevelopers:

                                var groupedTestRuns = pendingTestRuns.GroupBy(r => r.Requestor);  //Check O notation for grouping

                                // Sort test runs within each group by priority level in descending order
                                foreach (var group in groupedTestRuns) //TODO: to check with ChrisColombo
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

                                _logger.LogInformation($"[PROBE_QS-2] [Task {Task.CurrentId}] Time consumed to Retrieve and Prioritise using BalancedDevelopers Strategy on a total of {pendingTestRuns.Count} Pending Test Runs is {watch.ElapsedMilliseconds} ms");

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
                                _logger.LogError(ex, $"[ERROR] [Task {Task.CurrentId}] Failed to publish Test Run document {testRun.Id} in queue!");
                            }
                        }
                        _logger.LogInformation($"[PROBE_QS-3] [Task {Task.CurrentId}] Updated TestRun Status to InQueue and Sent To RabbitMQ of {runsToBeQueued} Pending Test Runs - Elapsed {watch.ElapsedMilliseconds} ms");

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
                    if (watch.IsRunning)
                    {
                        watch.Stop();
                    }
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
