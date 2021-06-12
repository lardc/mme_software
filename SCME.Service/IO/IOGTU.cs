using SCME.Types;
using SCME.Types.Commutation;
using SCME.UIServiceConfig.Properties;
using System;
using System.Collections.Generic;
using System.Threading;
using TypeBVT = SCME.Types.BVT;
using TypeGTU = SCME.Types.GTU;
using TypeSL = SCME.Types.VTM;

namespace SCME.Service.IO
{
    internal class IOGTU
    {
        private const int CAL_WAIT_TIME_MS = 5000;

        //Адаптер
        private readonly IOAdapter Adapter;
        //Коммуникация
        private readonly BroadcastCommunication Communication;
        //Текущий узел
        private readonly ushort Node;
        //Эмуляция блока
        private readonly bool IsEmulated;
        //Необходимость отрисовки графиков
        private readonly bool IsGraphRead;
        //Блок SL для тестирования IH ГОСТ
        public IOStLs SL;
        //Блок BVT для тестирования VGNT
        public IOBvt BVT;
        //Состояние подключения
        private DeviceConnectionState ConnectionState;
        //Состояние блока
        private volatile DeviceState State;
        //Параметры тестирования
        private TypeGTU.TestParameters Parameter;
        //Результаты тестирования
        private volatile TypeGTU.TestResults Result;
        //Таймаут ожиданий ответа блока
        private int Timeout;
        //Тестирование остановлено
        private volatile bool IsStopped;
        //Задержки между запросами состояния тестирования
        private const int RequestDelay = 50;
        private const int RequestDelayVGNT = 100;

        /// <summary>Инициализирует новый экземпляр класса IOGTU</summary>
        /// <param name="adapter">Адаптер</param>
        /// <param name="communication">Коммуникация</param>
        internal IOGTU(IOAdapter adapter, BroadcastCommunication communication)
        {
            Adapter = adapter;
            Communication = communication;
            Node = (ushort)Settings.Default.GateNode;
            IsEmulated = Settings.Default.GateEmulation;
            IsGraphRead = Settings.Default.GateReadGraph;
            Result = new TypeGTU.TestResults();
            //Сообщение в лог
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Info, string.Format("GTU created. Emulation mode: {0}", IsEmulated));
        }

        /// <summary>Коммутация</summary>
        internal IOCommutation ActiveCommutation
        {
            get; set;
        }

        /// <summary>Инициализация блока</summary>
        /// <param name="Enable">Доступность блока</param>
        /// <param name="timeout">Таймаут ожиданий ответа блока</param>
        /// <returns>Состояние подключения</returns>
        internal DeviceConnectionState Initialize(bool enable, int timeout)
        {
            Timeout = timeout;
            ConnectionState = DeviceConnectionState.ConnectionInProcess;
            ConnectionEvent_Fire(ConnectionState, "GTU initializing");
            //Эмуляция блока
            if (IsEmulated)
            {
                ConnectionState = DeviceConnectionState.ConnectionSuccess;
                ConnectionEvent_Fire(ConnectionState, "GTU initialized");
                return ConnectionState;
            }
            try
            {
                //Очистка предупреждений
                Warnings_Clear();
                ConnectionState = DeviceConnectionState.ConnectionSuccess;
                ConnectionEvent_Fire(ConnectionState, "GTU initialized");
            }
            catch (Exception error)
            {
                ConnectionState = DeviceConnectionState.ConnectionFailed;
                ConnectionEvent_Fire(ConnectionState, string.Format("GTU initialization error: {0}", error.Message));
            }
            return ConnectionState;
        }

        /// <summary>Деинициализация блока</summary>
        internal void Deinitialize()
        {
            //Текущее состояние блока
            DeviceConnectionState OldState = ConnectionState;
            ConnectionState = DeviceConnectionState.DisconnectionInProcess;
            ConnectionEvent_Fire(DeviceConnectionState.DisconnectionInProcess, "GTU disconnecting");
            //Остановка работы блока
            if (!IsEmulated && OldState == DeviceConnectionState.ConnectionSuccess)
                Stop();
            ConnectionState = DeviceConnectionState.DisconnectionSuccess;
            ConnectionEvent_Fire(DeviceConnectionState.DisconnectionSuccess, "GTU disconnected");
        }

        /// <summary>Готовность блока к запуску</summary>
        /// <returns>Возвращает True в случае успешной готовности блока к запуску</returns>
        internal bool IsReadyToStart()
        {
            TypeGTU.HWDeviceState DevState = (TypeGTU.HWDeviceState)ReadDeviceState();
            return !(DevState == TypeGTU.HWDeviceState.Fault || DevState == TypeGTU.HWDeviceState.Disabled || State == DeviceState.InProcess);
        }

