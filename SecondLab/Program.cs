class Program
{
    static Server firstServerPipe_ = new Server();
    static Client firstClientPipe_ = new Client();

    static Server secondServerPipe_ = new Server();
    static Client secondClientPipe_ = new Client();

    static int timeout_ = 5000;

    static Action RedirectMessage = () =>
    {
        try
        {
            while (true)
            {

                var message = firstServerPipe_.ReadWithTimeout(TimeSpan.FromMilliseconds(timeout_));
                Task.Run(() => secondClientPipe_.Write(message));
                Console.WriteLine(secondServerPipe_.Read());
            }
        }
        catch (TimeoutException e)
        {
            Task.Run(() => secondClientPipe_.Write(":timeout"));
            Console.WriteLine(secondServerPipe_.Read());
        }
    };

    static Func<string, string, Task> Connect = async (firstAddres, secondAddres) =>
    {
        await Task.WhenAll(
            Task.Run(() => firstClientPipe_.Connect(firstAddres)),
            Task.Run(() => firstServerPipe_.Connect(firstAddres)),
            Task.Run(() => secondClientPipe_.Connect(secondAddres)),
            Task.Run(() => secondServerPipe_.Connect(secondAddres))
        );
    };

    static Action TestPipeWrite = () =>
    {
        Enumerable.Range(0, 5).ToList().ForEach(i =>
        {
            firstClientPipe_.Write("test message");
            Thread.Sleep(3000);
        });
    };

    static void Main()
    {
        var firstAddres = "first";
        var secondAddres = "second";

        var task = Connect(firstAddres, secondAddres);
        task.Wait();
        Task.Run(() => TestPipeWrite());
        RedirectMessage();
    }
}
