// -----------------------------------------------------------------------
// <copyright file="ModelPFClient.cs" company="Seiko Epson Corporation">
// Copyright(C) Seiko Epson Corporation 2026. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Epson.RoboticsShared.Modbus.Client;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using VanguardModelPFScrewdriver.ProjectFileEditor;
using static VanguardModelPFScrewdriver.ModelPF.ModelPFRegister;

namespace VanguardModelPFScrewdriver.ModelPF
{
    /// <summary>
    /// PRO-FUSE client.
    /// </summary>
    public class ModelPFClient
    {
        /// <summary>
        /// Gets a value indicating whether the client is connected.
        /// </summary>
        public bool IsConnected => (_client != null);

        /// <summary>
        /// Gets the TCP port number currently used for the connection.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Modbus client.
        /// </summary>
        private IModbusClient? _client;

        /// <summary>
        /// TCP client.
        /// </summary>
        private TcpClient? _tcpClient;

        /// <summary>
        /// Default Modbus port number.
        /// </summary>
        private const int _defaultModbusPort = 502;

        /// <summary>
        /// Connects to the device.
        /// </summary>
        /// <param name="ipAddress">IP address of the device.</param>
        /// <returns>True if the connection was established; otherwise false.</returns>
        public async Task<bool> ConnectAsync(
            IPAddress ipAddress,
            CancellationToken cancellationToken = default
        )
        {
            _tcpClient = new();

            try
            {
                Port = _defaultModbusPort;
                await _tcpClient.ConnectAsync(ipAddress, Port, cancellationToken);

                _client = ModbusClientFactory.Create(_tcpClient);

                // Under Zscaler, Connect succeeds even for an endpoint that is not listening,
                // so perform an actual read to verify the connection.
                // (This part depends on the device.)
                _ = await _client.ReadInputRegistersAsync(0, 1, cancellationToken: cancellationToken);

                return true;
            }
            catch (Exception)
            {
                ErrorReporter.Report(ErrorLogEntry.ErrOp.Connect);
            }

            Disconnect();

            return false;
        }

        /// <summary>
        /// Disconnects from the device.
        /// </summary>
        public void Disconnect()
        {
            if (_client != null)
            {
                _client.Dispose();
                // The underlying _tcpClient is closed as well.
            }
            else
            {
                _tcpClient?.Dispose();
            }

            _client = null;
            _tcpClient = null;
        }