        /// <summary>Запуск блока</summary>
        /// <param name="parameters">Параметры тестирования</param>
        /// <param name="commParameters">Параметры коммутации</param>
        /// <returns>Состояние блока</returns>
        internal DeviceState Start(TypeGTU.TestParameters parameters, TestParameters commParameters)
        {
            Parameter = parameters;
            if (State == DeviceState.InProcess)
                throw new Exception("GTU test is already started");
            IsStopped = false;
            Result = new TypeGTU.TestResults
            {
                TestTypeId = Parameter.TestTypeId
            };
            //Очистка предупреждений
            Warnings_Clear();
            //Эмуляция блока
            if (IsEmulated)
            {
                //Проведение тестирования
                Measurement_Start(commParameters);
                return State;
            }
            TypeGTU.HWDeviceState DevState = (TypeGTU.HWDeviceState)ReadDeviceState();
            //Состояние ошибки 
            if (DevState == TypeGTU.HWDeviceState.Fault)
            {
                TypeGTU.HWFaultReason FaultReason = (TypeGTU.HWFaultReason)ReadFaultReason();
                NotificationEvent_Fire(FaultReason, TypeGTU.HWWarningReason.None, TypeGTU.HWDisableReason.None, TypeGTU.HWProblemReason.None);
                throw new Exception(string.Format("GTU is in fault state, reason: {0}", FaultReason));
            }
            //Состояние выключения
            if (DevState == TypeGTU.HWDeviceState.Disabled)
            {
                TypeGTU.HWDisableReason DisableReason = (TypeGTU.HWDisableReason)ReadDisableReason();
                NotificationEvent_Fire(TypeGTU.HWFaultReason.None, TypeGTU.HWWarningReason.None, DisableReason, TypeGTU.HWProblemReason.None);
                throw new Exception(string.Format("GTU is in disabled state, reason: {0}", DisableReason));
            }
            //Проведение тестирования
            Measurement_Start(commParameters);
            return State;
        }

        /// <summary>Остановка блока</summary>
        internal void Stop()
        {
            CallAction(ACT_STOP_TEST);
            BVT.CallAction(ACT_STOP_VGNT);
            IsStopped = true;
            State = DeviceState.Stopped;
        }

        private void Measurement_Start(TestParameters commutation) //Проведение тестирования
        {
            try
            {
                State = DeviceState.InProcess;
                AllEvents_Fire(State);
                //Переключение коммутации
                DeviceState DevState = ActiveCommutation.Switch(CommutationMode.Gate, commutation.CommutationType, commutation.Position);
                //Ошибка при переключении коммутации
                if (DevState == DeviceState.Fault)
                {
                    State = DeviceState.Fault;
                    AllEvents_Fire(State);
                    return;
                }
                if (Kelvin_Start())
                {
                    Resistance_Start();
                    VGT_Start();
                    if (!Parameter.UseIhGost)
                        IH_Start();
                    else
                        IHGOST_Start();
                    IL_Start();
                    VGNT_Start();
                }
                DevState = ActiveCommutation.Switch(CommutationMode.None);
                //Ошибка при переключении коммутации
                if (DevState == DeviceState.Fault)
                {
                    State = DeviceState.Fault;
                    AllEvents_Fire(State);
                    return;
                }
                State = IsStopped ? DeviceState.Stopped : DeviceState.Success;
                AllEvents_Fire(State);
            }
            catch (Exception error)
            {
                ActiveCommutation.Switch(CommutationMode.None);
                State = DeviceState.Fault;
                AllEvents_Fire(State);
                ExceptionEvent_Fire(error.Message);
                throw new Exception("GTU error");
            }
        }

        private bool Kelvin_Start() //Запуск прозвонки
        {
            if (IsStopped)
                return false;
            KelvinEvent_Fire(DeviceState.InProcess, Result);
            CallAction(ACT_START_KELVIN);
            //Эмуляция блока
            if (IsEmulated)
            {
                Result.IsKelvinOk = true;
                KelvinEvent_Fire(DeviceState.Success, Result);
                return Result.IsKelvinOk;
            }
            //Ожидание окончания тестирования
            EndOfTest_Wait();
            Result.IsKelvinOk = ReadRegister(REG_RESULT_KELVIN) != 0;
            //Необходимость отрисовки графиков
            if (IsGraphRead)
            {
                Result.ArrayKelvin.Add((short)ReadRegister(REG_KELVIN_1_2));
                Result.ArrayKelvin.Add((short)ReadRegister(REG_KELVIN_4_1));
                Result.ArrayKelvin.Add((short)ReadRegister(REG_KELVIN_1_4));
                Result.ArrayKelvin.Add((short)ReadRegister(REG_KELVIN_3_2));
            }
            KelvinEvent_Fire(DeviceState.Success, Result);
            return Result.IsKelvinOk;
        }

        private void Resistance_Start() //Сопротивление
        {
            if (IsStopped)
                return;
            ResistanceEvent_Fire(DeviceState.InProcess, Result);
            CallAction(ACT_START_RG);
            //Эмуляция блока
            if (IsEmulated)
            {
                Result.Resistance = 5;
                ResistanceEvent_Fire(DeviceState.Success, Result);
                return;
            }
            //Ожидание окончания тестирования
            EndOfTest_Wait();
            Result.Resistance = ReadRegister(REG_RESULT_RG) / 10.0f;
            ResistanceEvent_Fire(DeviceState.Success, Result);
        }

