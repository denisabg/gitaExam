using Bogus;
using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace BackendExam
{
    class ProducerConsumer : IDisposable
    {
        private Task m_ProducerTask;
        private Faker m_Faker = new Faker();

        //  Observable  --  schedules calls concurrently
        private Subject<ReceivedDataItem> _subject = new Subject<ReceivedDataItem>();

        public ProducerConsumer(CancellationToken i_Token)
        {
            _subject.Subscribe(OnItemRecieved);

            m_ProducerTask = Task.Run(async () =>
            {
                while (!i_Token.IsCancellationRequested)
                {
                    var data = await GenerateSampleData();
                    var item = new ReceivedDataItem(data);
                    _subject.OnNext(item);
                }
            }, i_Token);
        }

        private void OnItemRecieved(ReceivedDataItem item)
        {
            DecodeData(item.Data);
            Console.WriteLine(item.Timestamp.ToString("u"));
        }

        public async Task<string> GenerateSampleData()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(m_Faker.Random.Int(300, 1000))); //Simulate text generator in random interval between 300 ms to 1 sec
            return m_Faker.Lorem.Sentence(m_Faker.Random.Int(0, 100), m_Faker.Random.Int(0, 100));
        }

        public void DecodeData(string i_Data)
        {
            var processingDelay = TimeSpan.FromMilliseconds(m_Faker.Random.Int(10, 300)); //Simulated text decoder that completed in 10 to 300 ms
            Thread.Sleep(processingDelay);
        }

        public Task WaitForCompletion() => Task.WhenAll(m_ProducerTask ?? Task.CompletedTask);

        public void Dispose()
        {
            //  when ready to unsubscribe...
            _subject.Dispose();
        }
    }
}
