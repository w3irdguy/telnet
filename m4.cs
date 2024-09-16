using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

class Program
{
    private const string ExpectedLogin = "admin";
    private const string ExpectedPassword = "tcpopen";

    static void Main()
    {
        // Configura o servidor
        string host = "127.0.0.1";
        int port = 65432;

        TcpListener server = new TcpListener(IPAddress.Parse(host), port);
        server.Start();

        Console.WriteLine("Servidor (owned) ouvindo em " + host + ":" + port);

        using (TcpClient client = server.AcceptTcpClient())
        using (NetworkStream stream = client.GetStream())
        {
            // Autenticação
            Console.WriteLine("Digite o login:");
            string login = Console.ReadLine().Trim();
            Console.WriteLine("Digite a senha:");
            string password = Console.ReadLine().Trim();

            if (login != ExpectedLogin || password != ExpectedPassword)
            {
                Console.WriteLine("Autenticação falhou.");
                return;
            }

            Console.WriteLine("Autenticado com sucesso. Aguardando comandos...");

            while (true)
            {
                // Recebe o comando
                byte[] data = new byte[1024];
                int bytes = stream.Read(data, 0, data.Length);
                string receivedMessage = Encoding.UTF8.GetString(data, 0, bytes).Trim();

                // Verifica se a mensagem começa com "CMD:"
                if (receivedMessage.StartsWith("CMD:"))
                {
                    string command = receivedMessage.Substring(4).Trim();

                    if (string.IsNullOrWhiteSpace(command))
                    {
                        break;
                    }

                    // Executa o comando
                    try
                    {
                        ProcessStartInfo processInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = "/C " + command,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (Process process = Process.Start(processInfo))
                        {
                            string output = process.StandardOutput.ReadToEnd();
                            string error = process.StandardError.ReadToEnd();

                            Console.WriteLine("Saída:");
                            Console.WriteLine(output);

                            if (!string.IsNullOrWhiteSpace(error))
                            {
                                Console.WriteLine("Erro:");
                                Console.WriteLine(error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Erro ao executar o comando: " + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Mensagem recebida não é um comando válido.");
                }
            }
        }

        server.Stop();
    }
}