        private void VGT_Start() //VGT
        {
            if (IsStopped)
                return;
            VGTEvent_Fire(DeviceState.InProcess, Result);
            WriteRegister(REG_GATE_VGT_PURE, (ushort)(Parameter.IsCurrentEnabled ? 1 : 0));
            WriteRegister(REG_SCOPE_RATE, 1);
            CallAction(ACT_START_GATE);
            //Эмуляция блока
            if (IsEmulated)
            {
                Result.IGT = 170;
                Result.VGT = 990;
                VGTEvent_Fire(DeviceState.Success, Result);
                return;
            }
            //Ожидание окончания тестирования
            EndOfTest_Wait();
            Result.IGT = ReadRegister(REG_RESULT_IGT);
            Result.VGT = ReadRegister(REG_RESULT_VGT);
            //Необходимость отрисовки графиков
            if (IsGraphRead)
            {
                Result.ArrayIGT = ReadArrayFastS(EP16_Data_Ig);
                Result.ArrayVGT = ReadArrayFastS(EP16_Data_Vg);
            }
            VGTEvent_Fire(DeviceState.Success, Result);
        }

        private void IH_Start() //IH
        {
            if (IsStopped)
                return;
            if (!Parameter.IsIhEnabled)
                return;
            IHEvent_Fire(DeviceState.InProcess, Result);
            WriteRegister(REG_HOLD_USE_STRIKE, (ushort)(Parameter.IsIhStrikeCurrentEnabled ? 1 : 0));
            CallAction(ACT_START_IH);
            //Эмуляция блока
            if (IsEmulated)
            {
                Result.IH = 32;
                IHEvent_Fire(DeviceState.Success, Result);
                return;
            }
            //Ожидание окончания тестирования
            EndOfTest_Wait();
            Result.IH = ReadRegister(REG_RESULT_IH);
            //Необходимость отрисовки графиков
            if (IsGraphRead)
                Result.ArrayIH = ReadArrayFastS(EP16_Data_Vg);
            IHEvent_Fire(DeviceState.Success, Result);
        }

        //TODO!!!
        private void IHGOST_Start() //IH ГОСТ
        {
            if (IsStopped)
                return;
            if (!Parameter.IsIhEnabled)
                return;
            IHEvent_Fire(DeviceState.InProcess, Result);
            ActiveCommutation.CallAction(ACT_COMM2_GATE_SL);
            WriteRegister(REG_HOLD_WITH_SL, 1);
            CallAction(ACT_START_IH);

            SL.WriteRegister(154, 1);
            SL.WriteRegister(161, 1);
            SL.WriteRegister(162, 1);
            SL.WriteRegister(163, 1);
            SL.WriteRegister(128, 1);
            SL.WriteRegister(140, Parameter.Itm);
            SL.WriteRegister(141, 10000);
            SL.WriteRegister(160, 1);
            SL.CallAction(100);
            
            //Эмуляция блока
            if (IsEmulated)
            {
                Result.IH = 32;
                IHEvent_Fire(DeviceState.Success, Result);
                return;
            }
            //Ожидание окончания тестирования IH ГОСТ
            EndOfIHGOST_Wait();
            Result.IH = ReadRegister(REG_RESULT_IH);
            //Необходимость отрисовки графиков
            if (IsGraphRead)
                Result.ArrayIH = ReadArrayFastS(EP16_Data_Vg);
            WriteRegister(REG_HOLD_WITH_SL, 0);
            
            SL.WriteRegister(154, 0);
            
            IHEvent_Fire(DeviceState.Success, Result);
        }

        private void IL_Start() //IL
        {
            if (IsStopped)
                return;
            if (!Parameter.IsIlEnabled)
                return;
            ILEvent_Fire(DeviceState.InProcess, Result);
            CallAction(ACT_START_IL);
            //Эмуляция блока
            if (IsEmulated)
            {
                Result.IL = 270;
                ILEvent_Fire(DeviceState.Success, Result);
                return;
            }
            //Ожидание окончания тестирования
            EndOfTest_Wait();
            Result.IL = ReadRegister(REG_RESULT_IL);
            ILEvent_Fire(DeviceState.Success, Result);
        }

        //TODO!!!
        private void VGNT_Start() //VGNT
        {
            if (IsStopped)
                return;
            if (!Parameter.UseVgnt)
                return;
            VGNTEvent_Fire(DeviceState.InProcess, Result);
            ActiveCommutation.CallAction(ACT_COMM2_VGNT);
            BVT.WriteRegister(IOBvt.REG_MEASUREMENT_TYPE, 3);
            BVT.WriteRegister(IOBvt.REG_LIMIT_CURRENT, (ushort)(Parameter.CurrentLimit * 10));
            BVT.WriteRegister(IOBvt.REG_LIMIT_VOLTAGE, Parameter.VoltageLimitD);
            BVT.WriteRegister(IOBvt.REG_VOLTAGE_PLATE_TIME, Parameter.PlateTime);
            BVT.WriteRegister(IOBvt.REG_VOLTAGE_AC_RATE, (ushort)(Parameter.RampUpVoltage * 10));
            BVT.WriteRegister(IOBvt.REG_START_VOLTAGE_AC, Parameter.StartVoltage);

            WriteRegister(REG_V_GATE_LIMIT, 12000);
            WriteRegister(REG_I_GATE_LIMIT, 1000);

            //WriteRegister(REG_V_GATE_LIMIT, m_Parameter.GateLimitV);
            //WriteRegister(REG_I_GATE_LIMIT, m_Parameter.GateLimitI);

            BVT.CallAction(IOBvt.ACT_START_TEST);
            CallAction(ACT_START_VGNT);
            //Эмуляция блока
            if (IsEmulated)
            {
                Result.IGNT = 25;
                Result.VGNT = 100;
                VGNTEvent_Fire(DeviceState.Success, Result);
            }
            //Ожидание окончания VGNT
            EndOfVGNTTest_Wait();
            Result.VGNT = ReadRegister(REG_RESULT_VGNT);
            Result.IGNT = ReadRegister(REG_RESULT_IGNT);
            VGNTEvent_Fire(DeviceState.Success, Result);
        }

