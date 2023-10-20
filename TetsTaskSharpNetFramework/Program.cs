using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.ServiceProcess;

namespace TetsTaskSharpNetFramework
{
    class Program
    {
        static Mutex mutex = new Mutex(); 
        static void getIdThread()
        {
            Console.WriteLine($"ID текущего потока: {Thread.CurrentThread.ManagedThreadId}");
        }
        static void startProccess(string serviceName, ref List<ServiceController> services)
        {
            mutex.WaitOne();
            getIdThread();
            ServiceController service = new ServiceController(serviceName);
            try
            {
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }
                services.Add(service);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при запуске сервиса: " + ex.Message);
            }
            mutex.ReleaseMutex();
        }
        static void ViewServiceStatus(List<ServiceController> services)
        {
            try
            {
                Console.Clear();
                foreach (var service in services)
                {
                    service.Refresh();
                    Console.WriteLine($"Название сервиса: {service.DisplayName}");
                    Console.WriteLine($"Состояние сервиса: {service.Status}");
                    Console.WriteLine($"Можноли приостоновить работу сервиса: {service.CanPauseAndContinue}");
                    Console.WriteLine($"Необходимо ли уведомлять о преостановке сервиса: {service.CanShutdown}");
                    Console.WriteLine($"\n{new string('-', 50)}\n");
                }
                Thread.Sleep(2000);
                ViewServiceStatus(services);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении данных о сервисе: " + ex.Message);
            }
        }
        static void Main(string[] args)
        {
            List<ServiceController> services = new List<ServiceController>();

            string[] serviceNames = new string[3] { "aspnet_state", "Schedule", "RasAuto" }; // сервисы которые запускает программа(первый сервис не на всех пк есть)
            Parallel.Invoke(
                () => startProccess(serviceNames[0], ref services),
                () => startProccess(serviceNames[1], ref services),
                () => startProccess(serviceNames[2], ref services)
                ); // заупскаю три сервиса паралельно в разных потоках

            Thread.Sleep(3000); // 3-х секндная задержка чтобы был виден вывод того что все запущено в разных потоках(сделал ток для того чтобы можно было это увидеть)

            ViewServiceStatus(services); // возлагаю вывод данных о сервисах на основной поток
        }
    }
}
