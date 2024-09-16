using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

class Program
{
    private const int Port = 5000;
    
    static void Main()
    {
        TcpListener server = null;
        try
        {
            // Configuração do listener
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, Port);
            server.Start();

            Console.WriteLine("Aguardando conexões...");

            while (true)
            {
                // Aceita uma conexão
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Conexão recebida.");

                // Cria uma thread para lidar com o cliente
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    // Recebe o comando do cliente
                    string command = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Comando recebido: " + command);

                    // Executa o comando e captura a saída
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C " + command,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    
                    using (Process process = Process.Start(startInfo))
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        string result = output + error;

                        // Envia a saída de volta ao cliente
                        byte[] response = Encoding.UTF8.GetBytes(result);
                        stream.Write(response, 0, response.Length);
                    }
                }

                client.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Erro: " + e.Message);
        }
        finally
        {
            server.Stop();
        }
    }
}