        private void EndOfTest_Wait() //Ожидание окончания тестирования
        {
            //Время окончания таймаута
            int TimeStamp = Environment.TickCount + Timeout;
            //Опрос состояния блока
            while (Environment.TickCount < TimeStamp)
            {
                TypeGTU.HWDeviceState DevState = (TypeGTU.HWDeviceState)ReadDeviceState(true);
                TypeGTU.HWOperationResult OperationResult = (TypeGTU.HWOperationResult)ReadFinished(true);
                //Состояние ошибки
                if (DevState == TypeGTU.HWDeviceState.Fault)
                {
                    TypeGTU.HWFaultReason FaultReason = (TypeGTU.HWFaultReason)ReadFaultReason();
                    NotificationEvent_Fire(FaultReason, TypeGTU.HWWarningReason.None, TypeGTU.HWDisableReason.None, TypeGTU.HWProblemReason.None);
                    throw new Exception(string.Format("GTU is in fault state, reason: {0}", FaultReason));
                }
                //Состояние выключения
                if (DevState == TypeGTU.HWDeviceState.Disabled)
                {
                    TypeGTU.HWDisableReason DisableReason = (TypeGTU.HWDisableReason)ReadDisableReason();
                    NotificationEvent_Fire(TypeGTU.HWFaultReason.None, TypeGTU.HWWarningReason.None, DisableReason, TypeGTU.HWProblemReason.None);
                    throw new Exception(string.Format("GTU is in disabled state, reason: {0}", DisableReason));
                }
                //Тест в процессе выполнения
                if (OperationResult == TypeGTU.HWOperationResult.InProcess)
                {
                    Thread.Sleep(RequestDelay);
                    continue;
                }
                TypeGTU.HWWarningReason WarningReason = (TypeGTU.HWWarningReason)ReadWarning();
                TypeGTU.HWProblemReason ProblemReason = (TypeGTU.HWProblemReason)ReadProblem();
                //Имеются предупреждения
                if (WarningReason != TypeGTU.HWWarningReason.None)
                {
                    NotificationEvent_Fire(TypeGTU.HWFaultReason.None, WarningReason, TypeGTU.HWDisableReason.None, TypeGTU.HWProblemReason.None);
                    CallAction(ACT_CLEAR_WARNING);
                }
                //Имеются проблемы
                if (ProblemReason != TypeGTU.HWProblemReason.None)
                    NotificationEvent_Fire(TypeGTU.HWFaultReason.None, TypeGTU.HWWarningReason.None, TypeGTU.HWDisableReason.None, ProblemReason);
                return;
            }
            //Таймаут выполнения тестирования
            if (Environment.TickCount > TimeStamp)
            {
                ExceptionEvent_Fire("Timeout while waiting for GTU test to end");
                throw new Exception("Timeout while waiting for GTU test to end");
            }
        }

        private void EndOfIHGOST_Wait() //Ожидание окончания тестирования IH ГОСТ
        {
            //Время окончания таймаута
            int TimeStamp = Environment.TickCount + Timeout;
            //Опрос состояния блока
            while (Environment.TickCount < TimeStamp)
            {
                TypeGTU.HWDeviceState DevState = (TypeGTU.HWDeviceState)ReadDeviceState(true);
                TypeSL.HWDeviceState DevStateSL = (TypeSL.HWDeviceState)SL.ReadDeviceState(true);
                //Состояние ошибки
                if (DevState == TypeGTU.HWDeviceState.Fault)
                {
                    TypeGTU.HWFaultReason FaultReason = (TypeGTU.HWFaultReason)ReadFaultReason();
                    NotificationEvent_Fire(FaultReason, TypeGTU.HWWarningReason.None, TypeGTU.HWDisableReason.None, TypeGTU.HWProblemReason.None);
                    throw new Exception(string.Format("GTU is in fault state, reason: {0}", FaultReason));
                }
                //Состояние выключения
                if (DevState == TypeGTU.HWDeviceState.Disabled)
                {
                    TypeGTU.HWDisableReason DisableReason = (TypeGTU.HWDisableReason)ReadDisableReason();
                    NotificationEvent_Fire(TypeGTU.HWFaultReason.None, TypeGTU.HWWarningReason.None, DisableReason, TypeGTU.HWProblemReason.None);
                    throw new Exception(string.Format("GTU is in disabled state, reason: {0}", DisableReason));
                }
                if (DevStateSL != TypeSL.HWDeviceState.Charging && DevStateSL != TypeSL.HWDeviceState.InProcess)
                {
                    Thread.Sleep(RequestDelay);
                    continue;
                }
                if (DevState != TypeGTU.HWDeviceState.IH)
                {
                    Thread.Sleep(RequestDelay);
                    continue;
                }
                TypeGTU.HWWarningReason WarningReason = (TypeGTU.HWWarningReason)ReadWarning();
                TypeGTU.HWProblemReason ProblemReason = (TypeGTU.HWProblemReason)ReadProblem();
                //Имеются предупреждения
                if (WarningReason != TypeGTU.HWWarningReason.None)
                {
                    NotificationEvent_Fire(TypeGTU.HWFaultReason.None, WarningReason, TypeGTU.HWDisableReason.None, TypeGTU.HWProblemReason.None);
                    CallAction(ACT_CLEAR_WARNING);
                }
                //Имеются проблемы
                if (ProblemReason != TypeGTU.HWProblemReason.None)
                    NotificationEvent_Fire(TypeGTU.HWFaultReason.None, TypeGTU.HWWarningReason.None, TypeGTU.HWDisableReason.None, ProblemReason);
                return;
            }
            //Таймаут выполнения тестирования
            if (Environment.TickCount > TimeStamp)
            {
                ExceptionEvent_Fire("Timeout while waiting for GTU test to end");
                throw new Exception("Timeout while waiting for GTU test to end");
            }
        }

