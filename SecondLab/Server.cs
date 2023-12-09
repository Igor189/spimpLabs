using System.IO.Pipes;

public class Server
{
    private NamedPipeServerStream pipeServer_;
    private StreamReader reader_;

    private bool pipeConnected_;

    public Func<string, bool> Connect;
    public Action Disconnect;
    public Func<string> Read;
    public Func<TimeSpan, string> ReadWithTimeout;
    public Action Dispose;

    public Server()
    {
        Connect = (address) =>
        {
            try
            {
                pipeServer_ = new NamedPipeServerStream(address, PipeDirection.In);
                pipeServer_.WaitForConnection();

                reader_ = new StreamReader(pipeServer_);

                pipeConnected_ = true;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType()}: {ex.Message}");
                Console.WriteLine($"{ex.StackTrace}");
                return false;
            }
        };

        Disconnect = () =>
        {
            if (pipeConnected_)
            {
                pipeConnected_ = false;
                try
                {
                    reader_?.Close();
                    pipeServer_?.Close();
                }
                catch (ObjectDisposedException) { }
                finally
                {
                    pipeServer_?.Dispose();
                    reader_?.Dispose();

                    pipeServer_ = null;
                    reader_ = null;
                }
            }
        };

        Read = () =>
        {
            if (pipeConnected_ == false)
                throw new InvalidOperationException("Pipe is not connected");

            try
            {
                var message = reader_.ReadLine();
                return message;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        };

        ReadWithTimeout = (timeout) =>
        {
            if (pipeConnected_ == false)
                throw new InvalidOperationException("Pipe is not connected");

            Task<string> readTask = reader_.ReadLineAsync();

            if (readTask.Wait(timeout))
            {
                return readTask.Result;
            }
            else
            {
                throw new TimeoutException();
            }
        };

        Dispose = () =>
        {
            Disconnect();
            pipeServer_?.Dispose();
            reader_?.Dispose();
        };
    }
}
