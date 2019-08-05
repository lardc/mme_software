using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.InterfaceImplementations
{
    class LongTimeRoutineWorker
    {
        private BackgroundWorker FWorker = null;

        public delegate void LongTimeRoutineDelegate(InputWorkerParameters WorkerParameters);
        public delegate void CompletedRoutineDelegate(string Error);

        private LongTimeRoutineDelegate FLongTimeRoutineHandler = null;
        private CompletedRoutineDelegate FCompletedRoutineHandler = null;

        public void Run(InputWorkerParameters Parameters)
        {
            //запуск продолжительно выполняющегося кода с передачей для его исполнения входных параметров Parameters
            this.FWorker.RunWorkerAsync(Parameters);
        }

        public LongTimeRoutineWorker(LongTimeRoutineDelegate LongTimeRoutineHandler, CompletedRoutineDelegate CompletedRoutineHandler)
        {
            //запоминаем то, что нам надо вызывать 
            this.FLongTimeRoutineHandler = LongTimeRoutineHandler;
            this.FCompletedRoutineHandler = CompletedRoutineHandler;

            //создаём необходимые механизмы того, что позволит нам вызывать наши реализации в потоке
            this.FWorker = new BackgroundWorker();

            //установка той реализации, которая будет исполняться в потоке
            this.FWorker.DoWork += new DoWorkEventHandler(LongTimeRoutineEvent);

            //установка той реализации, которая будет исполняться по завершению исполнения LongTimeRoutine 
            this.FWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedRoutineEvent);
        }

        private void LongTimeRoutineEvent(object sender, DoWorkEventArgs e)
        {
            //реализация, которая очень долго выполняется
            if (this.FLongTimeRoutineHandler != null)
            {
                //извлекаем параметры, которые нужны для исполнения потоковой функции
                InputWorkerParameters WorkerParameters = (InputWorkerParameters)e.Argument;

                //запускаем потоковую функцию
                this.FLongTimeRoutineHandler(WorkerParameters);
            }
        }

        private void CompletedRoutineEvent(object sender, RunWorkerCompletedEventArgs e)
        {
            //реализация, которая будет вызвана по окончании исполнения LongTimeRoutine
            //раз мы здесь - потоковая функция как-то исполнена - возможно успешно, а возможно с ошибкой
            if (this.FCompletedRoutineHandler != null)
            {
                //передаём в вызываемую реализацию текст ошибки, чтобы она могла понять чем закончилась синхронизация
                string Error;

                switch (e.Error == null)
                {
                    case (true):
                        Error = "";
                        break;

                    default:
                        Error = e.Error.ToString();
                        break;
                }

                this.FCompletedRoutineHandler(Error);
            }
        }
    }

    class InputWorkerParameters
    {
        //описание входных параметров для BackGround Worker
        public int Timeout
        { get; set; }

        public InputWorkerParameters(int timeout)
        {
            Timeout = timeout;
        }
    }
}