        private void EndOfVGNTTest_Wait() //Ожидание окончания тестирования VGNT
        {
            //Время окончания таймаута
            int TimeStamp = Environment.TickCount + Timeout;
            //Опрос состояния блока
            while (Environment.TickCount < TimeStamp)
            {
                TypeGTU.HWDeviceState DevState = (TypeGTU.HWDeviceState)ReadDeviceState(true);
                TypeBVT.HWDeviceState DevStateBVT = (TypeBVT.HWDeviceState)BVT.ReadDeviceState(true);
                //Состояние ошибки
                if (DevState == TypeGTU.HWDeviceState.Fault)
                {
                    TypeGTU.HWFaultReason FaultReason = (TypeGTU.HWFaultReason)ReadFaultReason();
                    NotificationEvent_Fire(FaultReason, TypeGTU.HWWarningReason.None, TypeGTU.HWDisableReason.None, TypeGTU.HWProblemReason.None);
                    throw new Exception(string.Format("GTU is in fault state, reason: {0}", FaultReason));
                }
                //Состояние выключения
                if (DevState == TypeGTU.HWDeviceState.Disabled)
                {
                    TypeGTU.HWDisableReason DisableReason = (TypeGTU.HWDisableReason)ReadDisableReason();
                    NotificationEvent_Fire(TypeGTU.HWFaultReason.None, TypeGTU.HWWarningReason.None, DisableReason, TypeGTU.HWProblemReason.None);
                    throw new Exception(string.Format("GTU is in disabled state, reason: {0}", DisableReason));
                }
                //VGNT в процессе выполнения
                if (DevState == TypeGTU.HWDeviceState.VGNT && DevStateBVT == TypeBVT.HWDeviceState.DS_InProcess)
                {
                    Thread.Sleep(RequestDelay);
                    continue;
                }
                TypeGTU.HWWarningReason WarningReason = (TypeGTU.HWWarningReason)ReadWarning();
                TypeGTU.HWProblemReason ProblemReason = (TypeGTU.HWProblemReason)ReadProblem();
                //Имеются предупреждения
                if (WarningReason != TypeGTU.HWWarningReason.None)
                {
                    NotificationEvent_Fire(TypeGTU.HWFaultReason.None, WarningReason, TypeGTU.HWDisableReason.None, TypeGTU.HWProblemReason.None);
                    CallAction(ACT_CLEAR_WARNING);
                }
                //Имеются проблемы
                if (ProblemReason != TypeGTU.HWProblemReason.None)
                    NotificationEvent_Fire(TypeGTU.HWFaultReason.None, TypeGTU.HWWarningReason.None, TypeGTU.HWDisableReason.None, ProblemReason);
                Thread.Sleep(RequestDelayVGNT);
                if (DevStateBVT == TypeBVT.HWDeviceState.DS_InProcess)
                    BVT.CallAction(IOBvt.ACT_STOP);
                return;
            }
            //Таймаут выполнения тестирования
            if (Environment.TickCount > TimeStamp)
            {
                ExceptionEvent_Fire("Timeout while waiting for GTU test to end");
                throw new Exception("Timeout while waiting for GTU test to end");
            }
        }

