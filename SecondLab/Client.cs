using System.IO.Pipes;
using System.Text;

public class Client
{
    private NamedPipeClientStream pipeClient_;
    private StreamWriter writer_;

    private bool pipeConnected_;

    public Func<string, bool> Connect;
    public Action Disconnect;
    public Action<string> Write;
    public Action Dispose;

    public Client()
    {
        Connect = (address) =>
        {
            try
            {
                pipeClient_ = new NamedPipeClientStream(".", address, PipeDirection.Out, PipeOptions.Asynchronous);
                pipeClient_.Connect();

                writer_ = new StreamWriter(pipeClient_, new UTF8Encoding(false), 65000);
                writer_.AutoFlush = true;
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
                    writer_?.Close();
                    pipeClient_?.Close();
                }
                catch (ObjectDisposedException) { }
                finally
                {
                    pipeClient_?.Dispose();
                    writer_?.Dispose();

                    pipeClient_ = null;
                    writer_ = null;
                }
            }
        };

        Write = (message) =>
        {
            if (pipeConnected_ == false)
                throw new InvalidOperationException("Pipe is not connected");

            lock (writer_)
            {
                writer_.WriteLine(message);
                writer_.Flush();
            }
        };

        Dispose = () =>
        {
            Disconnect();
            pipeClient_?.Dispose();
            writer_?.Dispose();
        };
    }
}
