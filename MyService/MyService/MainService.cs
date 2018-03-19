using Entidades;
using ExcelMapper;
using ServicioClienteITCM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyService
{
    public partial class MainService : ServiceBase
    {
        TimeSpan startTimeSpan;
        TimeSpan periodTimeSpan;
        TimerCallback timerCallback;
        ExcelMapper<Circuito> mapper;
        System.Threading.Timer timer;

        private const string LOOGER = "ITCM Servicio Event Log";
        private const string SOURCE = "itcservicio_source";

        public MainService()
        {
            InitializeComponent();           
            this.Logger.Source = SOURCE;
            this.Logger.Log = LOOGER;
            Settings();
        }

        private void Settings()
        {
            //this.startTimeSpan = TimeSpan.FromSeconds(5);
            //this.periodTimeSpan = TimeSpan.FromHours(1);
            this.startTimeSpan = TimeSpan.FromSeconds(5);
            this.periodTimeSpan = TimeSpan.FromHours(1);

            this.timerCallback = new TimerCallback(
                (e) =>
                {
                    //if (DateTime.Now.Hour == 9 || DateTime.Now.Hour == 10)
                    if (DateTime.Now.Hour == 14 || DateTime.Now.Hour == 19)
                        RunAsync(e);
                    else
                        this.Logger.WriteEntry($"On {nameof(Settings)}: It's not time to execute thread", EventLogEntryType.Information);                    
                });
            this.mapper = new ExcelMapper<Circuito>();
            this.timer = new System.Threading.Timer(this.timerCallback, this.mapper, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void OnStart(string[] args)
        {
            this.Logger.WriteEntry($"On {nameof(OnStart)}", EventLogEntryType.Information);
            if (this.timer != null)
            {                
                this.timer.Change(this.startTimeSpan, this.periodTimeSpan);
            }
        }

        protected override void OnStop()
        {
            this.Logger.WriteEntry($"On {nameof(OnStop)}", EventLogEntryType.Information);
            if (this.timer != null)
            {
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        protected override void OnContinue()
        {            
            this.Logger.WriteEntry($"On {nameof(OnContinue)}", EventLogEntryType.Information);
            if (this.timer != null)
            {
                this.timer.Change(this.startTimeSpan, this.periodTimeSpan);
            }
            base.OnContinue();
        }
        
        protected override void OnShutdown()
        {            
            this.Logger.WriteEntry($"On {nameof(OnShutdown)}", EventLogEntryType.Information);
            if (this.timer != null)
            {
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            base.OnShutdown();
        }

        private void RunAsync(object mapper)
        {
            string _methodName = nameof(RunAsync);
            try
            {
                ExcelMapper<Circuito> excelMapper = (mapper is ExcelMapper<Circuito>) ?
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
                            bool success = await RunExportAndUploadTaskAsync<Circuito>(circuitos);
                            if (success)
                            {
                                this.Logger.WriteEntry($"On {_methodName}: Exporting and Uploading Task ({nameof(Circuito)}) completed successfully", EventLogEntryType.Information);
                            }
                            else
                            {
                                this.Logger.WriteEntry($"On {_methodName}: Service was unable to retrieve data ({nameof(Circuito)})", EventLogEntryType.Information);                                
                            }
                        }
                    });

                Task _task_clientes_completed = _task_clientes.ContinueWith(
                    async (_task_completed) =>
                    {
                        List<Cliente> clientes = (_task_completed.Result != null ? _task_completed.Result.ToList() : null);
                        if (clientes != null && clientes.Count > 0)
                        {
                            bool success = await RunExportAndUploadTaskAsync<Cliente>(clientes);
                            if (success)
                            {
                                this.Logger.WriteEntry($"On {_methodName}: Exporting and Uploading Task ({nameof(Cliente)}) completed successfully", EventLogEntryType.Information);
                            }
                            else
                            {
                                this.Logger.WriteEntry($"On {_methodName}: Service was unable to retrieve data ({nameof(Cliente)})", EventLogEntryType.Information);                                
                            }
                        }
                    });

                Task.WhenAll(_task_circuitos, _task_clientes).ContinueWith(
                    (done) =>
                    {
                        this.Logger.WriteEntry($"On {_methodName}: Data ({nameof(Cliente)} and {nameof(Circuito)}) retrieved successfully", EventLogEntryType.Information);                        
                    });

                Task.WhenAll(_task_circuitos_completed, _task_clientes_completed).ContinueWith(
                    (done) =>
                    {
                        this.Logger.WriteEntry($"On {_methodName}: Files ({nameof(Cliente)} and {nameof(Circuito)}) exported and uploaded successfullly", EventLogEntryType.Information);                        
                    });
            }
            catch (Exception ex)
            {
                this.Logger.WriteEntry(string.Format("On RunAsync: {0}", ex.Message), EventLogEntryType.Error);
                this.Logger.WriteEntry(string.Format("On RunAsync: {0}", ex.Source), EventLogEntryType.Error);
                this.Logger.WriteEntry(string.Format("On RunAsync: {0}", ex.StackTrace), EventLogEntryType.Error);
                this.Logger.WriteEntry(string.Format("On RunAsync: {0}", ex.InnerException.Message), EventLogEntryType.Error);                
            }
        }

        private async Task<bool> RunExportAndUploadTaskAsync<T>(List<T> _list) where T : class
        {
            string _methodName = nameof(RunExportAndUploadTaskAsync);            
            bool success = false;
            if (_list != null && _list.Count > 0)
            {
                ExcelMapper<T> _excel_mapper = new ExcelMapper<T>();
                bool exported = await _excel_mapper.Export(_list);
                if (exported)
                {
                    string exportedFile = _excel_mapper.OldPath;
                    this.Logger.WriteEntry($"On {_methodName}: Excel File ({Path.GetFileName(exportedFile)}) created and exported successfully", EventLogEntryType.Information);                    
                    ClienteFTP ftpClient = new ClienteFTP(exportedFile);
                    bool wasUploaded = ftpClient.UploadFile();
                    if (wasUploaded)
                    {
                        this.Logger.WriteEntry($"On {_methodName}: {Path.GetFileName(exportedFile)} uploaded successfully", EventLogEntryType.Information);                        
                    }
                    success = exported && wasUploaded;
                }
            }
            return success;
        }
    }
}