        //WHAT IS THIS???
        //{
        internal Tuple<ushort, ushort> PulseCalibrationGate(ushort current)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Info,
                                         string.Format("Calibrate gate, current - {0}", current));

            if (IsEmulated)
                return new Tuple<ushort, ushort>(0, 0);

            if (ActiveCommutation.Switch(Types.Commutation.CommutationMode.Gate) == DeviceState.Fault)
                return new Tuple<ushort, ushort>(0, 0);

            try
            {
                var timeout = Environment.TickCount + CAL_WAIT_TIME_MS;

                WriteRegister(REG_CAL_CURRENT, current);
                CallAction(ACT_CALIBRATE_GATE);

                var done = false;
                while (timeout > Environment.TickCount && !done)
                    done = ReadRegister(REG_DEV_STATE, true) == (ushort)Types.GTU.HWDeviceState.None;

                if (!done)
                    return new Tuple<ushort, ushort>(0, 0);

                var resultCurrent = ReadRegister(REG_RES_CAL_CURRENT);
                var resultVoltage = ReadRegister(REG_RES_CAL_VOLTAGE);

                return new Tuple<ushort, ushort>(resultCurrent, resultVoltage);
            }
            finally
            {
                ActiveCommutation.Switch(Types.Commutation.CommutationMode.None);
            }
        }

        internal ushort PulseCalibrationMain(ushort Current)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Info,
                                         string.Format("Calibrate main circuit, current - {0}", Current));

            if (IsEmulated)
                return 0;

            if (ActiveCommutation.Switch(Types.Commutation.CommutationMode.Gate) == DeviceState.Fault)
                return 0;

            try
            {
                var timeout = Environment.TickCount + CAL_WAIT_TIME_MS;

                WriteRegister(REG_CAL_CURRENT, Current);
                CallAction(ACT_CALIBRATE_HOLDING);

                var done = false;
                while (timeout > Environment.TickCount && !done)
                    done = ReadRegister(REG_DEV_STATE, true) == (ushort)Types.GTU.HWDeviceState.None;

                if (!done)
                    return 0;

                var resultCurrent = ReadRegister(REG_RES_CAL_CURRENT);

                return resultCurrent;
            }
            finally
            {
                ActiveCommutation.Switch(Types.Commutation.CommutationMode.None);
            }
        }

        internal void WriteCalibrationParams(Types.GTU.CalibrationParameters Parameters)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note,
                                         "Gate @WriteCalibrationParams begin");


            WriteRegisterS(REG_GATE_VGT_OFFSET, Parameters.GateVGTOffset, true);
            WriteRegisterS(REG_GATE_IGT_OFFSET, Parameters.GateIGTOffset, true);
            WriteRegisterS(REG_GATE_IHL_OFFSET, Parameters.GateIHLOffset, true);

            WriteRegister(REG_RG_CURRENT, Parameters.RgCurrent, true);
            WriteRegister(REG_GATE_FINE_IGT_N, Parameters.GateFineIGT_N, true);
            WriteRegister(REG_GATE_FINE_IGT_D, Parameters.GateFineIGT_D, true);
            WriteRegister(REG_GATE_FINE_VGT_N, Parameters.GateFineVGT_N, true);
            WriteRegister(REG_GATE_FINE_VGT_D, Parameters.GateFineVGT_D, true);
            WriteRegister(REG_GATE_FINE_IHL_N, Parameters.GateFineIHL_N, true);
            WriteRegister(REG_GATE_FINE_IHL_D, Parameters.GateFineIHL_D, true);

            if (!IsEmulated)
                CallAction(ACT_SAVE_TO_ROM);

            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note,
                                         "Gate @WriteCalibrationParams end");
        }

        internal TypeGTU.CalibrationParameters ReadCalibrationParams()
        {
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note,
                                         "Gate @ReadCalibrationParams begin");

            var parameters = new Types.GTU.CalibrationParameters
            {
                GateIGTOffset = ReadRegisterS(REG_GATE_IGT_OFFSET, true),
                GateVGTOffset = ReadRegisterS(REG_GATE_VGT_OFFSET, true),
                GateIHLOffset = ReadRegisterS(REG_GATE_IHL_OFFSET, true),

                RgCurrent = ReadRegister(REG_RG_CURRENT, true),
                GateFineIGT_N = ReadRegister(REG_GATE_FINE_IGT_N, true),
                GateFineIGT_D = ReadRegister(REG_GATE_FINE_IGT_D, true),
                GateFineVGT_N = ReadRegister(REG_GATE_FINE_VGT_N, true),
                GateFineVGT_D = ReadRegister(REG_GATE_FINE_VGT_D, true),
                GateFineIHL_N = ReadRegister(REG_GATE_FINE_IHL_N, true),
                GateFineIHL_D = ReadRegister(REG_GATE_FINE_IHL_D, true),
            };

            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note,
                                         "Gate @ReadCalibrationParams end");

            return parameters;
        }
        //}

        #region Standart API
        internal void ClearFaults() //Очистка ошибок блока
        {
            CallAction(ACT_CLEAR_FAULT);
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note, "GTU faults cleared");
        }

        internal void Warnings_Clear() //Очистка предупреждений блока
        {
            CallAction(ACT_CLEAR_WARNING);
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note, "GTU warnings cleared");
        }

        /// <summary>Чтение значения регистра</summary>
        /// <param name="address">Адрес регистра</param>
        /// <param name="skipJournal">Пропускает запись в лог, если значение True</param>
        /// <returns>Значение регистра</returns>
        internal ushort ReadRegister(ushort address, bool skipJournal = false)
        {
            ushort Value = 0;
            if (!IsEmulated)
                Value = Adapter.Read16(Node, address);
            if (!skipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note, string.Format("GTU @ReadRegister, address {0}, value {1}", address, Value));
            return Value;
        }

        /// <summary>Чтение значения регистра со знаком</summary>
        /// <param name="address">Адрес регистра</param>
        /// <param name="skipJournal">Пропускает запись в лог, если значение True</param>
        /// <returns>Значение регистра</returns>
        internal short ReadRegisterS(ushort address, bool SkipJournal = false)
        {
            short Value = 0;
            if (!IsEmulated)
                Value = Adapter.Read16S(Node, address);
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note, string.Format("GTU @ReadRegisterS, address {0}, value {1}", address, Value));
            return Value;
        }

        /// <summary>Чтение состояния блока</summary>
        /// <param name="skipJournal">Пропускает запись в лог, если значение True</param>
        /// <returns>Значение регистра</returns>
        internal ushort ReadDeviceState(bool skipJournal = false)
        {
            return ReadRegister(REG_DEV_STATE, skipJournal);
        }

        /// <summary>Чтение ошибок</summary>
        /// <param name="skipJournal">Пропускает запись в лог, если значение True</param>
        /// <returns>Значение регистра</returns>
        internal ushort ReadFaultReason(bool skipJournal = false)
        {
            return ReadRegister(REG_FAULT_REASON, skipJournal);
        }

        /// <summary>Чтение предупреждений</summary>
        /// <param name="skipJournal">Пропускает запись в лог, если значение True</param>
        /// <returns>Значение регистра</returns>
        internal ushort ReadWarning(bool skipJournal = false)
        {
            return ReadRegister(REG_WARNING, skipJournal);
        }

        /// <summary>Чтение выключения</summary>
        /// <param name="skipJournal">Пропускает запись в лог, если значение True</param>
        /// <returns>Значение регистра</returns>
        internal ushort ReadDisableReason(bool skipJournal = false)
        {
            return ReadRegister(REG_DISABLE_REASON, skipJournal);
        }

        /// <summary>Чтение проблем</summary>
        /// <param name="skipJournal">Пропускает запись в лог, если значение True</param>
        /// <returns>Значение регистра</returns>
        internal ushort ReadProblem(bool skipJournal = false)
        {
            return ReadRegister(REG_PROBLEM, skipJournal);
        }

        /// <summary>Чтение результата тестирования</summary>
        /// <param name="skipJournal">Пропускает запись в лог, если значение True</param>
        /// <returns>Значение регистра</returns>
        internal ushort ReadFinished(bool skipJournal = false)
        {
            return ReadRegister(REG_TEST_FINISHED, skipJournal);
        }

        /// <summary>Запись значения в регистр</summary>
        /// <param name="address">Адрес регистра</param>
        /// <param name="value">Значение</param>
        /// <param name="skipJournal">Пропускает запись в лог, если значение True</param>
        internal void WriteRegister(ushort address, ushort value, bool skipJournal = false)
        {
            if (!skipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note, string.Format("GTU @WriteRegister, address {0}, value {1}", address, value));
            if (IsEmulated)
                return;
            Adapter.Write16(Node, address, value);
        }

        /// <summary>Запись значения со знаком в регистр</summary>
        /// <param name="address">Адрес регистра</param>
        /// <param name="value">Значение</param>
        /// <param name="skipJournal">Пропускает запись в лог, если значение True</param>
        internal void WriteRegisterS(ushort address, short value, bool skipJournal = false)
        {
            if (!skipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note, string.Format("GTU @WriteRegisterS, address {0}, value {1}", address, value));
            if (IsEmulated)
                return;
            Adapter.Write16S(Node, address, value);
        }

        /// <summary>Чтение эндпоинтов</summary>
        /// <param name="address">Адрес эндпоинтов</param>
        /// <returns>Значения эндпоинтов</returns>
        private IList<short> ReadArrayFastS(ushort address)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note, string.Format("GTU @ReadArrayFastS, endpoint {0}", address));
            if (IsEmulated)
                return new List<short>();
            return Adapter.ReadArrayFast16S(Node, address);
        }

        /// <summary>Вызов функции</summary>
        /// <param name="action">Адрес функции</param>
        internal void CallAction(ushort action)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note, string.Format("GTU @Call, action {0}", action));
            if (IsEmulated)
                return;
            Adapter.Call(Node, action);
        }
        #endregion

        #region Events
        private void ConnectionEvent_Fire(DeviceConnectionState state, string message) //Оповещение о подключении
        {
            Communication.PostDeviceConnectionEvent(ComplexParts.Gate, state, message);
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Info, message);
        }

        private void AllEvents_Fire(DeviceState state) //Оповещение о тестировании
        {
            Communication.PostGateAllEvent(state);
            if (state == DeviceState.Stopped)
                Communication.PostTestAllEvent(state, "GTU manual stop");
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Info, string.Format("GTU test state {0}", state));
        }

        private void KelvinEvent_Fire(DeviceState state, TypeGTU.TestResults result) //Оповещение о прозвонке
        {
            string Message = string.Format("GTU Kelvin state {0}", state);
            if (state == DeviceState.Success)
                Message = string.Format("GTU Kelvin is {0}", result.IsKelvinOk);
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Info, Message);
            Communication.PostGateKelvinEvent(state, result);
        }

        private void ResistanceEvent_Fire(DeviceState state, TypeGTU.TestResults result) //Оповещение о сопротивлении
        {
            string Message = string.Format("GTU resistance state {0}", state);
            if (state == DeviceState.Success)
                Message = string.Format("GTU resistance {0} Ohm", result.Resistance);
            Communication.PostGateResistanceEvent(state, result);
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Info, Message);
        }

        private void VGTEvent_Fire(DeviceState state, TypeGTU.TestResults result) //Оповещение о VGT
        {
            string Message = string.Format("GTU VGT state {0}", state);
            if (state == DeviceState.Success)
                Message = string.Format("GTU IGT {0} mA, VGT {1} mV", Result.IGT, Result.VGT);
            Communication.PostGateGateEvent(state, result);
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Info, Message);
        }

        private void IHEvent_Fire(DeviceState state, TypeGTU.TestResults result) //Оповещение о IH
        {
            string Message = string.Format("GTU IH state {0}", state);
            if (state == DeviceState.Success)
                Message = string.Format("Gate IH {0} mA", result.IH);
            Communication.PostGateIhEvent(state, result);
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Info, Message);
        }

        private void ILEvent_Fire(DeviceState state, TypeGTU.TestResults result) //Оповещение о IL
        {
            string Message = string.Format("GTU IL state {0}", state);
            if (state == DeviceState.Success)
                Message = string.Format("GTU IL {0} mA", result.IL);
            Communication.PostGateIlEvent(state, result);
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Info, Message);
        }

        private void VGNTEvent_Fire(DeviceState state, TypeGTU.TestResults result) //Оповещение о VGNT
        {
            string Message = string.Format("GTU VGNT state {0}", state);
            if (state == DeviceState.Success)
                Message = string.Format("GTU IGNT {0} mA, Gate VGNT {1} mV", result.IGNT, result.VGNT);
            Communication.PostGateVgntEvent(state, result);
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Info, Message);
        }

        private void NotificationEvent_Fire(TypeGTU.HWFaultReason fault, TypeGTU.HWWarningReason warning, TypeGTU.HWDisableReason disable, TypeGTU.HWProblemReason problem)
        {
            Communication.PostGateNotificationEvent(problem, warning, fault, disable);
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Warning, string.Format("GTU device notification: problem {0}, warning {1}, fault {2}, disable {3}", problem, warning, fault, disable));
        }

        private void ExceptionEvent_Fire(string message) //Оповещение об исключении
        {
            Communication.PostExceptionEvent(ComplexParts.Gate, message);
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Error, message);
        }
        #endregion

        #region Registers
        internal const ushort
            //Функции
            ACT_CLEAR_FAULT = 3,
            ACT_CLEAR_WARNING = 4,
            ACT_START_KELVIN = 100,
            ACT_START_TEST = 100,
            ACT_START_GATE = 101,
            ACT_STOP_VGNT = 101,
            ACT_START_IH = 102,
            ACT_START_IL = 103,
            ACT_START_RG = 104,
            ACT_STOP_TEST = 105,
            ACT_START_VGNT = 106,
            ACT_COMM2_GATE_SL = 116,
            ACT_COMM2_VGNT = 117,
            //Регистры
            REG_GATE_VGT_PURE = 128,
            REG_MEASUREMENT_TYPE = 128,
            REG_HOLD_USE_STRIKE = 129,
            REG_LIMIT_CURRENT = 130,
            REG_HOLD_WITH_SL = 130,
            REG_LIMIT_VOLTAGE = 131,
            REG_VOLTAGE_PLATE_TIME = 132,
            REG_V_GATE_LIMIT = 133,
            REG_VOLTAGE_AC_RATE = 133,
            REG_I_GATE_LIMIT = 134,
            REG_START_VOLTAGE_AC = 134,
            REG_SCOPE_RATE = 150,
            REG_DEV_STATE = 192,
            REG_FAULT_REASON = 193,
            REG_DISABLE_REASON = 194,
            REG_WARNING = 195,
            REG_PROBLEM = 196,
            REG_TEST_FINISHED = 197,
            REG_RESULT_KELVIN = 198,
            REG_RESULT_IGT = 199,
            REG_RESULT_VGT = 200,
            REG_RESULT_IH = 201,
            REG_RESULT_IL = 202,
            REG_RESULT_RG = 203,
            REG_RESULT_VGNT = 205,
            REG_RESULT_IGNT = 206,
            REG_KELVIN_1_2 = 211,
            REG_KELVIN_4_1 = 212,
            REG_KELVIN_1_4 = 213,
            REG_KELVIN_3_2 = 214,
            //Эндпоинты
            EP16_Data_Vg = 1,
            EP16_Data_Ig = 2,

            REG_GATE_FINE_IHL_N = 33,
            REG_GATE_FINE_IHL_D = 34,
            REG_GATE_IHL_OFFSET = 35,
            REG_GATE_FINE_IGT_N = 50,
            REG_GATE_FINE_IGT_D = 51,
            REG_GATE_FINE_VGT_N = 52,
            REG_GATE_FINE_VGT_D = 53,
            REG_GATE_VGT_OFFSET = 56,
            REG_GATE_IGT_OFFSET = 57,
            REG_RG_CURRENT = 93,
            ACT_CALIBRATE_GATE = 110,
            ACT_CALIBRATE_HOLDING = 111,
            REG_CAL_CURRENT = 140,
            ACT_SAVE_TO_ROM = 200,
            REG_RES_CAL_CURRENT = 204,
            REG_RES_CAL_VOLTAGE = 205;
        #endregion
    }
}