        /// <summary>
        /// Reads input registers.
        /// </summary>
        /// <param name="startAddress">First register address to read.</param>
        /// <param name="count">Number of registers to read.</param>
        /// <returns>The register values, or null if the read failed.</returns>
        public async Task<ushort[]?> ReadInputRegistersAsync(
            ushort startAddress,
            ushort count,
            CancellationToken cancellationToken = default
        )
        {
            if (_client != null)
            {
                try
                {
                    return await _client.ReadInputRegistersAsync(startAddress, count, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    Debug.Print($"ModelPFClient.ReadInputRegistersAsync Failed: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Reads holding registers.
        /// </summary>
        /// <param name="startAddress">First register address to read.</param>
        /// <param name="count">Number of registers to read.</param>
        /// <returns>The register values, or null if the read failed.</returns>
        public async Task<ushort[]?> ReadHoldingRegistersAsync(
            ushort startAddress,
            ushort count,
            CancellationToken cancellationToken = default
        )
        {
            if (_client != null)
            {
                try
                {
                    return await _client.ReadHoldingRegistersAsync(startAddress, count, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    Debug.Print($"ModelPFClient.ReadHoldingRegistersAsync Failed: {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Writes a value to a single holding register.
        /// </summary>
        /// <param name="address">Register address to write.</param>
        /// <param name="value">Value to write.</param>
        /// <returns>True if the write succeeded; otherwise false.</returns>
        public async Task<bool> WriteSingleRegisterAsync(
            ushort address,
            ushort value,
            CancellationToken cancellationToken = default
        )
        {
            if (_client != null)
            {
                try
                {
                    await _client.WriteSingleRegisterAsync(address, value, cancellationToken: cancellationToken);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.Print($"ModelPFClient.WriteSingleRegisterAsync Failed: {ex.Message}");
                }
            }

            return false;
        }

        /// <summary>
        /// Writes values to consecutive holding registers.
        /// </summary>
        /// <param name="address">First register address to write.</param>
        /// <param name="values">Values to write.</param>
        /// <returns>True if the write succeeded; otherwise false.</returns>
        public async Task<bool> WriteMultipleRegistersAsync(
            ushort address,
            ushort[] values,
            CancellationToken cancellationToken = default
        )
        {
            if (_client != null)
            {
                try
                {
                    await _client.WriteMultipleRegistersAsync(address, values, cancellationToken: cancellationToken);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.Print($"ModelPFClient: WriteMultipleRegistersAsync Failed: {ex.Message}");
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a DI value based on the current DI register and the specified program number.
        /// </summary>
        /// <param name="programNo">Program number to set.</param>
        /// <returns>The composed DI value, or null if the current DI value could not be read.</returns>
        private async Task<DIValue?> MakeDIValueAsync(
            int programNo,
            CancellationToken cancellationToken = default
        )
        {
            var regValues = await ReadHoldingRegistersAsync(Holding.DI, 1, cancellationToken: cancellationToken);
            if (regValues == null)
            {
                return null;
            }
            else
            {
                DIValue value = (DIValue)regValues[0];
                value.SetProgram(programNo);
                //Debug.Print($"DI = {(ushort)value:X4}");

                return value;
            }
        }

        /// <summary>
        /// Stops the current operation by clearing all operation bits of the DI register.
        /// </summary>
        /// <returns>True if the operation was stopped; otherwise false.</returns>
        public async Task<bool> StopOperationAsync(
            CancellationToken cancellationToken = default
        )
        {
            var regValues = await ReadHoldingRegistersAsync(Holding.DI, 1, cancellationToken: cancellationToken);
            if (regValues == null)
            {
                ErrorReporter.Report(ErrorLogEntry.ErrOp.Stop);
                return false;
            }

            var diValue = (DIValue)regValues[0];
            diValue &= ~DIValue.OPERATIONS;

            if (!await WriteSingleRegisterAsync(Holding.EnableRemote, (ushort)Remote.DI, cancellationToken: cancellationToken))
            {
                ErrorReporter.Report(ErrorLogEntry.ErrOp.Stop);
                return false;
            }

            var isSuccess = await WriteSingleRegisterAsync(Holding.DI, (ushort)diValue, cancellationToken: cancellationToken);
            if (!isSuccess)
            {
                ErrorReporter.Report(ErrorLogEntry.ErrOp.Stop);
            }

            return isSuccess;
        }

        /// <summary>
        /// Starts the specified operation with the specified program.
        /// </summary>
        /// <param name="programNo">Program number to use.</param>
        /// <param name="operation">Operation to start (TIGHTEN, LOOSEN or FREERUN).</param>
        /// <returns>True if the operation was started; otherwise false.</returns>
        public async Task<bool> StartOperationAsync(
            int programNo,
            DIValue operation,
            CancellationToken cancellationToken = default
        )
        {
            ErrorLogEntry.ErrOp op;
            switch (operation)
            {
                case DIValue.TIGHTEN:
                    op = ErrorLogEntry.ErrOp.Tighten;
                    break;
                case DIValue.LOOSEN:
                    op = ErrorLogEntry.ErrOp.Loosen;
                    break;
                case DIValue.FREERUN:
                    op = ErrorLogEntry.ErrOp.FreeRun;
                    break;

                default:
                    return false;
            }

            await StopOperationAsync(cancellationToken);

            var diValue = await MakeDIValueAsync(programNo, cancellationToken);
            if (diValue == null)
            {
                ErrorReporter.Report(op);
                return false;
            }
            diValue |= operation;

            if (operation == DIValue.TIGHTEN)
            {
                if (!await WriteSingleRegisterAsync(Holding.RequestExecute, (ushort)RequestCode.CLEAR_CAN_GET_TIGHTENING_RESULT, cancellationToken: cancellationToken))
                {
                    ErrorReporter.Report(op);
                    return false;
                }
            }

            if (!await WriteSingleRegisterAsync(Holding.EnableRemote, (ushort)Remote.DI, cancellationToken: cancellationToken))
            {
                ErrorReporter.Report(op);
                return false;
            }

            var isSuccess = await WriteSingleRegisterAsync(Holding.DI, (ushort)diValue, cancellationToken: cancellationToken);
            if (!isSuccess)
            {
                ErrorReporter.Report(op);
            }

            return isSuccess;
        }

        /// <summary>
        /// Starts a tightening operation.
        /// </summary>
        /// <param name="progarmNo">Program number to use.</param>
        /// <returns>True if the operation was started; otherwise false.</returns>
        public Task<bool> StartTighteningAsync(
            int progarmNo,
            CancellationToken cancellationToken = default
        ) => StartOperationAsync(progarmNo, DIValue.TIGHTEN, cancellationToken);

        /// <summary>
        /// Starts a loosening operation.
        /// </summary>
        /// <param name="programNo">Program number to use.</param>
        /// <returns>True if the operation was started; otherwise false.</returns>
        public Task<bool> StartLooseningAsync(
            int programNo,
            CancellationToken cancellationToken = default
        ) => StartOperationAsync(programNo, DIValue.LOOSEN, cancellationToken);

        /// <summary>
        /// Starts a free run operation.
        /// </summary>
        /// <param name="programNo">Program number to use.</param>
        /// <returns>True if the operation was started; otherwise false.</returns>
        public Task<bool> StartFreeRunAsync(
            int programNo,
            CancellationToken cancellationToken = default
        ) => StartOperationAsync(programNo, DIValue.FREERUN, cancellationToken);

        /// <summary>
        /// Reads the current DO register to check the device status.
        /// </summary>
        /// <returns>The DO value, or null if the read failed.</returns>
        public async Task<DOValue?> CheckStatusAsync(
            CancellationToken cancellationToken = default
        )
        {
            var regValues = await ReadHoldingRegistersAsync(Holding.DO, 1, cancellationToken: cancellationToken);

            if (regValues == null)
            {
                ErrorReporter.Report(ErrorLogEntry.ErrOp.CheckStatus);
                return null;
            }
            else
            {
                return (DOValue)regValues[0];
            }
        }

        /// <summary>
        /// Waits until a tightening result becomes available and retrieves it.
        /// </summary>
        /// <returns>The tightening result, or null if it could not be retrieved.</returns>
        public async Task<ResultLogEntry?> GetTighteningResultAsync(
            CancellationToken cancellationToken = default
        )
        {
            ushort[]? regValues;

            const int _maxRepeatTimes = 50;
            const int _waitMSec = 10;

            // Poll until the device reports that a tightening result is available.
            var repeatTimes = 0;
            while (true)
            {
                regValues = await ReadInputRegistersAsync(Input.CanGetTighteningResult, 1, cancellationToken);
                if (regValues == null)
                {
                    ErrorReporter.Report(ErrorLogEntry.ErrOp.GetTighteningResult);
                    return null;
                }
                else if (regValues[0] != 0)
                {
                    break;
                }
                if (repeatTimes++ >= _maxRepeatTimes)
                {
                    ErrorReporter.Report(ErrorLogEntry.ErrOp.GetTighteningResult);
                    return null;
                }
                await Task.Delay(_waitMSec, cancellationToken);
            }

            // Offsets of each field relative to the first result register.
            const int OffsetOrigin = Input.GetTightingResult;
            const int OffsetProgramNo = Input.GetTightingResult_ProgramNo - OffsetOrigin;
            const int OffsetNumTurns = Input.GetTightingResult_NumTurns - OffsetOrigin;
            const int OffsetTorque = Input.GetTightingResult_Torque - OffsetOrigin;
            const int OffsetElapsedTime = Input.GetTightingResult_ElapsedTime - OffsetOrigin;
            const int OffsetResult = Input.GetTightingResult_Result - OffsetOrigin;
            const int OffsetErrorDetail = Input.GetTightingResult_ErrorDetail - OffsetOrigin;

            const ushort count = (ushort)(Input.Reserved4 - OffsetOrigin);
            regValues = await ReadInputRegistersAsync(Input.GetTightingResult, count, cancellationToken);
            if (regValues == null)
            {
                ErrorReporter.Report(ErrorLogEntry.ErrOp.GetTighteningResult);
                return null;
            }

            return new ResultLogEntry(0)
            {
                TimeStamp = DateTime.Now,
                ProgramNo = regValues[OffsetProgramNo],
                NumTurns = (float)regValues[OffsetNumTurns] / 10f,
                Torque = (float)regValues[OffsetTorque] / 10f,
                ElapsedTime = (float)regValues[OffsetElapsedTime] / 1000f,
                Result = regValues[OffsetResult] == 0 ? "OK" : "NG",
                ErrorDetail = $"{regValues[OffsetErrorDetail]}",
            };
        }

        /// <summary>
        /// Retrieves the torque waveform data of the last operation.
        /// </summary>
        /// <param name="timeStamp">Time stamp assigned to the retrieved waveform data.</param>
        /// <returns>The torque waveform data, or null if it could not be retrieved.</returns>
        public async Task<TorqueWaveformData?> GetTorqueDataAsync(
            DateTime timeStamp,
            CancellationToken cancellationToken = default
        )
        {
            ushort[]? regValues;

            regValues = await ReadInputRegistersAsync(Input.NumTorques, 1, cancellationToken);
            if (regValues == null)
            {
                ErrorReporter.Report(ErrorLogEntry.ErrOp.GetTorqueData);
                return null;
            }
            int numTorques = regValues[0];

            regValues = await ReadHoldingRegistersAsync(Holding.SampleingAngle, 1, cancellationToken);
            if (regValues == null)
            {
                ErrorReporter.Report(ErrorLogEntry.ErrOp.GetTorqueData);
                return null;
            }
            int sampleingAngle = regValues[0];

            TorqueWaveformData data = new()
            {
                Count = numTorques,
                SamplingAngle  = sampleingAngle,
                TimeStamp = timeStamp,
                Torques = new float[numTorques],
            };

            // A single Modbus read is limited to 125 registers, so read the waveform in chunks.
            const int _maxNumRegistersPerRead = 125;
            for (var offset = 0; offset < numTorques; offset += _maxNumRegistersPerRead)
            {
                var count = (numTorques - offset > _maxNumRegistersPerRead) ? _maxNumRegistersPerRead : numTorques - offset;
                if (count == 0)
                {
                    break;
                }
                regValues = await ReadInputRegistersAsync((ushort)(Input.Torques + offset), (ushort)count, cancellationToken);
                if (regValues == null)
                {
                    ErrorReporter.Report(ErrorLogEntry.ErrOp.GetTorqueData);
                    return null;
                }
                for (var i = 0; i < count; i++)
                {
                    data.Torques[offset + i] = (float)regValues[i] / 10f;
                }
            }

            return data;
        }

        /// <summary>
        /// Issues the specified request and waits until the device reports completion.
        /// </summary>
        /// <param name="requestCode">Request code to issue.</param>
        /// <returns>True if the request completed; otherwise false.</returns>
        public async Task<bool> WaitReadyAsync(
            RequestCode requestCode,
            CancellationToken cancellationToken = default
        )
        {
            bool isSuccess;
            ushort[]? regValues;

            ErrorLogEntry.ErrOp op = requestCode switch
            {
                RequestCode.DOWNLOAD_PROGRAM => ErrorLogEntry.ErrOp.UploadProgram,
                RequestCode.UPLOAD_PROGRAM => ErrorLogEntry.ErrOp.UploadProgram,
                RequestCode.UPLOAD_SYSPARAM => ErrorLogEntry.ErrOp.UploadNetworkConfig,
                _ => ErrorLogEntry.ErrOp.Unknown,
            };

            isSuccess = await WriteSingleRegisterAsync(Holding.RequestExecute, (ushort)requestCode, cancellationToken);
            if (!isSuccess)
            {
                ErrorReporter.Report(op);
                return false;
            }

            const int _maxRepeatTimes = 20;

            // Poll the request register until it turns to DONE.
            var repeatTimes = 0;
            while (true)
            {
                regValues = await ReadHoldingRegistersAsync(Holding.RequestExecute, 1, cancellationToken);
                if (regValues == null)
                {
                    ErrorReporter.Report(op);
                    return false;
                }
                else if (regValues[0] == (ushort)RequestCode.DONE)
                {
                    break;
                }
                repeatTimes++;
                if (repeatTimes >= _maxRepeatTimes)
                {
                    ErrorReporter.Report(op);
                    return false;
                }
            }

            return true;
        }
    }
}
