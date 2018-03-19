using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades;
using ServicioClienteITCM;
using ExcelMapper;
using System.Threading;
using System.IO;

namespace TestServicio
{
    class Program
    {
        static bool running;
        static TimeSpan startTimeSpan;
        static TimeSpan periodTimeSpan;
        static System.Threading.Timer timer;

        static void Main(string[] args)
        {
            running = true;
            startTimeSpan = TimeSpan.Zero;
            periodTimeSpan = TimeSpan.FromMinutes(2);

            TimerCallback timerCallback = new TimerCallback(
                (e) => 
                {
                    //if (DateTime.Now.Hour == 14 || DateTime.Now.Hour == 19)
                    if (DateTime.Now.Hour == 9 || DateTime.Now.Hour == 10)
                        RunAsync(e);
                    else
                        Console.WriteLine("It's not time to execute thread");
                });

            ExcelMapper<Circuito> mapper = new ExcelMapper<Circuito>();
            timer = new System.Threading.Timer(timerCallback, mapper, Timeout.Infinite, Timeout.Infinite);
            Start();
            
            while (running)
            {
               
            }
        }

        static void Start()
        {
            timer.Change(startTimeSpan, periodTimeSpan);
            running = true;
            Console.WriteLine("Service is running");
        }

        static void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            running = false;
            Console.WriteLine("Service is stopped");
        }

        private static void RunAsync(object mapper)
        {
            try
            {
                ExcelMapper<Circuito> _circuito_excel_mapper = (mapper is ExcelMapper<Circuito>) ? 
                    (ExcelMapper<Circuito>)mapper : new ExcelMapper<Circuito>();
                
                ClienteCircuito circuitoClient = new ClienteCircuito();
                ClienteContacto contactoClient = new ClienteContacto(circuitoClient.Token);
                
                Task<IEnumerable<Circuito>> _task_circuitos = circuitoClient.GetCircuitos();
                Task<IEnumerable<Cliente>> _task_clientes = contactoClient.GetContactos();

                Task _task_circuitos_completed = _task_circuitos.ContinueWith(
                    async (_task_completed) =>
                    {
                        List<Circuito> circuitos = (_task_completed.Result != null ? _task_completed.Result.ToList() : null);
                        if (circuitos != null && circuitos.Count > 0)
                        {
                            bool exported = await RunExportAndUploadTaskAsync<Circuito>(circuitos);
                        }
                        else
                        {
                            Console.WriteLine($"Service was unable to retrieve data ({nameof(Circuito)})");
                        }
                    });

                Task _task_clientes_completed = _task_clientes.ContinueWith(
                    async (_task_completed) => 
                    {
                        List<Cliente> clientes = (_task_completed.Result != null ? _task_completed.Result.ToList() : null);
                        if (clientes != null && clientes.Count > 0)
                        {
                            bool exported = await RunExportAndUploadTaskAsync<Cliente>(clientes);
                        }
                        else
                        {
                            Console.WriteLine($"Service was unable to retrieve data ({nameof(Cliente)})");
                        }
                    });

                Task.WhenAll(_task_circuitos, _task_clientes).ContinueWith(
                    (done) => 
                    {
                        Console.WriteLine(string.Format("Data retrieved successfully"));
                    });

                Task.WhenAll(_task_circuitos_completed, _task_clientes_completed).ContinueWith(
                    (done) => 
                    {
                        Console.WriteLine($"Files exported and uploaded successfullly");
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.Source);
                Console.WriteLine(ex.StackTrace);
            }            
        }

        private static async Task<bool> RunExportAndUploadTaskAsync<T>(List<T> _list) where T: class
        {
            bool exported = false;
            if (_list != null && _list.Count > 0)
            {
                ExcelMapper<T> _excel_mapper = new ExcelMapper<T>();                
                exported = await _excel_mapper.Export(_list);
                if (exported)
                {
                    string exportedFile = _excel_mapper.OldPath;
                    Console.WriteLine(exportedFile);
                    ClienteFTP ftpClient = new ClienteFTP(exportedFile);
                    bool wasUploaded = ftpClient.UploadFile();
                    if (wasUploaded)
                    {
                        Console.WriteLine($"{Path.GetFileName(exportedFile)} uploaded successfully");

                    }
                }
            }
            return exported;
        }
    }
}
